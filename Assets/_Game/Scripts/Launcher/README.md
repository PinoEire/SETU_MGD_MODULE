# Launcher

The first scene in the build. `LauncherMenu.cs` reads the scene list from Build Settings at runtime and shows one button per scene, so a new sample appears here as soon as its builder registers the scene. Tapping a button loads that scene with `await SceneManager.LoadSceneAsync(...)` and disables the list while loading. The footer prints the app version, Unity version and device model, which is the About / Build Info line every submission needs.

Every sample returns here through `Scripts/Shared/BackToLauncher.cs`: by default the Android back gesture (Escape in the Input System) loads build index 0; a sample that uses back for its own purpose, such as pausing, turns that listener off and wires a button to `Go()` instead. Design notes are in the `MGD_MODULE_GDD` vault, note `Samples/Launcher`.
