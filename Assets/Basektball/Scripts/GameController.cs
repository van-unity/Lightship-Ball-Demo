using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ballerz.Presentation;
using UnityEngine;

namespace Basektball.Scripts {
    public class GameController : MonoBehaviour {
        public HoopManager _hoopManager;
        public GameObject _ballPrefab;
        public Vector3 _ballCameraPositionOffset;

        public float _zMultiplier = 1;
        public float _yMultiplier = 1;
        public float _forceMultiplier = 3;
        public string _cloudTag;

        private List<Ball> _balls;
        private Vector2 _startTouchPosition;
        private bool _swipeStarted;
        private Ball _currentBall;

        private IEnumerator Start() {
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

        private Ball GetNextBall() {
            var ball = _balls.First(b => !b.GameObject.activeSelf);
            ball.SetActive(true);
            ball.transform.SetParent(Camera.main.transform);
            ball.LocalPosition = _ballCameraPositionOffset;
            ball.transform.localRotation = Quaternion.identity;
            return ball;
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
                StartCoroutine(ReleaseBall(_currentBall));
                _currentBall = null;
                StartCoroutine(WaitAndGetNextBall());
                // _currentBall.Rigidbody.useGravity = true;
                // _currentBall.Collider.enabled = true;
                // _currentBall.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                // ThrowBallToPosition(_currentBall.Rigidbody, pos);
                // _hoopManager.AddBallCollider(_currentBall.Collider);
                // StartCoroutine(ReleaseBall(_currentBall));
                // _currentBall = null;
                // StartCoroutine(WaitAndGetNextBall());
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
            yield return new WaitForSeconds(1);

            _currentBall = GetNextBall();
        }

        bool WasMouseClickedOnObjectWithTag(string tagName, out Vector3 hitPos) {
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