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

        [HelpBox("Followed player, kept between screen center related bounds", HelpBoxType.Info, order = 1)]
        [SerializeField, ReadOnly] private bool isPlayerAssigned = false;
        [SerializeField, ReadOnly] private Collider2D player = null;
        private float facingSide = 1;

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

        #region Player
        /// <summary>
        /// Update the camera position and make it follow its target.
        /// </summary>
        void ICameraUpdate.Update()
        {
            if (!isPlayerAssigned)
                return;

            // Initialization
            Bounds _bounds = GetPlayerBounds();
            Bounds _target = player.bounds;
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
            _movement *= attributes.Speed;
            transform.position = new Vector3(transform.position.x + _movement.x, transform.position.y + _movement.y, -10);
        }

        private Bounds GetPlayerBounds()
        {
            Bounds _bounds = attributes.TargetBounds;
            _bounds.center = new Vector3(_bounds.center.x * facingSide, _bounds.center.y, _bounds.center.z) + transform.position;
            return _bounds;
        }

        // -----------------------

        /// <summary>
        /// Set player facing side.
        /// </summary>
        public void SetFacingSide(float _facingSide)
        {
            facingSide = _facingSide;
        }

        /// <summary>
        /// Set player collider to follow.
        /// </summary>
        public void SetPlayer(Collider2D _playerCollider)
        {
            isPlayerAssigned = true;
            player = _playerCollider;
        }

        /// <summary>
        /// Remove player to follow.
        /// </summary>
        public void RemovePlayer()
        {
            isPlayerAssigned = false;
            player = null;
        }
        #endregion

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
                float _trauma = Mathf.Pow(shakeTrauma, attributes.ShakeTraumaPower);

                float _angle = attributes.ShakeMaxAngle * _trauma * ((Mathf.PerlinNoise(0, Time.time * attributes.ShakeForce) * 2) - 1);
                float _offsetX = attributes.ShakeMaxOffset.x * _trauma * ((Mathf.PerlinNoise(1, Time.time * attributes.ShakeForce) * 2) - 1);
                float _offsetY = attributes.ShakeMaxOffset.y * _trauma * ((Mathf.PerlinNoise(2, Time.time * attributes.ShakeForce) * 2) - 1);

                camera.transform.localEulerAngles = new Vector3(0, 0, _angle);
                camera.transform.localPosition = new Vector3(_offsetX, _offsetY, 0);

                yield return null;
                shakeTrauma = Mathf.Max(shakeTrauma - (Time.deltaTime * attributes.ShakeSoftening), 0);
            }

            camera.transform.localEulerAngles = Vector3.zero;
            camera.transform.localPosition = Vector3.zero;
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

                Gizmos.DrawWireCube(GetPlayerBounds().center + Vector3.forward, attributes.TargetBounds.size);

                Gizmos.color = _originalColor;
            }
        }
        #endif

        #endregion

        #endregion
    }
}
