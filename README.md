# Duet Cats Playable — Technical Overview

This README describes the refactored implementation. The original submission is preserved at [commit `8a1dfad`](https://github.com/Quochung2497/Case-study-Duet-Cats/tree/8a1dfad562b9c61ab1b47434302765bff30f25f3); `Main` can include later improvements.

## 1. Overall Project Structure

One Unity WebGL scene supports portrait and landscape, with two cats and four note lanes.

| Folder | Contents |
| --- | --- |
| `Assets/Scenes` | Playable scene. |
| `Assets/Game` | Art, Spine data, audio, fonts, prefabs, and JSON charts. |
| `Assets/Scriptable Object` | Root settings, input, song catalog, and animation assets. |
| `Assets/Script/Behaviour` | Cat, note, background, and game components. |
| `Assets/Script/Control` | Game/cat FSMs, drag logic, note parsing, timeline, and pool. |
| `Assets/Script/Installer` | Scene setup and dependency wiring. |
| `Assets/Script/UI` | Views, controllers, scoring, and responsive layout. |
| `Assets/Script/Settings` | System settings, layout data, and song/animation definitions. |
| `Assets/Script/Utility` | Input, injection, generic pooling, and shared extensions. |

`PlayableSettings` holds tuning and shared assets. `PlayableInstaller` validates references/song selection before injection. `InputInstaller` owns input lifecycle; `CatInstaller` builds each cat's action, animation controller, and FSM; `NoteInstaller` builds timeline/pool; `GameInstaller` builds game FSM. `GameManager` routes events; UI and viewport controllers update presentation and layout.

## 2. Key Gameplay Systems

1. **Setup/start:** Select a song in `PlayableInstaller`; checking another clears the previous choice. Missing references or invalid song selection stop startup. `NoteChart` validates and sorts JSON once. The first tap fades the tutorial; after the transition delay, Start schedules audio and enters Playing, resetting score and starting both cats.
2. **Input:** `InputReader` converts touch/mouse actions to pointer events. Each `CatAction` owns a pointer on its side and clamps horizontal dragging to layout bounds, supporting two simultaneous touches.
3. **Spawn:** `NoteManager` uses `AudioSettings.dspTime` for audio and note timing. `INoteTimeline` returns due events; pooled notes receive type-specific sprites/radii and move along their lanes in `FixedUpdate`.
4. **Hit/miss:** A matching cat trigger plays hit feedback, awards **2** points for Normal, **5** for Strong, or **10** for Long/LolipopLong, then releases the note. `ScoreChanged` updates UI. Passing travel time plus the hit window raises a miss and ends the one-life run.
5. **Result/replay:** When the chart and audio finish with no active notes, game flow requests a win. When configured, it waits for both cats' hit animations to finish. Result stops audio and plays the outcome; after a delay, CTA hides cats and shows the cards. A CTA tap reloads the scene.
6. **Extension hooks:** `CatBehaviour.ResetForRun()` resets cat state and input without rebuilding it; timeline, note playback, and score also expose reset/restart methods. `SongCatalog` accepts more audio/chart pairs, and `INoteManager.Initialize()` accepts replacement playback dependencies. Settings expose variations in timing, scoring, feedback, animation, and layout. Complete same-scene replay and runtime selection still need coordinated game/UI flow.

## 3. Design Patterns and Architecture Decisions

- **Implementation approach:** Research relevant Unity APIs, built-in components/Inspector settings and native platform/browser support before custom code. Prefer configuration or a small integration that meets the scope; record the concrete gap and reason when custom implementation is needed.
- **Installer/DI:** Installers compose systems and pass dependencies through contracts for input, cats, notes, score, game state, and layout. Duplicate provider return types stop registration with an error naming both providers.
- **Data-driven/ScriptableObject:** `PlayableSettings` groups system settings and references song/animation assets. `SongCatalog` pairs audio, JSON, and playback options; `CatAnimationSet` maps cues to clip variants, loops, and mix durations. Particle shape/bursts are authored in prefabs.
- **FSM/Builder:** Game states (`Start`, `Playing`, `Result`, `Cta`) and cat states (`Idle`, `Playing`, `Hit`, `Result`) share the FSM framework and are built in installers. `CanTransition()` exposes action readiness.
- **Logic/presentation separation:** Drag rules and note timing sit outside MonoBehaviours. `CatAnimationController` handles Spine playback; `GameUIController` owns visibility rules, fades, and delays; `GameStateUI` applies view changes.
- **Responsive gameplay/background:** `ViewportController` publishes camera/Canvas changes; `LayoutMath.Calculate()` supplies world geometry. Background keeps separate portrait/landscape sprites and SpriteRenderer cover scaling.
- **UI layout:** Canvas Scaler **Expand** and anchors preserve the authored Status/CTA composition across portrait and landscape. See [UI Layout Setup](doc/UI_LAYOUT_CALCULATIONS.md) for details.
- **Object Pool:** `NoteInstaller` owns `NotePool`, backed by Unity's `ObjectPool<T>`. `NoteManager` uses `INotePool` and `INoteBehaviour`; prefab creation stays at the Unity boundary.
- **Singleton:** `Injector` retains `Singleton<T>`; game and score services use interfaces.
- **Events:** Pointer, hit, miss, song, score, state, and viewport events notify listeners.

## 4. Trade-offs, Simplifications, and Improvement Ideas

Personal constraints left 1–2 days for the original playable, prioritizing gameplay and mobile WebGL. The refactors above were added afterwards. Remaining limits:

- **Composition:** Reflection DI, execution order and contracts add setup/debugging cost for a small playable. The existing wiring is retained; a simpler composition root is an option when that cost outweighs its value.
- **Notes:** All types use instant collision. Long notes could gain hold duration and begin/end handling.
- **Chart data:** Custom JSON keeps only `id`, `spawnTime`, `lane`, and `visualType`. The original field meanings were unclear and weekend clarification was unavailable; mapping the original schema remains future work.
- **Runtime song switching:** Selection is captured at scene setup; there is no song-selection UI, and music resources are limited. Future switching would move selection out of the installer and connect a selection UI to playback reconfiguration.
- **Testing:** No project test cases or unit tests have been added yet.
- **Sessions:** Replay reloads the scene. Cat reset support exists, but complete same-scene replay and coordinated pause are not wired.
- **Landscape UI:** The selected Expand/anchor setup preserves portrait composition and may leave extra horizontal space. A different landscape arrangement is separate work; final padding, animation extents and phone-browser readability still need visual review. Extra safe-area handling is conditional on the browser/embedding canvas, not automatically required by a phone screen.
- **CTA:** Cards are visual; production destinations would replace the current reload-on-tap behavior.

## 5. CI/CD

- **Commit check:** A local Git hook enforces `type: description` subjects on any branch.
- **PR check:** PRs into `Main` check conflict markers, `.meta`, GUIDs and JSON, then compile/build with Unity `6000.3.22f1`.
- **Build & release:** Push `build-v*` tags to build WebGL with Brotli + fallback, publish a ZIP release and deploy GitHub Pages.

## 6. Open a Fresh Clone

Use Unity **6000.3.22f1** with WebGL Build Support. Restore the ignored Spine **4.1** runtime and TMP resources with their `.meta` files before opening `Assets/Scenes/Case Study Scene.unity`. See [Setup and Dependencies](doc/SETUP.md) for the recorded versions, official import paths and reference checks. Private dependency URLs/tokens are not required in the public README.
