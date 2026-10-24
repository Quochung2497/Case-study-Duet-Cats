# Setup and Dependencies

## 1. Editor and project packages

Clone the repository and open this project folder with **Unity 6000.3.22f1**, as recorded in ProjectSettings/ProjectVersion.txt. Install **WebGL Build Support** in Unity Hub when building for web. Let Package Manager resolve the committed manifest/lock; this setup does not upgrade packages.

TextMesh Pro code is provided by **Unity UI (com.unity.ugui 2.0.0)** in this Unity version. The ignored Assets/TextMesh Pro folder contains imported resources; restoring those resources is separate from resolving the UPM package.

## 2. Restore ignored dependencies

Git deliberately excludes Assets/Spine, Assets/Spine Examples, Assets/Editor/SpineSettings.asset and Assets/TextMesh Pro, including their root .meta files. A public clone therefore needs a dependency restore before opening the playable scene.

### Recorded Spine baseline

The local working dependency set records:

- Runtime package: **spine-unity-4.1-2024-06-19.unitypackage**; package.json reports **4.1.43**.
- Both cat skeleton JSON files in Assets/Game/Cats report Spine export **4.1.19**.
- Restore the existing Assets/Spine layout, including its runtime/editor .meta files. Do not install another runtime alongside it or replace it with the newest major/minor version by default.

For the closest reproduction, obtain the same dependency set from the project maintainer through an authorized channel. If supplied as ci-dependencies.zip, extract from the project root so the result is **Assets/Spine/...** and **Assets/TextMesh Pro/...**, with original .meta files. Avoid nested Assets/Assets folders. Keep the ZIP and access details outside Git.

For a self-managed installation, obtain the matching **4.1** package from the official [Spine downloads](https://esotericsoftware.com/spine-unity-download) and follow [runtime installation](https://esotericsoftware.com/spine-unity-installation) under its supplied license. A different patch/package must be checked against the existing skeletons and serialized references before treating it as equivalent. Examples are optional for the playable.

**Compatibility limit:** Esoteric Software's current table lists 4.1 support through Unity 2023.1. The dependency set above is this project's recorded Unity 6 build baseline; it is not a claim of official Unity 6 support for every 4.1 package. Migrating the skeletons/runtime or moving Spine into UPM is separate work.

### TMP resources

If the maintainer dependency set is unavailable, use **Window > TextMeshPro > Import TMP Essential Resources** in the Editor. Keep the imported Assets/TextMesh Pro resources and their .meta files together. Do not add an older standalone TMP package to this manifest. The menu's resource-import behavior is documented in the [TMP guide](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html); this project uses the TMP implementation bundled in Unity UI.

Keep the tracked fonts under Assets/Game/Fonts. If a font/material/shader reference is missing after a different resource import, restore the matching resource/GUID or deliberately reassign the intended asset in the Inspector. Regenerating fonts is not part of the normal clone procedure. Restore the original Spine settings and its .meta file as well when they are included in the maintainer dependency set.

## 3. Open and verify the scene

1. Open **Assets/Scenes/Case Study Scene.unity** after scripts finish compiling.
2. Check for Missing Script, unresolved font/material or missing skeleton/animation references. Confirm that both cats have their SkeletonData assets and that the existing fonts render.
3. In PlayableInstaller, keep the shared PlayableSettings and choose exactly one valid song. Play startup reports missing configuration; Injector rejects duplicate provider return Types instead of replacing a previous registration.
4. Confirm Canvas Scaler is **Scale With Screen Size / 1080 x 2280 / Expand**. ResponsiveRectTransform is already disabled on both Status Layout and CTA Panel; retain its source and serialized settings.
5. Play a short run: start, drag each cat, hit/miss, result and CTA/reload. Review portrait/landscape and text/animation extents on the target browser when making a WebGL build.

These are verification steps for the person restoring a clone, not a claim that a fresh clone or browser matrix was run during the documentation change.

## 4. CI dependency restore

The existing workflow restores a dependency ZIP before building and verifies its SHA-256. Maintainers configure CI_DEPENDENCIES_URL and CI_DEPENDENCIES_TOKEN as repository secrets, and CI_DEPENDENCIES_SHA256 as a repository variable. The GitHub token for this repository does not automatically grant access to a separate private dependency repository.

Use the configured private channel or the official import paths above for local setup. Public documentation intentionally contains no dependency download secret, token or Unity credential.
