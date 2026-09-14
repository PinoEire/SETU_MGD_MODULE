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

        void Apply()
        {
            Rect safe = Screen.safeArea;
            float width = Screen.width;
            float height = Screen.height;

            // The editor can report a zero-sized screen for a frame while a view is
            // being created. Dividing by zero would produce NaN anchors, so wait.
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            _applied = safe;

            // Convert the pixel rect to normalised anchors (0..1). Anchors, not
            // offsets, keep the panel correct at any resolution and Canvas Scaler
            // setting because they are relative to the parent, not absolute.
            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= width;
            anchorMin.y /= height;
            anchorMax.x /= width;
            anchorMax.y /= height;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
