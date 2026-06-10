# Game Scripts Reference

Full source code for the VR Target Shooter tutorial scripts. Write these to `Assets/Scripts/` in the Unity project.

## Shooting.cs

Raycast shooting that works with mouse click (desktop testing) and Quest right trigger (VR).

```csharp
using UnityEngine;
using UnityEngine.XR;

public class Shooting : MonoBehaviour
{
    public float range = 100f;
    public GameObject hitEffect;
    public AudioClip shootSound;
    AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        bool triggered = Input.GetMouseButtonDown(0);

        if (!triggered)
        {
            var xrController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (xrController.isValid)
            {
                xrController.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed);
                if (triggerPressed) triggered = true;
            }
        }

        if (triggered)
        {
            if (audioSource && shootSound)
                audioSource.PlayOneShot(shootSound);

            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                if (hit.collider.CompareTag("Target"))
                {
                    if (hitEffect)
                    {
                        GameObject effect = Instantiate(hitEffect, hit.point, Quaternion.identity);
                        Destroy(effect, 1f);
                    }
                    Destroy(hit.collider.gameObject);
                    GameManager.Instance.AddScore();
                }
            }
        }
    }
}
```

**Attach to:** Main Camera (under XR Origin → Camera Offset → Main Camera in VR).

**How it works:** Each frame, checks for mouse click or VR trigger press. If triggered, casts a ray forward from the camera. If the ray hits a collider tagged "Target", destroys it and increments the score.

## GameManager.cs

Singleton that tracks and displays the score.

```csharp
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI scoreText;
    int score;

    void Awake()
    {
        Instance = this;
    }

    public void AddScore()
    {
        score++;
        if (scoreText) scoreText.text = "Score: " + score;
    }
}
```

**Attach to:** An empty GameObject named "GameManager".

**Wire:** Drag the Text (TMP) object into the `scoreText` field in the Inspector.

**Prerequisite:** TextMeshPro Essential Resources must be imported first (Window → TextMeshPro → Import TMP Essential Resources).

## TargetSpawner.cs

Spawns target cubes at random positions on a timer.

```csharp
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public GameObject targetPrefab;
    public float spawnInterval = 2f;
    public float spawnRange = 8f;

    void Start()
    {
        InvokeRepeating(nameof(Spawn), 1f, spawnInterval);
    }

    void Spawn()
    {
        float x = Random.Range(-spawnRange, spawnRange);
        float z = Random.Range(3f, spawnRange + 5f);
        float y = Random.Range(0.5f, 3f);
        Instantiate(targetPrefab, new Vector3(x, y, z), Quaternion.identity);
    }
}
```

**Attach to:** An empty GameObject named "TargetSpawner".

**Wire:** Create a prefab by dragging a Target cube into Assets/Prefabs, then assign it to the `targetPrefab` field. Ensure the prefab has the "Target" tag.

## VRMovement.cs

Thumbstick locomotion for Quest controllers.

```csharp
using UnityEngine;
using UnityEngine.XR;

public class VRMovement : MonoBehaviour
{
    public float speed = 3f;
    InputDevice leftController;

    void Update()
    {
        if (!leftController.isValid)
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (leftController.isValid && leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
        {
            Camera cam = Camera.main;
            Vector3 forward = cam.transform.forward;
            Vector3 right = cam.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 move = (forward * axis.y + right * axis.x) * speed * Time.deltaTime;
            transform.position += move;
        }
    }
}
```

**Attach to:** The XR Origin (VR) root GameObject (not the camera).

**How it works:** Reads the left controller's thumbstick, projects the movement direction based on where the player is looking (ignoring vertical tilt), and moves the XR Origin.
