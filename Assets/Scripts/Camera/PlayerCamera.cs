// ======= Created by Lucas Guibert - https://github.com/LucasJoestar ======= //
//
// Notes :
//
// ========================================================================== //

using EnhancedEditor;
using System.Collections;
using UnityEngine;

namespace Nowhere
{
    public class PlayerCamera : UpdatedBehaviour, ICameraUpdate
    {
        #region Fields
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static PlayerCamera Instance { get; private set; }

        // -----------------------

        [Section("CAMERA", 50, 0, order = 0), HorizontalLine(2, SuperColor.Chocolate, order = 1)]

        [SerializeField, Required] private PlayerCameraAttributes attributes = null;
        [SerializeField, Required] private new Camera camera = null;

        [HorizontalLine(1, order = 0)]

        [HelpBox("Followed target, kept between screen center related bounds", HelpBoxType.Info, order = 1)]
        [SerializeField] private Collider2D target = null;

        [HorizontalLine(2, SuperColor.Crimson)]

        [SerializeField, Min(0)] private float shakeForce =     1;
        [SerializeField, Min(0)] private float shakeSoftening = .5f;
        [SerializeField, Min(0)] private int shakeTraumaPower = 2;

        [SerializeField] private float shakeMaxAngle = 10;
        [SerializeField] private Vector2 shakeMaxOffset = Vector2.one;

        [HorizontalLine(2, SuperColor.Sapphire)]

        [SerializeField, ReadOnly] private bool isMoving = false;
        [SerializeField, ReadOnly] private float shakeTrauma = 0;

        // -----------------------

        #if UNITY_EDITOR
        [Section("EDITOR", 25, 0, order = 0), HorizontalLine(2, SuperColor.Grey, order = 1)]

        [SerializeField] private bool doDrawBounds = false;
        [SerializeField] private SuperColor boundsColor = SuperColor.Silver;
        #endif

        #endregion

        #region Methods
        /// <summary>
        /// Update the camera position and make it follow its target.
        /// </summary>
        void ICameraUpdate.Update()
        {
            if (!target)
                return;

            // Initialization
            Bounds _bounds = GetTargetBounds();
            Bounds _target = target.bounds;
            Vector2 _movement = Vector2.zero;

            // Horizontal movement.
            float _boundsCalcul = (_target.center.x + _target.extents.x) - (_bounds.center.x + _bounds.extents.x);
            if (_boundsCalcul > 0)
                _movement.x = _boundsCalcul;

            else
            {
                _boundsCalcul = (_target.center.x - _target.extents.x) - (_bounds.center.x - _bounds.extents.x);
                if (_boundsCalcul < 0)
                    _movement.x = _boundsCalcul;
            }

            // Vertical movement.
            _boundsCalcul = (_target.center.y + _target.extents.y) - (_bounds.center.y + _bounds.extents.y);
            if (_boundsCalcul > 0)
                _movement.y = _boundsCalcul;

            else
            {
                _boundsCalcul = (_target.center.y - _target.extents.y) - (_bounds.center.y - _bounds.extents.y);
                if (_boundsCalcul < 0)
                    _movement.y = _boundsCalcul;
            }

            // Check if target remains in bounds.
            if (_movement.IsNull())
            {
                if (isMoving)
                    isMoving = false;

                return;
            }

            if (!isMoving)
                isMoving = true;

            // Move 10% closer to the target each frame.
            _movement *= .1f;
            transform.position = new Vector3(transform.position.x + _movement.x, transform.position.y + _movement.y, -10);
        }

        private Bounds GetTargetBounds()
        {
            Bounds _bounds = attributes.TargetBounds;
            _bounds.center += transform.position;
            return _bounds;
        }

        #region Screenshake
        private Coroutine screenshakeCoroutine = null;

        // -----------------------

        public void Shake(float _trauma)
        {
            shakeTrauma = Mathf.Clamp01(shakeTrauma + _trauma);

            if (screenshakeCoroutine == null)
                screenshakeCoroutine = StartCoroutine(DoShake());
        }

        private IEnumerator DoShake()
        {
            while (shakeTrauma > 0)
            {
                yield return null;

                shakeTrauma = Mathf.Max(shakeTrauma - (Time.deltaTime * shakeSoftening), 0);
                float _trauma = Mathf.Pow(shakeTrauma, shakeTraumaPower);

                float _angle = shakeMaxAngle * _trauma * ((Mathf.PerlinNoise(0, Time.time * shakeForce) * 2) - 1);
                float _offsetX = shakeMaxOffset.x * _trauma * ((Mathf.PerlinNoise(1, Time.time * shakeForce) * 2) - 1);
                float _offsetY = shakeMaxOffset.y * _trauma * ((Mathf.PerlinNoise(2, Time.time * shakeForce) * 2) - 1);

                camera.transform.localEulerAngles = new Vector3(0, 0, _angle);
                camera.transform.localPosition = new Vector3(_offsetX, _offsetY, 0);
            }

            screenshakeCoroutine = null;
        }
        #endregion

        #region Monobehaviour
        private void Awake()
        {
            // Singleton set.
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            UpdateManager.Instance.Register(this);
        }

        protected override void OnDisableCallback()
        {
            UpdateManager.Instance.Unregister(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw target bounds on screen.
            if (doDrawBounds && attributes)
            {
                Color _originalColor = Gizmos.color;
                Gizmos.color = boundsColor.GetColor();

                Gizmos.DrawWireCube(GetTargetBounds().center + Vector3.forward, attributes.TargetBounds.size);

                Gizmos.color = _originalColor;
            }
        }
        #endif

        #endregion

        #endregion
    }
}
