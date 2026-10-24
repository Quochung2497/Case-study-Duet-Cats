# Duet Cats Playable — File Reference

This reference describes the current source on `build/refactor-and-improvement`. It covers the C# files under `Assets/Script`, grouped by responsibility, with their main methods and direct dependencies. Read it alongside [Runtime System Flows](SYSTEM_FLOWS.md). The original submission baseline is fixed at commit `8a1dfad562b9c61ab1b47434302765bff30f25f3`; the moving `Main` branch may contain later improvements.

Paths below are relative to `Assets/Script`. `PlayerControls.cs` is generated from the Input Actions asset; third-party Spine source is outside this inventory.

Cross-checked against the current source and the saved scene/settings/prefabs. Dependencies list the principal contracts, data and APIs rather than every framework import.

## 1. Scene setup and dependency injection

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Installer/PlayableInstaller.cs` | Composition root holding settings, prefab/scene references and song choices. Awake at -1100 validates configuration and caches the selected song before Injector. Providers expose individual contracts/assets; game-state and host providers share one GameManager. Invalid startup deactivates scene roots and requests Editor Play Mode exit. Editor helpers synchronize IDs, enforce a single new checkbox selection and validate. | PlayableSettings, SongDefinition, scene references, GameManager, DI attributes, reflection and Editor-only HashSet selection history. |
| `Installer/InputInstaller.cs` | Initializes injected input in Awake and shuts it down in OnDestroy. | IInputReader, DI attributes and Unity lifecycle. |
| `Utility/DependencyInjection/Injector.cs` | Runs at -1000. Discovers active scene providers, registers Provide results by exact return type and injects fields/methods. Rejects a duplicate return type before invoking the second method, reporting both methods and scene-object identities. Defines InjectAttribute/ProvideAttribute. Also contains property-resolution code, although InjectAttribute currently permits only fields/methods. | IDependencyProvider, reflection, active scene MonoBehaviours and `Singleton<Injector>`. |
| `Utility/DependencyInjection/IDependencyProvider.cs` | Provider-discovery marker with no methods. | No gameplay dependencies. |
| `Installer/IInstaller.cs` | AwakeInitialize/StartInitialize contract. Cat/Note/Game installers call it from their Unity callbacks; Injector does not execute it. | Implemented by the three system installers. |
| `Installer/CatInstaller.cs` | Resolves ICatSetup from each scene Transform. Build logs a missing collider but continues; missing Spine/invalid animation settings stop that cat's build. Creates action/controller/states/FSM per cat, then configures and initializes it. | IInputReader, IResponsiveLayout, LayoutSceneRefs, ICatSetup, CatAnimationSet, cat action/states, FSM builder and Spine/Unity components. |
| `Installer/NoteInstaller.cs` | Validates song/timing/layer/prefab physics. Parses JSON once, constructs timeline/pool and passes their contracts plus playback settings into the manager. Clears the pool on destruction. | INoteManager, NoteBehaviour prefab, SongDefinition, INoteSettings, IResponsiveLayout, IScoreManager, parent Transform, NoteChart, NoteTimeline and NotePool. |
| `Installer/GameInstaller.cs` | Resolves ICatBehaviour, constructs four game states and the game FSM, then calls ConfigureStateMachine and Initialize. Passes the configured win-wait boolean into GamePlayingState. | IGameState, IGameHost, INoteManager, IScoreManager, IInputReader, IGameFlowSettings, LayoutSceneRefs, cat contracts and FSM builder. |
| `Installer/LayoutSceneRefs.cs` | Read-only container for the camera and two cat Transforms. | Unity scene references from PlayableInstaller. |

## 2. Gameplay configuration

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Settings/PlayableSettings.cs` | Root ScriptableObject embedding settings groups and referencing catalog, cat animations and input assets. Exposes individual settings/contracts to the installer. | Settings groups, SongCatalog, CatAnimationSet and InputReader. |
| `Settings/SongCatalog.cs` | Contains ISongPlaybackSettings, serializable SongDefinition and SongCatalog. Definitions pair ID, AudioClip, JSON and playback options. Get(id) rejects missing IDs and duplicate matches; scene choices select an entry. | Unity serialization, AudioClip/TextAsset and a list of definitions. |
| `Settings/GameFlowSettings.cs` | Serializable IGameFlowSettings implementation, defined in the same file. Stores Start/Result/reload durations and WaitForCatActionsOnWin. | Unity serialization; embedded in PlayableSettings. |
| `Settings/SceneVisualSettings.cs` | Portrait/landscape world LayoutProfile values and two required orientation-specific background sprites. | Unity sprites/profile data; embedded in PlayableSettings. |
| `Settings/LayoutData.cs` | Read-only runtime world geometry. LaneX, ClampCatX and SameAs support placement, dragging and change detection. | System.Math; consumed by layout, cats and notes. |
| `Settings/UIRectLayout.cs` | Serializable portrait baseline: anchors, pivot, anchored position, sizeDelta, rotation and scale. | Unity Vector2/Vector3. |
| `Settings/UIRectLayoutRules.cs` | Defines scale/position enums and serialized rules for authored parent size, available fractions, scale caps, landscape clamp and content bounds. | Unity Rect/Vector2; embedded in ResponsiveRectTransform. |
| `Settings/INoteSettings.cs` | Travel/miss timing, cat-layer name, collider radius and sprite-lookup contract. | NoteEvent, NoteVisualType and Unity Sprite. |
| `Settings/NoteSettings.cs` | Implements INoteSettings. GetNoteRadius selects by type; GetNoteSprite selects visuals by type/lane. Audio/chart data belongs to SongDefinition. | INoteSettings, note data and sprites. |
| `Settings/HitVfxSettings.cs` | Particle profiles contain count, size, lifetime and speed curve. GetHitVfx selects a profile; GetHitVfxStartColor provides side colors or a random-color gradient. | NoteVisualType and Unity particle/color value types. |
| `Settings/HitTextSettings.cs` | Font, words, color, size, duration, offset and rise settings for hit labels. | TMP font and Unity value types. |
| `Settings/ScoreSettings.cs` | Implements IScoreRules with per-type point values and GetPoints. | IScoreRules and NoteVisualType. |
| `Settings/IScoreRules.cs` | Small point-lookup contract replaceable in tests. | NoteVisualType. |
| `Settings/CatAnimationSet.cs` | Defines cues, clip groups and selection data. Groups store variants, loop flags and destination mix duration. Choose selects a clip; TryValidate checks required/duplicate cues and Spine animation names. | Unity serialization/random selection and Spine SkeletonData. |

## 3. Input

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Utility/Input/IInputReader.cs` | PointerSample and pointer Down/Moved/Up events, plus Initialize/Shutdown. | C# events and scalar pointer coordinates; independent of Input System. |
| `Utility/Input/InputReader.cs` | ScriptableObject adapter for generated input callbacks. Enables/disposes actions, tracks touch IDs/mouse state and suppresses emulated mouse briefly after touch. | IInputReader, PlayerControls, Input System and touch/mouse controls. |
| `Utility/PlayerControls.cs` | Generated wrapper for Duet-Cats-Input.inputactions, Gameplay actions and callbacks. Edit bindings in the asset and regenerate. | Input System and the Input Actions asset. |

## 4. Viewport, responsive layout and background

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `UI/IViewport.cs` | Changed(bool isPortrait), HasViewport and IsPortrait contract. | C# Action. |
| `UI/ViewportController.cs` | Caches root Canvas/camera. Central Update compares Canvas size plus camera pixel rect, aspect, orthographic size and position. Checks nonzero XY world scale for readiness and publishes orientation when ready/changed. | IViewport and Unity Canvas/RectTransform/Camera. |
| `UI/IResponsiveLayout.cs` | Current geometry, Changed event, RecalculateLayout and ScreenToWorldX contract. | LayoutData and Action. |
| `UI/ResponsiveLayout.cs` | Listens to viewport changes, selects a profile, calls LayoutMath.Calculate, places cats and emits changed geometry. Converts input to world X and draws Editor gizmos. | IResponsiveLayout, IViewport, SceneVisualSettings, LayoutSceneRefs, LayoutData, LayoutMath and Unity geometry. |
| `UI/LayoutMath.cs` | Calculate remains the world bounds/lanes/heights helper. Retained custom UI helpers GetRectInLocalSpace/CalculateUI implement group bounds, fit, offsets and clamping; the selected Canvas Scaler Expand/anchor setup does not need those UI helpers. | Layout profile/data, UIRectLayout/UIRectLayoutRules, System.Math and Unity math structs; no scene lookup. |
| `UI/ResponsiveRectTransform.cs` | Retained custom group-layout adapter: geometry reads, IViewport subscription, math/apply and Editor capture/preview. Disabled on both Status and CTA in the saved scene at commit eb3491c, rechecked 2026-10-24. The component, rules and calculation source are retained intentionally. | IViewport, UIRectLayout, UIRectLayoutRules, LayoutMath and Unity Canvas/transforms. |
| `Behaviour/BackgroundBehaviour.cs` | Keeps both portrait/landscape sprites. IViewport selects the orientation; ApplyBackground centers the SpriteRenderer and uniformly covers the camera using the larger axis ratio, with cropping of excess. UI Canvas Scaler does not replace this responsibility. | IViewport, SceneVisualSettings, LayoutSceneRefs, SpriteRenderer and orthographic camera. |

The saved UI uses Canvas Scaler Expand (1080 x 2280) and authored anchors/sizes; no Aspect Ratio Fitter is required on the complete Status/CTA groups. The retained custom adapter is inactive on both groups. [UI Layout Setup](UI_LAYOUT_CALCULATIONS.md) distinguishes the active setup from the retained custom calculation reference. Viewport/world layout/background and UI visibility/animation remain separate responsibilities.

## 5. Game session

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Behaviour/GameManager.cs` | Implements IGameState/IGameHost. Receives an installer-built FSM, routes input/note events, ticks with unscaled delta time, stores Won and publishes UI events. Owns delayed scene reload. | `IStateMachine<GameState>`, Start/Playing/Cta event states, INoteManager, IInputReader, IGameFlowSettings and Unity SceneManager. |
| `Behaviour/IGameState.cs` | State/Won and StateChanged/StartTransitionBegan for consumers; ConfigureStateMachine/Initialize for setup. | GameState, game states/FSM, INoteManager, IInputReader and IGameFlowSettings. |
| `Control/Game/IGameHost.cs` | Contract for state requests: transitions, finish run, begin/restore Start UI and schedule reload. Exposes Won for Result. | GameState. |
| `Control/Game/GameStartState.cs` | Accepts the first tap, starts the UI transition and counts down using the delta supplied by the FSM. Schedules the song before requesting Playing; restores Start UI if scheduling fails. | INoteManager, IGameFlowSettings, IGameHost and `State<GameState>`. |
| `Control/Game/GamePlayingState.cs` | Enter resets score/starts cats. Loss finishes immediately. A configured boolean can defer a win until both cats report CanTransition; Update retries a pending win and Exit clears it. | IScoreManager, two ICatBehaviour contracts, IGameHost, setup boolean and `State<GameState>`. |
| `Control/Game/GameResultState.cs` | Enter stops playback, shows outcome clips and resets the countdown. Requests Cta after ResultSeconds using the delta supplied by the FSM. | INoteManager, ICatBehaviour, IGameFlowSettings, IGameHost and `State<GameState>`. |
| `Control/Game/GameCtaState.cs` | Enter hides cats. First tap requests delayed reload; later taps are ignored for that state instance. | ICatBehaviour, IGameHost and `State<GameState>`. |

## 6. Cat input and movement

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Control/Cat/ICatAction.cs` | TryBegin/TryMove/TryEnd/Cancel and IsDragging contract. | Scalar pointer/position data; no Unity dependency. |
| `Control/Cat/CatAction.cs` | Owns a pointer per cat, checks the starting half, applies world-X deltas and clamps to supplied bounds. | ICatAction and caller-supplied positions/limits. |
| `Behaviour/Cat/CatBehaviour.cs` | Input/transform/FSM adapter. Receives ICatSetup dependencies and exposes ICatBehaviour gameplay. Handles hit, Start/Stop/Result, visibility, ResetForRun and guarded subscriptions. Ticks with Time.deltaTime and chooses Idle/Playing after Hit. | IInputReader, IResponsiveLayout, ICatAction, `IStateMachine<CatState>`, Hit/Result states and Unity transforms. |
| `Behaviour/Cat/ICatSetup.cs` | Installer contract: ConfigureStateMachine receives FSM/event states; Initialize receives action, side, input and layout. | Cat FSM/states, ICatAction, IInputReader and IResponsiveLayout. |
| `Behaviour/Cat/ICatBehaviour.cs` | IsLeft/NoteHit, OnNoteHit, CanTransition, ResetForRun, StartPlaying, StopPlaying, ShowResult and SetVisible contract. | NoteVisualType and Action. |
| `Behaviour/Cat/CatAnimationController.cs` | Implements ICatAnimation. Keeps per-cat idle history, plays Spine track 0, applies destination MixDuration and ignores callbacks from replaced clips using a version counter. | ICatAnimation, CatAnimationSet, note types and Spine SkeletonAnimation. |

## 7. Cat states and shared FSM

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Control/Cat/CatState.cs` | Idle/Playing/Hit/Result enum keys. | Generic FSM and cat consumers. |
| `Control/Cat/ICatAnimation.cs` | Semantic PlayIdle/Playing/Hit/Result contract, with completion callbacks for one-shot actions. | NoteVisualType and Action. |
| `Control/Cat/CatIdleState.cs` | Plays idle on Enter and starts another clip after completion during Update. | ICatAnimation and `State<CatState>`. |
| `Control/Cat/CatPlayingState.cs` | Enter requests the Playing animation. | ICatAnimation and `State<CatState>`. |
| `Control/Cat/CatHitState.cs` | OnHit selects/replays a clip. PlayHit clears completion; its callback sets it. CanTransition reports completion. | ICatAnimation, NoteVisualType and `State<CatState>`. |
| `Control/Cat/CatResultState.cs` | SetOutcome stores the result; Enter requests its animation. | ICatAnimation and `State<CatState>`. |
| `Control/FSM/IState.cs` | State key, machine assignment, Enter/Update/Exit and advisory CanTransition contract. | `IStateMachine<TState>` and enum keys. |
| `Control/FSM/IStateMachine.cs` | CurrentStateKey, CanTransition, TransitionToState and Tick contract. | Enum keys. |
| `Control/FSM/State.cs` | Abstract base with one-time machine assignment, empty lifecycle defaults and readiness true. | IState/IStateMachine. |
| `Control/FSM/StateBuilder.cs` | Add gathers states; Build constructs a machine with the initial key. | `IState<TState>` and `StateManager<TState>`. |
| `Control/FSM/StateManager.cs` | Validates keys, assigns machines and enters the initial state. Tick updates; CanTransition delegates readiness. Explicit transitions exit/enter immediately without automatically enforcing readiness. | `IState<TState>`, Dictionary and System; no Unity dependency. |

## 8. Chart, timeline and notes

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Control/Note/NoteEvent.cs` | NoteVisualType and immutable event data: ID, spawn time, lane and type. | No Unity dependency. |
| `Control/Note/NoteChart.cs` | Parse validates custom JSON entries, unique positive IDs, lane/time/type values and sorts by spawn time then ID. | JsonUtility, NoteEvent, enum parsing and collections. |
| `Control/Note/INoteTimeline.cs` | TryTakeDue, Reset, IsDone, TravelSeconds and GetY contract. | NoteEvent; no Unity dependency. |
| `Control/Note/NoteTimeline.cs` | Holds the parsed chart/travel duration. Advances/rewinds a due-event cursor; GetY interpolates/extrapolates from elapsed song time. | NoteEvent array and scalar timing data. |
| `Behaviour/Notes/INoteManager.cs` | NoteMissed/SongFinished events and Initialize/StartSong/StopSong contract. | INoteTimeline, INotePool, ISongPlaybackSettings, INoteSettings, IResponsiveLayout and Action. |
| `Behaviour/Notes/NoteManager.cs` | Owns playback and active INoteBehaviour entries. Initialize receives prebuilt dependencies. StartSong resets the existing timeline and schedules audio 0.1 DSP seconds ahead. FixedUpdate spawns/moves notes and reports misses/completion. StopSong releases active notes. | INoteTimeline, INotePool, INoteBehaviour, ISongPlaybackSettings, INoteSettings, IResponsiveLayout and Unity audio/physics timing. |
| `Behaviour/Notes/INoteBehaviour.cs` | Note/HasNote, SetNote and Place gameplay contract; pool lifecycle is separate. | NoteEvent, INoteTimeline, LayoutData and Unity Sprite. |
| `Behaviour/Notes/NoteBehaviour.cs` | Implements gameplay/pool contracts. Initialize receives pool, score and layer name. SetNote changes data/sprite/radius; Place uses timeline Y and lane X. Trigger checks layer before ICatBehaviour/side matching, then notifies, scores and releases. | INoteBehaviour, `IPoolObject<NoteBehaviour>`, `IObjectPool<NoteBehaviour>`, IScoreManager, ICatBehaviour, INoteTimeline, LayoutData and Unity physics/rendering. |

## 9. Note object pool

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Utility/Object Pool/IPool.cs` | GetPool exposes `IObjectPool<T>`. | Unity pooling API. |
| `Utility/Object Pool/IPoolObject.cs` | SetPool/ResetForReuse/Release reusable-object contract. | Unity pooling API. |
| `Utility/Object Pool/Pool.cs` | Abstract `ObjectPool<T>` wrapper. Lifecycle callbacks instantiate, reset before activation, deactivate and destroy. Default capacity/max inactive size are 10; capacity does not prewarm instances. | `IPool<T>`, `IPoolObject<T>`, Unity pooling API and MonoBehaviour. |
| `Control/Note/INotePool.cs` | Get returns INoteBehaviour; Release accepts that contract. | INoteBehaviour. |
| `Control/Note/NotePool.cs` | Concrete pooling boundary. OnCreate instantiates under the supplied parent and initializes pool/score/layer. Release checks the concrete type and calls its guarded Release. | NoteBehaviour prefab, `Pool<NoteBehaviour>`, IScoreManager, parent Transform and Unity Object.Instantiate. |

## 10. Score

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `UI/IScoreManager.cs` | Score, ScoreChanged, AddPoints and ResetScore contract. | NoteVisualType and Action. |
| `UI/ScoreManager.cs` | Scene IScoreManager component with DisallowMultipleComponent. Injected private IScoreRules supplies points. AddPoints/ResetScore publish an argument-free change event. | IScoreRules, NoteVisualType, DI attributes and MonoBehaviour. |
| `UI/ScoreUI.cs` | Listens to score/game-state events, updates TMP and handles loss using a serialized Animator parameter name/value, defaulting to Lose/true. | IScoreManager, IGameState, TMP and parent Animator. |

## 11. Hit feedback

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Behaviour/Cat/CatHitVfx.cs` | Finds parent ICatBehaviour and listens to NoteHit. Clears/reuses a world-space ParticleSystem, applies profile properties, sets burst 0 count and plays. Shape/direction are authored in prefab modules. | ICatBehaviour, HitVfxSettings, ParticleSystem and DI attributes. |
| `Behaviour/Cat/CatHitText.cs` | Finds parent ICatBehaviour and reuses a world-space Canvas/TMP label. Chooses words avoiding immediate repeats, rises/fades in unscaled time, faces the camera and compensates inherited scale. | ICatBehaviour, HitTextSettings, LayoutSceneRefs.Camera, Canvas/TMP and Unity transforms. |

## 12. UI state visibility

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `UI/GameUIController.cs` | Owns panel visible-state lists, show delays and Start fades. ApplyState/ShowAfterDelay/FadeStartPanels/FadeOut control views using unscaled timing; shared visible panels preserve ongoing visibility/delay when appropriate. | IGameState, IGameFlowSettings, GameStateUI and coroutines. |
| `UI/GameStateUI.cs` | View applying Show/Hide, alpha and raycast changes without reading state. EnsureComponents prepares CanvasGroup/alpha target; Hide can deactivate the object. | CanvasGroup, GraphicComponentExtensions and Unity lifecycle. |

## 13. Shared and optional utilities

| File | Responsibility and main methods | Direct dependencies |
| --- | --- | --- |
| `Utility/Extensions/GameObjectExtensions.cs` | GetOrAdd retrieves/adds components; OrNull converts destroyed Unity objects to ordinary null. | GameObject/Component/Object. |
| `Utility/UI/GraphicComponentExtensions.cs` | Finds alpha targets and gets/sets alpha on CanvasGroup, UI Graphic/TMP and SpriteRenderer. Color access supports UI Graphic/TMP and SpriteRenderer; CanvasGroup exposes alpha only. | Unity UI, TMP and SpriteRenderer. |
| `Utility/Extensions/Singleton.cs` | Shared instance lookup/creation and lifecycle utility used by Injector. | Unity lifecycle and object lookup. |
| `Utility/Extensions/PrivateSingleton.cs` | Singleton variation with private Instance. Current gameplay components do not inherit it. | Unity lifecycle and object lookup. |
| `Utility/Extensions/PersistentObject.cs` | Calls DontDestroyOnLoad in Awake. | Unity scene lifecycle. |
| `Utility/UI/FadeGraphicComponent.cs` | Looping fade utility with Start/StopFadeSequence and per-target coroutines. Current gameplay code does not call it directly. | GraphicComponentExtensions and coroutines. |
| `Utility/Extensions/SubclassPicker.cs` | Editor managed-reference subclass picker/property drawer. | Unity Editor and reflection; no runtime role. |
