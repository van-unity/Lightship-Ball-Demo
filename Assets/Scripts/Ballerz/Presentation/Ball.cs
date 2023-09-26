using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Ballerz.Presentation {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class Ball : MonoBehaviour,  IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler {
        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private Transform _transform;
        private GameObject _gameObject;
        private TrailRenderer _trail;
        private ParticleSystem _particleSystem;

        [SerializeField] private Transform _visualTransform;
        [SerializeField] private Vector3 _visualTransformInitialScale = new(0.5f, 0.5f, 0.46f);
        [SerializeField] private Vector3 _visualTransformInitialRotation = new(-60, 0, 0);
        [SerializeField] private float _maxForceMagnitudeOnGroundCollision = .1f;
        [SerializeField] private ForceMode _groundCollisionForceMode = ForceMode.VelocityChange;

        private int _groundCollisionCount;
        private int _rimCollisionCount;
        private int _backBoardCollisionCount;
        private HoopManager _hoopManager;
        private Vector3 _screenPosition;

        public Vector3 Position {
            get => _transform.position;
            set {
                _transform.position = value;
                _screenPosition = UnityEngine.Camera.main.WorldToScreenPoint(_transform.position);
            }
        }

        public Vector3 LocalPosition {
            get => _transform.localPosition;
            set {
                _transform.localPosition = value;
                _screenPosition = UnityEngine.Camera.main.WorldToScreenPoint(_transform.position);
            }
        }

        public Quaternion Rotation {
            get => _transform.rotation;
            set { _transform.rotation = value; }
        }

        public Vector3 Scale {
            get => _transform.localScale;
            set => _transform.localScale = value;
        }

        public Vector3 Right => _transform.right;

        public Rigidbody Rigidbody => _rigidbody;
        public CapsuleCollider Collider => _collider;

        public GameObject GameObject => _gameObject;
        public Transform Transform => _transform;

        public Transform VisualTransform => _visualTransform;

        public string Id { get; } = Guid.NewGuid().ToString();

        public bool IsActive => _gameObject.activeSelf;

        private void Awake() {
        }

        public void Initialize() {
            _gameObject = gameObject;
            _transform = _gameObject.transform;
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            _trail = GetComponentInChildren<TrailRenderer>();
            _trail.gameObject.SetActive(false);
            _particleSystem = GetComponentInChildren<ParticleSystem>();
            _particleSystem.Stop();
            _particleSystem.gameObject.SetActive(false);
        }

        private void Start() {
            DisableTrail();
            _hoopManager = FindObjectOfType<HoopManager>();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("BackboardSpot")) {
                var velocity = _hoopManager.GetIdealShotPosition() - Position;
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.AddForce(velocity, ForceMode.Impulse);
            }
        }

        private void OnCollisionEnter(Collision collision) {
            _rigidbody.drag = .4f;
            var collisionMagnitude = collision.impulse.magnitude;
            if (collisionMagnitude > 2) {
                if (collision.gameObject.CompareTag("Rim") && ++_rimCollisionCount == 1) {
                    WaitOneFrameAndMultiplyVelocity(.5f);
                }
                else if (collision.gameObject.CompareTag("Board") && ++_backBoardCollisionCount == 1) {
                    WaitOneFrameAndMultiplyVelocity(.75f);
                }
            }

            //if colliding with ground apply some random force 
            //otherwise all balls behave same way which looks bad
            if (collision.gameObject.CompareTag("Ground") && ++_groundCollisionCount == 1) {
                Rigidbody.AddForce(
                    new Vector3(Random.Range(-1f, 1f), 0, 0) *
                    _maxForceMagnitudeOnGroundCollision,
                    _groundCollisionForceMode);
            }

        }

        private async void WaitOneFrameAndMultiplyVelocity(float multiplier) {
            await Task.Yield();
            Rigidbody.velocity *= multiplier;
        }

        public void SetActive(bool isActive) {
            _gameObject.SetActive(isActive);
        }

        public void EnableTrail() {
            // _particleSystem.Play();
        }

        public void DisableTrail() {
            // _particleSystem.Stop();
        }

        public void OnSpawn() {
            // SetActive(true);
            // _collider.enabled = true;
            VisualTransform.localScale = _visualTransformInitialScale;
            VisualTransform.localRotation = Quaternion.Euler(_visualTransformInitialRotation);
        }

        public void OnDespawn() {
            _rigidbody.drag = 0;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _transform.rotation = Quaternion.identity;
            _rigidbody.useGravity = false;
            _rigidbody.interpolation = RigidbodyInterpolation.None;
            _collider.enabled = false;
            _groundCollisionCount = 0;
            _rimCollisionCount = 0;
            _backBoardCollisionCount = 0;
            SetActive(false);
            DisableTrail();
        }

        private float _dragStartTime;

        public void OnDrag(PointerEventData eventData) {
            var mousePosition = eventData.position;
            if (mousePosition.y > _screenPosition.y * 2) {
                return;
            }
            var position = UnityEngine.Camera.main
                .ScreenToWorldPoint(new Vector3(_screenPosition.x, mousePosition.y,
                    _screenPosition.z));
            if (mousePosition.y <= _screenPosition.y) {
                _transform.position = position;
            }

            var horizontalDrag = .5f - UnityEngine.Camera.main.ScreenToViewportPoint(eventData.position).x;
            // UnityEngine.Camera.main.transform.rotation = Quaternion.Euler(0, horizontalDrag * 30f, 0);

            var y = (_screenPosition.y - mousePosition.y) / _screenPosition.y;
            var z = y;
            var x = horizontalDrag / .5f;
            var throwDirection = new Vector3(x, y, z);
        }

        public void OnBeginDrag(PointerEventData eventData) {
            _dragStartTime = Time.time;
        }

        public void OnEndDrag(PointerEventData eventData) {
            var mousePosition = eventData.position;
            if (mousePosition.y > _screenPosition.y * 2) {
                mousePosition.y = _screenPosition.y * 2;
            }

            var r = .5f - UnityEngine.Camera.main.ScreenToViewportPoint(mousePosition).x;
            var y = (_screenPosition.y - mousePosition.y) / _screenPosition.y;
            var z = y;
            var x = r / .5f;
            var throwDirection = new Vector3(x, y, z);
        }

        public void SetParent(Transform parent) {
            _transform.SetParent(parent);
        }

        public void OnPointerClick(PointerEventData eventData) {
            Debug.LogError("click");
        }
    }
}