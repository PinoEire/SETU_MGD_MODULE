using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Keeps this RectTransform inside <see cref="Screen.safeArea"/>: the part of the
    /// screen not covered by a notch, camera cut-out, rounded corner or the Android
    /// gesture bar.
    ///
    /// Put it on an empty, stretch/stretch panel directly under the Canvas and parent
    /// every HUD element to that panel. Backgrounds stay outside it so they still fill
    /// the whole screen.
    ///
    /// The safe area is given in pixels and can change while the app runs (rotation,
    /// folding, multi-window), so it is re-applied whenever it differs from the last
    /// value. The comparison is a plain struct compare: no allocation, cheap every frame.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public sealed class SafeArea : MonoBehaviour
    {
        RectTransform _rect;
        Rect _applied;

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != _applied)
            {
                Apply();
            }
        }

        /// <summary>
        /// Converts a pixel safe-area rect into normalised anchors (0..1). Anchors,
        /// not offsets, keep the panel correct at any resolution and Canvas Scaler
        /// setting because they are relative to the parent, not absolute. Returns
        /// false for a zero-sized screen, which the editor can report for a frame
        /// while a view is being created; dividing by it would give NaN anchors.
        /// </summary>
        public static bool ToAnchors(Rect safe, float screenWidth, float screenHeight, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            if (screenWidth <= 0f || screenHeight <= 0f)
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return false;
            }

            anchorMin = new Vector2(safe.xMin / screenWidth, safe.yMin / screenHeight);
            anchorMax = new Vector2(safe.xMax / screenWidth, safe.yMax / screenHeight);
            return true;
        }

        void Apply()
        {
            Rect safe = Screen.safeArea;
            if (!ToAnchors(safe, Screen.width, Screen.height, out Vector2 anchorMin, out Vector2 anchorMax))
            {
                return;
            }

            _applied = safe;
            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
