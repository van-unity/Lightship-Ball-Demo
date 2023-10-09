using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ballerz.Presentation;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Basektball.Scripts {
    public class GameController : MonoBehaviour {
        public PointsAnimator _1;
        public PointsAnimator _2;
        public PointsAnimator _3;
        public HoopManager _hoopManager;
        public GameObject _ballPrefab;
        public Vector3 _ballCameraPositionOffset;
        public Vector3 _ballIdleScale;
        public CloudSpawner _cloudSpawner;
        public Vector3 _cloudSpawnerOffset;
        public float _zMultiplier = 1;
        public float _yMultiplier = 1;
        public float _forceMultiplier = 3;
        public string _cloudTag;
        public GameObject _tapToPlace;
        public GameObject _tooClose;

        private List<Ball> _balls;
        private Vector2 _startTouchPosition;
        private bool _swipeStarted;
        private Ball _currentBall;

        private Transform _mainCameraTransform;

        private void Awake() {
            Application.targetFrameRate = 60;
        }

        private IEnumerator Start() {
            _mainCameraTransform = Camera.main.transform;
            _balls = new List<Ball>();
            for (int i = 0; i < 100; i++) {
                var ball = Instantiate(_ballPrefab, transform).GetComponent<Ball>();
                ball.Initialize();
                ball.SetActive(false);
                _balls.Add(ball);
            }

            yield return null;

            _currentBall = GetNextBall();
            Ball.TriggeredWithCloud += BallOnTriggeredWithCloud;
            Ball.Goal += BallOnGoal;
            StartCoroutine(CheckForDistance());
        }

        private void BallOnGoal(Ball obj) {
            var distance = Vector3.Distance(Camera.main.transform.position, _hoopManager.GetHoopPosition());

            if (distance <= 2) {
                _1.Animate(_hoopManager.GetHoopPosition());
            }
            else if (distance <= 3) {
                _2.Animate(_hoopManager.GetHoopPosition());
            }
            else {
                _3.Animate(_hoopManager.GetHoopPosition());
            }
        }

        public float _minDistance = 1;
        public YieldInstruction _distanceCheckInstructon = new WaitForSeconds(1f);

        private IEnumerator CheckForDistance() {
            var camTrans = Camera.main.transform;
            while (true) {
                yield return _distanceCheckInstructon;
                var distance = Vector3.Distance(camTrans.position, _hoopManager.GetIdealShotPosition());
                if (distance < _minDistance) {
                    _tooClose.SetActive(true);
                }
                else {
                    _tooClose.SetActive(false);
                }

                _cloudSpawner.transform.position = _mainCameraTransform.position + _cloudSpawnerOffset;
            }
        }

        private void BallOnTriggeredWithCloud(Ball ball) {
            var hoopPos = _hoopManager.GetIdealShotPosition();
            var force = CalculateForceForDuration(ball.Rigidbody, ball.Position, hoopPos, 1);
            ball.Rigidbody.velocity = Vector3.zero;
            StartCoroutine(WaitOneFramAndCall(() => {
                ball.Rigidbody.AddForce(force, ForceMode.Impulse);
                _hoopManager.AddBallCollider(ball.Collider);
            }));
        }

        private IEnumerator WaitOneFramAndCall(Action callback) {
            yield return null;
            callback?.Invoke();
        }

        public float _nextBallAngle = 15f;
        public float _nextBallDistance = 1;
        public float _nextBallDuration = .5f;
        public Ease _nextBallEase = Ease.OutQuad;

        private Ball GetNextBall() {
            var ball = _balls.First(b => !b.GameObject.activeSelf);
            ball.SetActive(true);
            ball.transform.SetParent(Camera.main.transform);
            ball.LocalPosition = _ballCameraPositionOffset;
            ball.transform.localRotation = Quaternion.identity;
            ball.VisualTransform.localScale = _ballIdleScale;
            return ball;
        }

        public static Vector3 GetRotatedPositionOffset(Transform cameraTransform, float angle, float distance) {
            // Create a rotation quaternion based on the angle and camera's up vector
            Quaternion rotation = Quaternion.AngleAxis(angle, cameraTransform.up);

            // Rotate the camera's forward vector
            Vector3 rotatedForward = rotation * cameraTransform.forward;

            // Normalize the rotated vector, multiply by the distance, and add to the camera's position
            return cameraTransform.position + rotatedForward.normalized * distance;
        }

        private void Update() {
            if (_currentBall == null) {
                return;
            }

            // _currentBall.Position = _ballPositionTransform.position;

            // Checking mouse button down event
            if (Input.GetMouseButtonDown(0)) {
                _swipeStarted = true;
                _startTouchPosition = Input.mousePosition;
            }

            // Checking mouse button up event
            if (Input.GetMouseButtonUp(0) && _swipeStarted) {
                Vector2 endTouchPosition = Input.mousePosition;
                Vector2 swipeDelta = endTouchPosition - _startTouchPosition;

                // Checking if swipe length is greater than a threshold
                if (swipeDelta.magnitude > 50) // Threshold can be adjusted
                {
                    Vector2 swipeDirection =
                        new Vector2(swipeDelta.x / Screen.width, swipeDelta.y / (Screen.height * .6f));
                    _currentBall.transform.SetParent(null);
                    ApplyForce(new Vector3(swipeDirection.x, swipeDirection.y, swipeDirection.y));
                    _currentBall.VisualTransform.DOScale(1, .2f).SetEase(Ease.InOutQuad);
                    _currentBall = null;
                    StartCoroutine(WaitAndGetNextBall());
                }

                _swipeStarted = false;
            }

            if (WasMouseClickedOnObjectWithTag(_cloudTag, out var pos)) {
                var force = CalculateForceForDuration(_currentBall.Rigidbody, _currentBall.Position, pos, .7f);
                _currentBall.SetParent(null);
                _currentBall.Rigidbody.useGravity = true;
                _currentBall.Collider.enabled = true;
                _currentBall.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _currentBall.Rigidbody.AddForce(force, ForceMode.Impulse);
                _currentBall.VisualTransform.DOScale(1, .2f).SetEase(Ease.InOutQuad);
                StartCoroutine(ReleaseBall(_currentBall));
                _currentBall = null;
                StartCoroutine(WaitAndGetNextBall());
            }
        }

        public static Vector3 CalculateForceForDuration(Rigidbody rb, Vector3 fromPos, Vector3 toPos, float duration) {
            // Calculate required velocity to reach the position in given duration
            Vector3 deltaPos = toPos - fromPos;

            float vx = deltaPos.x / duration;
            float vy = (deltaPos.y - 0.5f * Physics.gravity.y * Mathf.Pow(duration, 2)) / duration;
            float vz = deltaPos.z / duration;

            Vector3 desiredVelocity = new Vector3(vx, vy, vz);

            // Calculate the force required to change current velocity to desired velocity
            // Force = mass * deltaVelocity (since ForceMode.VelocityChange is effectively an acceleration mode that ignores mass)
            Vector3 force = desiredVelocity * rb.mass;

            return force;
        }

        private void ApplyForce(Vector3 direction) {
            var force = new Vector3(direction.x, direction.y * _yMultiplier,
                direction.z * _zMultiplier); // Assuming Y is up. Adjust as needed.
            _currentBall.Rigidbody.useGravity = true;
            _currentBall.Collider.enabled = true;
            _currentBall.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _currentBall.Rigidbody.AddRelativeForce(force * _forceMultiplier,
                ForceMode.VelocityChange); // forceMultiplier is a public float to adjust the magnitude of the force applied.
            _hoopManager.AddBallCollider(_currentBall.Collider);
            StartCoroutine(ReleaseBall(_currentBall));
        }

        private IEnumerator ReleaseBall(Ball ball) {
            yield return new WaitForSeconds(10);

            ball.transform.SetParent(transform);
            ball.OnDespawn();
        }

        private IEnumerator WaitAndGetNextBall() {
            yield return new WaitForSeconds(1 - _nextBallDuration);

            var ball = GetNextBall();
            var ballPos = GetRotatedPositionOffset(Camera.main.transform,
                Random.Range(_nextBallAngle * .75f, _nextBallAngle) * (Random.value > .5f ? -1 : 1),
                _nextBallDistance);
            ball.Position = ballPos;

            ball.Transform.DOLocalMove(_ballCameraPositionOffset, _nextBallDuration)
                .SetEase(_nextBallEase);

            yield return new WaitForSeconds(_nextBallDuration + Time.deltaTime);

            _currentBall = ball;
        }

        private static bool WasMouseClickedOnObjectWithTag(string tagName, out Vector3 hitPos) {
            if (Input.GetMouseButtonDown(0)) // 0 for left mouse button
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)) {
                    if (hit.collider.gameObject.CompareTag(tagName)) {
                        hitPos = hit.transform.position;
                        return true;
                    }
                }
            }

            hitPos = Vector3.zero;
            return false;
        }

        private void ThrowBallToPosition(Rigidbody ballRb, Vector3 toPos) {
            Debug.LogError(toPos);
            // Calculate the difference in positions
            Vector3 deltaPos = toPos - ballRb.position;

            // Horizontal distance (distance on the xz-plane)
            float horizontalDistance = new Vector3(deltaPos.x, 0, deltaPos.z).magnitude;

            // Getting the angle in radians
            float launchAngle = 45f * Mathf.Deg2Rad; // 45 degrees provides maximum range in ideal conditions

            // Calculating the initial velocity required to land the ball on target
            float g = Mathf.Abs(Physics.gravity.y);
            float tanAngle = Mathf.Tan(launchAngle);
            float v0_square = (g * horizontalDistance * horizontalDistance) /
                              (2 * (deltaPos.y - horizontalDistance * tanAngle));
            float v0 = Mathf.Sqrt(v0_square);

            // Decompose the velocity into horizontal and vertical components
            float v0x = v0 * Mathf.Cos(launchAngle);
            float v0y = v0 * Mathf.Sin(launchAngle);

            // Calculate the launch velocities in each direction
            Vector3 launchVelocity = new Vector3((deltaPos.x / horizontalDistance) * v0x, v0y,
                (deltaPos.z / horizontalDistance) * v0x);

            // Apply the launch velocity to the ball
            ballRb.velocity = launchVelocity;
        }

        private void OnDestroy() {
            Ball.TriggeredWithCloud -= BallOnTriggeredWithCloud;
        }
    }
}