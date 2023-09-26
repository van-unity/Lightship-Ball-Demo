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
        public GameObject _cloudPrefab;
        
        public Transform _ballPositionTransform;
        public float _zMultiplier = 1;
        public float _yMultiplier = 1;
        public float _forceMultiplier = 3;
        
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
        }

        private Ball GetNextBall() {
            var ball = _balls.First(b => !b.GameObject.activeSelf);
            ball.SetActive(true);
            ball.transform.SetParent(_ballPositionTransform);
            ball.LocalPosition = Vector3.zero;
            ball.transform.localRotation = Quaternion.identity;
            return ball;
        }
        
        private void Update() {
            if (_currentBall == null) {
                return;
            }
            
            // _currentBall.Position = _ballPositionTransform.position;
            
            // Checking mouse button down event
            if (Input.GetMouseButtonDown(0))
            {
                _swipeStarted = true;
                _startTouchPosition = Input.mousePosition;
            }

            // Checking mouse button up event
            if (Input.GetMouseButtonUp(0) && _swipeStarted)
            {
                Vector2 endTouchPosition = Input.mousePosition;
                Vector2 swipeDelta = endTouchPosition - _startTouchPosition;

                // Checking if swipe length is greater than a threshold
                if (swipeDelta.magnitude > 50)  // Threshold can be adjusted
                {
                    Vector2 swipeDirection = new Vector2(swipeDelta.x / Screen.width, swipeDelta.y / (Screen.height * .6f));
                    _currentBall.transform.SetParent(null);
                    ApplyForce(swipeDirection);
                    _currentBall = null;
                    StartCoroutine(WaitAndGetNextBall());
                }

                _swipeStarted = false;
            }
        }
        
        private void ApplyForce(Vector2 direction)
        {
            var force = new Vector3(direction.x, direction.y * _yMultiplier, direction.y * _zMultiplier); // Assuming Y is up. Adjust as needed.
            _currentBall.Rigidbody.useGravity = true;
            _currentBall.Collider.enabled = true;
            _currentBall.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _currentBall.Rigidbody.AddForce(force * _forceMultiplier, ForceMode.VelocityChange); // forceMultiplier is a public float to adjust the magnitude of the force applied.
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
    }
}