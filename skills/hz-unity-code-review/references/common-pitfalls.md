# Common Pitfalls in Unity Quest Development

This document catalogs frequently encountered mistakes in Unity code targeting Meta Quest, along with their fixes and best practices.

## GC Allocations in Update()

**Impact: Frame hitches and stuttering**

The garbage collector on Mono/IL2CPP can cause multi-millisecond pauses when it runs. In VR, even a single GC spike can cause a dropped frame and visible judder. The solution is to eliminate all heap allocations from hot paths.

### Common Allocation Sources

```csharp
// BAD: String concatenation allocates new strings
void Update()
{
    debugText.text = "FPS: " + (1f / Time.deltaTime).ToString("F1");
}

// GOOD: Use StringBuilder, reuse it across frames
private StringBuilder _sb = new StringBuilder(64);

void Update()
{
    _sb.Clear();
    _sb.Append("FPS: ");
    _sb.AppendFormat("{0:F1}", 1f / Time.deltaTime);
    debugText.text = _sb.ToString();
}
```

```csharp
// BAD: LINQ allocates enumerators, delegates, and intermediate collections
void Update()
{
    var activeEnemies = enemies.Where(e => e.IsAlive).OrderBy(e => e.Distance).ToList();
}

// GOOD: Manual iteration with cached list
private List<Enemy> _activeEnemies = new List<Enemy>(32);

void Update()
{
    _activeEnemies.Clear();
    for (int i = 0; i < enemies.Count; i++)
    {
        if (enemies[i].IsAlive)
            _activeEnemies.Add(enemies[i]);
    }
    // Sort using a cached comparison delegate
    _activeEnemies.Sort(_distanceComparer);
}
```

```csharp
// BAD: Boxing value types
void Update()
{
    object score = playerScore;  // Boxing: int -> object
    LogValue(score);
}

// GOOD: Use generic methods or specific overloads
void Update()
{
    LogValue(playerScore);  // No boxing with generic overload
}
```

## Camera.main Anti-Pattern

**Impact: Hidden FindWithTag call every access**

`Camera.main` internally calls `GameObject.FindWithTag("MainCamera")`, which is a linear search through all GameObjects. This is expensive when called every frame.

```csharp
// BAD: Camera.main called every frame
void Update()
{
    float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
    transform.LookAt(Camera.main.transform);
}

// GOOD: Cache on startup
private Transform _mainCameraTransform;

void Awake()
{
    _mainCameraTransform = Camera.main.transform;
}

void Update()
{
    float distance = Vector3.Distance(transform.position, _mainCameraTransform.position);
    transform.LookAt(_mainCameraTransform);
}
```

Note: In Unity 2020.2+, `Camera.main` is cached internally after the first call within a frame. However, caching it yourself is still best practice for clarity and backward compatibility.

## GameObject.Find and FindObjectOfType

**Impact: Linear search through the entire scene hierarchy**

These methods scan the scene hierarchy or all loaded objects. They should never be called in Update, FixedUpdate, or LateUpdate.

```csharp
// BAD: Searching every frame
void Update()
{
    var player = GameObject.Find("Player");
    var healthBar = FindObjectOfType<HealthBar>();
}

// GOOD: Cache references
private GameObject _player;
private HealthBar _healthBar;

void Start()
{
    _player = GameObject.Find("Player");
    _healthBar = FindObjectOfType<HealthBar>();
}

// BETTER: Use direct references via Inspector or dependency injection
[SerializeField] private GameObject _player;
[SerializeField] private HealthBar _healthBar;
```

## Physics on Main Thread

**Impact: Frame time spikes from physics simulation**

Heavy physics work on the main thread directly eats into your frame budget. Quest targets 72-120 Hz, leaving only 8-14ms per frame.

```csharp
// BAD: Complex physics queries every frame
void Update()
{
    // Allocates array every call
    Collider[] hits = Physics.OverlapSphere(transform.position, 50f);
    foreach (var hit in hits)
    {
        // Expensive per-collider work
    }
}

// GOOD: Use FixedUpdate, non-alloc variants, limit scope
private Collider[] _hitBuffer = new Collider[32];

void FixedUpdate()
{
    int hitCount = Physics.OverlapSphereNonAlloc(transform.position, 10f, _hitBuffer, _layerMask);
    for (int i = 0; i < hitCount; i++)
    {
        // Process _hitBuffer[i]
    }
}
```

### Physics Settings for Quest

- Set **Fixed Timestep** to match target frame rate: `1/72 = 0.01389` or `1/90 = 0.01111`
- Reduce **Default Solver Iterations** to 4-6 (default is 6)
- Reduce **Default Solver Velocity Iterations** to 1-2
- Use simplified colliders (box, sphere, capsule) instead of mesh colliders
- Configure the **Layer Collision Matrix** to disable unnecessary collision pairs

## Coroutine Allocation

**Impact: Heap allocation every time StartCoroutine is called**

Each `StartCoroutine` call allocates a coroutine object on the heap. Additionally, `yield return new WaitForSeconds()` allocates a new object each time.

```csharp
// BAD: Allocates every call
void DoAction()
{
    StartCoroutine(WaitAndExecute());
}

IEnumerator WaitAndExecute()
{
    yield return new WaitForSeconds(1f);  // Allocates
    Execute();
}

// GOOD: Cache WaitForSeconds, reuse coroutines
private WaitForSeconds _waitOneSecond = new WaitForSeconds(1f);
private Coroutine _activeCoroutine;

void DoAction()
{
    if (_activeCoroutine != null)
        StopCoroutine(_activeCoroutine);
    _activeCoroutine = StartCoroutine(WaitAndExecute());
}

IEnumerator WaitAndExecute()
{
    yield return _waitOneSecond;  // No allocation
    Execute();
}

// ALTERNATIVE: Use async/await with UniTask (no coroutine allocation)
async void DoActionAsync()
{
    await UniTask.Delay(1000);
    Execute();
}
```

## String Operations in Hot Paths

**Impact: Frequent heap allocations, GC pressure**

String operations in C# create new string objects on every modification. In hot paths, this generates significant garbage.

```csharp
// BAD: String concatenation in loops
void UpdateLeaderboard()
{
    string result = "";
    for (int i = 0; i < scores.Length; i++)
    {
        result += $"{i + 1}. {names[i]}: {scores[i]}\n";  // New string each iteration
    }
    leaderboardText.text = result;
}

// GOOD: StringBuilder
private StringBuilder _leaderboardSB = new StringBuilder(512);

void UpdateLeaderboard()
{
    _leaderboardSB.Clear();
    for (int i = 0; i < scores.Length; i++)
    {
        _leaderboardSB.Append(i + 1);
        _leaderboardSB.Append(". ");
        _leaderboardSB.Append(names[i]);
        _leaderboardSB.Append(": ");
        _leaderboardSB.Append(scores[i]);
        _leaderboardSB.Append('\n');
    }
    leaderboardText.text = _leaderboardSB.ToString();
}
```

## Unoptimized UI

**Impact: Canvas rebuilds cause CPU spikes**

Unity's UI system rebuilds the entire Canvas mesh when any element changes. This is expensive, especially with many UI elements.

```csharp
// BAD: Frequently changing elements on a single large canvas
// All 200 elements get rebuilt when the score text changes

// GOOD: Split into multiple canvases by update frequency
// - Static Canvas: background, borders, labels that never change
// - Dynamic Canvas: score, timer, health bar that update often
// - Rare Canvas: menus, settings that change infrequently
```

### UI Best Practices

- Disable `Raycast Target` on non-interactive elements (Text, Image backgrounds)
- Use `CanvasGroup` alpha instead of enabling/disabling individual elements
- Avoid `Layout Groups` on frequently updated canvases (forces full rebuild)
- Pool UI elements instead of instantiating/destroying them
- Use TextMeshPro instead of Unity's built-in Text component

```csharp
// Disable raycast target on non-interactive text
[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class NonInteractiveText : MonoBehaviour
{
    void Awake()
    {
        GetComponent<TMPro.TextMeshProUGUI>().raycastTarget = false;
    }
}
```

## Loading AudioClip in Memory

**Impact: High memory usage for large audio clips**

Loading entire audio clips into memory wastes RAM, especially for music and ambient sounds.

```csharp
// BAD: "Decompress On Load" for a 3-minute music track
// This loads the entire uncompressed clip into memory (~30 MB for stereo 44.1kHz)

// GOOD: Use "Streaming" load type for clips > 1 second
// Set in AudioClip import settings:
//   Load Type: Streaming
//   Compression Format: Vorbis
//   Quality: 50-70%
```

## Not Using Object Pooling

**Impact: GC allocations and frame spikes from Instantiate/Destroy**

Creating and destroying GameObjects at runtime causes memory allocation and GC pressure. Use object pooling for anything created frequently.

```csharp
// BAD: Instantiate and destroy projectiles
void Fire()
{
    var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    Destroy(bullet, 3f);
}

// GOOD: Object pool
public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    private Queue<GameObject> _pool = new Queue<GameObject>(32);

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject bullet;
        if (_pool.Count > 0)
        {
            bullet = _pool.Dequeue();
            bullet.transform.SetPositionAndRotation(position, rotation);
            bullet.SetActive(true);
        }
        else
        {
            bullet = Instantiate(_bulletPrefab, position, rotation);
        }
        return bullet;
    }

    public void Return(GameObject bullet)
    {
        bullet.SetActive(false);
        _pool.Enqueue(bullet);
    }
}
```

## Missing Null Checks Causing Exceptions

**Impact: Exception handling is very expensive on mobile IL2CPP**

Thrown exceptions have significant overhead on Quest, especially with IL2CPP. Prevent them with null checks instead of relying on try/catch.

```csharp
// BAD: Exception thrown if target is destroyed
void Update()
{
    try
    {
        float dist = Vector3.Distance(transform.position, target.transform.position);
    }
    catch (NullReferenceException)
    {
        target = FindNewTarget();
    }
}

// GOOD: Explicit null check
void Update()
{
    if (target == null)
    {
        target = FindNewTarget();
        return;
    }
    float dist = Vector3.Distance(transform.position, target.transform.position);
}
```

## Unnecessary Update Methods

**Impact: Empty Update() still has native-to-managed call overhead**

Unity calls Update() via a native-to-managed transition that has measurable overhead, even if the method body is empty. With hundreds of MonoBehaviours, this adds up.

```csharp
// BAD: Empty Update still incurs overhead
public class StaticDecoration : MonoBehaviour
{
    void Update() { }  // Remove this entirely
}

// BAD: Update that only runs conditionally
void Update()
{
    if (!isActive) return;
    // Actual logic here
}

// GOOD: Enable/disable the component to control Update calls
public void Activate()
{
    isActive = true;
    enabled = true;
}

public void Deactivate()
{
    isActive = false;
    enabled = false;  // Unity stops calling Update on disabled components
}
```

## Reflection in Hot Paths

**Impact: Reflection is orders of magnitude slower than direct calls**

Avoid `GetComponent`, `SendMessage`, and reflection-based patterns in Update and other per-frame methods.

```csharp
// BAD: GetComponent every frame
void Update()
{
    var rb = GetComponent<Rigidbody>();
    rb.AddForce(Vector3.up);

    // SendMessage uses reflection
    gameObject.SendMessage("TakeDamage", 10);
}

// GOOD: Cache components, use direct references
private Rigidbody _rb;
private IDamageable _damageable;

void Awake()
{
    _rb = GetComponent<Rigidbody>();
    _damageable = GetComponent<IDamageable>();
}

void Update()
{
    _rb.AddForce(Vector3.up);
    _damageable.TakeDamage(10);
}
```

### Additional Reflection Anti-Patterns

```csharp
// BAD: Type checking every frame
void Update()
{
    foreach (var obj in objects)
    {
        if (obj.GetType() == typeof(Enemy))  // Reflection
            ProcessEnemy((Enemy)obj);
    }
}

// GOOD: Use interfaces or separate lists
private List<Enemy> _enemies = new List<Enemy>();

void Update()
{
    for (int i = 0; i < _enemies.Count; i++)
    {
        ProcessEnemy(_enemies[i]);
    }
}
```

## Summary of Hot Path Rules

The following operations should NEVER appear in `Update()`, `LateUpdate()`, `FixedUpdate()`, or any method called every frame:

| Operation | Why | Alternative |
|-----------|-----|-------------|
| `Camera.main` | Hidden FindWithTag | Cache in Awake/Start |
| `GameObject.Find` | Linear scene search | Cache or serialize reference |
| `FindObjectOfType` | Searches all objects | Cache in Awake/Start |
| `GetComponent` (uncached) | Traversal + type check | Cache in Awake/Start |
| `SendMessage` | Reflection-based | Direct method call or interface |
| `Instantiate` / `Destroy` | Allocation + GC | Object pooling |
| `new List/Array` | Heap allocation | Pre-allocate and reuse |
| String concatenation | Creates new strings | StringBuilder |
| LINQ queries | Multiple allocations | Manual loops |
| `StartCoroutine` | Coroutine allocation | Cache or use UniTask |
| `try/catch` for flow control | Exception overhead | Null checks |
| `foreach` on non-List | Enumerator allocation | `for` loop with index |
