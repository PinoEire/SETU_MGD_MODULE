using System.Collections.Generic;
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
        string[] _labels;
        int _lastFingers = -1;
        int _lastHeldMask = -1;

        void Awake()
        {
            // Labels are read once; the format loop then touches no component.
            _labels = new string[draggables.Length];
            for (int i = 0; i < draggables.Length; i++)
            {
                _labels[i] = draggables[i].Label;
            }
        }

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
            FormatStatus(_text, fingers, heldMask, _labels);
            status.SetText(_text);
        }

        /// <summary>
        /// "2 fingers, holding Red and Blue". Bit i of <paramref name="heldMask"/>
        /// means <paramref name="labels"/>[i] is held. Pure, so it is tested.
        /// </summary>
        public static void FormatStatus(StringBuilder into, int fingers, int heldMask, IReadOnlyList<string> labels)
        {
            into.Clear();
            into.Append(fingers).Append(fingers == 1 ? " finger" : " fingers");
            if (heldMask == 0)
            {
                into.Append(", nothing held");
                return;
            }

            into.Append(", holding");
            bool first = true;
            for (int i = 0; i < labels.Count; i++)
            {
                if ((heldMask & (1 << i)) == 0)
                {
                    continue;
                }

                into.Append(first ? " " : " and ").Append(labels[i]);
                first = false;
            }
        }
    }
}
