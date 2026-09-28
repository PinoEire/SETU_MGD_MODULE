using System.Text;
using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Demo scaffolding: one line saying how many fingers are down and which
    /// sprites are held. Rebuilt only when either changes, into a reused
    /// StringBuilder, so it allocates nothing while a finger moves.
    /// </summary>
    public sealed class TouchDragHud : MonoBehaviour
    {
        [SerializeField] TouchDragController controller;
        [SerializeField] Draggable[] draggables;
        [SerializeField] TMP_Text status;

        readonly StringBuilder _text = new StringBuilder(64);
        int _lastFingers = -1;
        int _lastHeldMask = -1;

        void Update()
        {
            int fingers = controller.FingerCount;
            int heldMask = 0;
            for (int i = 0; i < draggables.Length; i++)
            {
                if (controller.Tracker.IsHeld(draggables[i].transform))
                {
                    heldMask |= 1 << i;
                }
            }

            if (fingers == _lastFingers && heldMask == _lastHeldMask)
            {
                return;
            }

            _lastFingers = fingers;
            _lastHeldMask = heldMask;

            _text.Clear();
            _text.Append(fingers).Append(fingers == 1 ? " finger" : " fingers");
            if (heldMask == 0)
            {
                _text.Append(", nothing held");
            }
            else
            {
                _text.Append(", holding");
                bool first = true;
                for (int i = 0; i < draggables.Length; i++)
                {
                    if ((heldMask & (1 << i)) == 0)
                    {
                        continue;
                    }

                    _text.Append(first ? " " : " and ").Append(draggables[i].Label);
                    first = false;
                }
            }

            status.SetText(_text);
        }
    }
}
