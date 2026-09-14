# SafeArea sample

Keeps the HUD out of the notch, the camera cut-out and the Android gesture bar on any phone, in both orientations, while the background still fills the whole screen. `SafeArea.cs` goes on an empty stretch/stretch panel directly under the Canvas; every HUD element is parented to that panel. It converts `Screen.safeArea` (pixels) to normalised anchors and re-applies only when the rect changes. `SafeAreaReadout.cs` shows the live numbers on screen so you can quote them in your journal.

To use it in your own project copy `Scripts/SafeArea/` and, if you want the demo, `Scenes/SafeArea/` and `Textures/SafeArea/`. Test in Window > General > Device Simulator with a notched device first, then on your phone: cover the notch with a finger and swipe up from the bottom edge to confirm nothing sits under either. Design notes and the decision record are in the `MGD_MODULE_GDD` vault, note `Samples/SafeArea`.
