# Architecture Patterns for PSDK Integration

Common patterns for integrating Horizon Platform SDK into Android/Kotlin apps. These patterns complement the API-specific reference files.

## Service Connection Lifecycle

`HorizonServiceConnection.connect()` must be called before any SDK operation. Place it in the `Application` or main `Activity`:

### Application-Level (Recommended)

```kotlin
class MyApp : Application() {
    override fun onCreate() {
        super.onCreate()
        val scope = CoroutineScope(SupervisorJob() + Dispatchers.Main)
        HorizonServiceConnection.connect(APP_ID, applicationContext, scope)
    }
}
```

### Activity-Level

```kotlin
class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        HorizonServiceConnection.connect(APP_ID, applicationContext, lifecycleScope)
    }
}
```

**Key considerations:**
- Call `connect()` once — multiple calls are safe but unnecessary
- The `CoroutineScope` passed to `connect()` controls the service connection lifetime
- Using `lifecycleScope` ties the connection to the Activity lifecycle
- Using a `SupervisorJob()` scope in `Application` keeps it alive app-wide

## ViewModel Integration

PSDK clients are lightweight and can be instantiated in ViewModels:

```kotlin
class LeaderboardViewModel : ViewModel() {
    private val leaderboards = Leaderboards()

    private val _scores = MutableStateFlow<List<LeaderboardEntry>>(emptyList())
    val scores: StateFlow<List<LeaderboardEntry>> = _scores.asStateFlow()

    private val _error = MutableStateFlow<String?>(null)
    val error: StateFlow<String?> = _error.asStateFlow()

    fun loadScores(leaderboardName: String) {
        viewModelScope.launch {
            try {
                val entries = leaderboards.getEntries(leaderboardName, 10, LeaderboardFilterType.NONE, LeaderboardStartAt.TOP)
                _scores.value = entries
            } catch (e: LeaderboardsException) {
                _error.value = "Failed to load scores: ${e.message}"
            }
        }
    }
}
```

**Pattern notes:**
- Instantiate PSDK clients as ViewModel properties (they are stateless wrappers)
- Use `viewModelScope.launch {}` for all SDK calls (auto-cancelled on ViewModel clear)
- Expose results via `StateFlow` for Compose or `LiveData` for Views
- Catch package-specific exceptions (e.g., `LeaderboardsException`, `IapException`)

## Coroutine Scoping

All PSDK API methods are `suspend` functions. Choose the right scope:

| Scope | Use When | Lifecycle |
|-------|----------|-----------|
| `viewModelScope` | UI-triggered operations (load scores, check entitlement) | Cancelled when ViewModel is cleared |
| `lifecycleScope` | Activity/Fragment-bound work (service connection) | Cancelled when lifecycle is destroyed |
| `CoroutineScope(SupervisorJob())` | App-level background work | Must be manually cancelled |

### Error Handling Pattern

```kotlin
viewModelScope.launch {
    try {
        val result = client.someOperation()
        // Handle success
    } catch (e: HzPlatformSdkException) {
        when (e.statusCode) {
            2 -> // NotInitialized — call connect() first
            3 -> // EntitlementFailure — invalid app ID or missing entitlement
            6 -> // NetworkUnavailable — show offline message
            else -> // Log and show generic error
        }
    }
}
```

### Parallel Operations

When loading data from multiple PSDK packages:

```kotlin
viewModelScope.launch {
    try {
        val (user, achievements) = coroutineScope {
            val userDeferred = async { users.getLoggedInUser() }
            val achievDeferred = async { achievements.getAll() }
            userDeferred.await() to achievDeferred.await()
        }
        // Both loaded successfully
    } catch (e: HzPlatformSdkException) {
        // Handle failure from either call
    }
}
```

## Dependency Injection (Hilt)

If the project uses Hilt, provide PSDK clients via modules:

```kotlin
@Module
@InstallIn(SingletonComponent::class)
object PsdkModule {
    @Provides
    @Singleton
    fun provideLeaderboards(): Leaderboards = Leaderboards()

    @Provides
    @Singleton
    fun provideAchievements(): Achievements = Achievements()
}
```

Then inject into ViewModels:

```kotlin
@HiltViewModel
class GameViewModel @Inject constructor(
    private val leaderboards: Leaderboards,
    private val achievements: Achievements,
) : ViewModel() {
    // ...
}
```

**Note:** PSDK clients are lightweight — `@Singleton` is optional but avoids repeated allocation.

## Compose UI Integration

Collect ViewModel state in Composable functions:

```kotlin
@Composable
fun LeaderboardScreen(viewModel: LeaderboardViewModel = hiltViewModel()) {
    val scores by viewModel.scores.collectAsStateWithLifecycle()
    val error by viewModel.error.collectAsStateWithLifecycle()

    if (error != null) {
        Text("Error: $error", color = MaterialTheme.colorScheme.error)
    } else {
        LazyColumn {
            items(scores) { entry ->
                LeaderboardRow(entry)
            }
        }
    }
}
```

Use `collectAsStateWithLifecycle()` (from `androidx.lifecycle.compose`) to lifecycle-aware collect flows.

## Entitlement Check Pattern

Most apps should verify entitlement at startup:

```kotlin
class MainViewModel : ViewModel() {
    private val entitlements = Entitlements()

    private val _isEntitled = MutableStateFlow(false)
    val isEntitled: StateFlow<Boolean> = _isEntitled.asStateFlow()

    init {
        viewModelScope.launch {
            try {
                val result = entitlements.getIsViewerEntitled()
                _isEntitled.value = result
                if (!result) {
                    // User is not entitled — show error or exit
                }
            } catch (e: EntitlementsException) {
                _isEntitled.value = false
            }
        }
    }
}
```

Block feature access until entitlement is confirmed. This is required for most PSDK features.
