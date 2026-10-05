# Duet Cats Playable — Technical Overview

## 1. Overall Project Structure

This Unity WebGL playable uses one scene for portrait and landscape. `ResponsiveLayout` derives cat positions and four note lanes from the camera and layout profiles.

| Folder | Contents |
| --- | --- |
| `Assets/Scenes` | Playable scene. |
| `Assets/Game` | Art, Spine data, audio, fonts, prefabs, and JSON charts. |
| `Assets/Scriptable Object` | `PlayableSettings` and `InputReader` assets. |
| `Assets/Script/Behaviour` | Cat, note, background, and game components. |
| `Assets/Script/Control` | Cat actions/FSM, note parsing, timing, and pool. |
| `Assets/Script/Installer` | Scene setup and dependency wiring. |
| `Assets/Script/UI` and `Settings` | UI, score storage, and settings types. |
| `Assets/Script/Utility` | Input, layout, injection, pooling, animation helpers. |

`PlayableInstaller` provides scene references through `Injector`, keeping wiring out of gameplay scripts. `CatInstaller` builds each cat's action and FSM; `NoteInstaller` sets up the chart, pool, and `NoteManager`; `GameInstaller` connects them to `GameManager`. Hits update cats and score, while misses/song completion update game state. UI listens to score and state events.

## 2. Key Gameplay Systems

1. **Start:** `NoteChart` validates and sorts JSON notes at initialization. The first tap fades the tutorial; after the configured transition, `GameManager` resets score and starts the song and `Playing` state.
2. **Input:** `InputReader` converts touch/mouse actions to pointer events. Each `CatAction` claims one pointer on its side and clamps horizontal dragging to `ResponsiveLayout` bounds, allowing two simultaneous finger drags.
3. **Spawn:** `NoteManager` schedules audio and `NoteTimeline` against `AudioSettings.dspTime`. Due events take a pooled `NoteBehaviour`, set its sprite/collider radius by type, and move it along one of four lanes over the configured travel time in `FixedUpdate`.
4. **Hit and miss:** A trigger hit by the matching cat releases the note, plays eating animation, particles, and text, and adds **2** points for Normal, **5** for Strong, or **10** for Long/LolipopLong. `ScoreChanged` refreshes `ScoreUI`. A note surviving past travel time plus hit window raises `NoteMissed` and ends the one-life run; completing the chart and song wins.
5. **Result:** `GameManager` stops audio and plays win/loss animations, then enters `CTA` and hides both cats after a delay. A CTA tap reloads the scene after its configured delay.

## 3. Design Patterns and Architecture Decisions

- **Installer/DI:** `PlayableInstaller` centralizes scene references; `Injector` supplies them to cat, note, and game installers.
- **Data-driven/ScriptableObject:** JSON defines note time, lane, and type. `PlayableSettings` selects song/chart and exposes timing, layout, score, and feedback tuning without code edits.
- **Logic and interfaces:** `CatAction`, `NoteChart`, and `NoteTimeline` sit outside MonoBehaviours. `IInputReader`, `ICatAction`, and `ICatAnimation` separate input, dragging, and animation consumers from their implementations.
- **FSM/Builder:** Each cat uses `StateManager<CatState>` for `Idle`, `Playing`, `Hit`, and `Result`, assembled by `StateBuilder<CatState>`; dragging remains in `CatAction`.
- **Responsive layout/animation mapping:** `ResponsiveLayout` uses camera size and portrait/landscape profiles for one scene; `AnimationPlayer` centralizes Spine clip names behind `CatClip` values.
- **Object Pool:** `NotePool` reuses one note prefab through Unity's `ObjectPool<T>`, changing its sprite and collider radius per type instead of repeatedly instantiating notes.
- **Singleton:** `Injector` and `ScoreManager` use the shared `Singleton<T>` utility.
- **Events:** Pointer, hit, miss, song, score, and state events notify consumers without producers referencing their listeners.

## 4. Trade-offs, Simplifications, and Improvement Ideas

Personal constraints left only 1–2 days to build this playable, so I prioritized a working game loop and mobile WebGL build:

- **Game flow:** `GameManager` changes states directly. Applying the existing FSM framework to `Start`, `Playing`, `Result`, and `CTA` would isolate phase-specific transitions and timers.
- **Coupling:** Some plain C# logic and UI still use concrete classes, such as `GameStateUI` reading `GameManager`. Small interfaces for state data and events would improve isolation.
- **Notes:** All types use instant collision. Long notes could gain hold duration and begin/end handling.
- **Variants:** One song/chart, a one-miss loss, and total score keep the run short. Chart variants, song start/end points, and session duration could enable A/B tests on the same game loop.
- **CTA:** Song cards are visual only; a production build should open their destinations instead of reloading on tap.
- **Landscape UI:** Gameplay adapts, but UI was tuned mainly for portrait. Landscape-specific `RectTransform` position, size, and rotation rules would improve it across screen ratios.
