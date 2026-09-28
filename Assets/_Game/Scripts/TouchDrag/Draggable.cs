using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Marks a sprite the player can drag. Needs a <c>Collider2D</c>, because the
    /// controller finds what is under a finger with <c>Physics2D.OverlapPoint</c>;
    /// make the collider at least 48 dp across, including any padding, or the
    /// sprite is hard to pick up. While held the sprite grows a little and
    /// brightens, so the pick-up is visible in monochrome and without sound.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class Draggable : MonoBehaviour
    {
        [SerializeField] string label = "Sprite";
        [SerializeField, Range(1f, 1.5f)] float heldScale = 1.15f;

        SpriteRenderer _renderer;
        Color _restColour = Color.white;
        Vector3 _restScale = Vector3.one;

        /// <summary>Name shown by the HUD while this sprite is held.</summary>
        public string Label => label;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _restScale = transform.localScale;
            if (_renderer != null)
            {
                _restColour = _renderer.color;
            }
        }

        public void SetHeld(bool held)
        {
            transform.localScale = held ? _restScale * heldScale : _restScale;
            if (_renderer != null)
            {
                _renderer.color = held ? Color.Lerp(_restColour, Color.white, 0.35f) : _restColour;
            }
        }
    }
}
