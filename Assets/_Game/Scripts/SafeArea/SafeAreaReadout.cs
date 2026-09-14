using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Prints the current screen size and safe area into a label so the numbers can
    /// be read on the phone and quoted in a journal ("the HUD moves 84 px in from the
    /// top on a Pixel 8"). The string is rebuilt only when a value changes, so there is
    /// no per-frame allocation.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class SafeAreaReadout : MonoBehaviour
    {
        TMP_Text _label;
        Rect _lastSafe;
        int _lastWidth;
        int _lastHeight;

        void Awake()
        {
            _label = GetComponent<TMP_Text>();
        }

        // Start, not Awake: TextMeshPro on the same object may not have finished its
        // own Awake yet, and text assigned before that can miss the first frame.
        void Start()
        {
            Refresh();
        }

        void Update()
        {
            if (Screen.safeArea != _lastSafe || Screen.width != _lastWidth || Screen.height != _lastHeight)
            {
                Refresh();
            }
        }

        void Refresh()
        {
            _lastSafe = Screen.safeArea;
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            // Insets are what students care about: how far the HUD had to move in.
            float left = _lastSafe.xMin;
            float right = _lastWidth - _lastSafe.xMax;
            float bottom = _lastSafe.yMin;
            float top = _lastHeight - _lastSafe.yMax;

            _label.text =
                $"Screen {_lastWidth} x {_lastHeight} px\n" +
                $"Safe area {_lastSafe.width:0} x {_lastSafe.height:0} px\n" +
                $"Insets  L {left:0}  R {right:0}  T {top:0}  B {bottom:0}";
        }
    }
}
