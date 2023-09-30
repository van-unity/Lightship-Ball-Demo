using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Basektball.Scripts {
    public class Cloud : MonoBehaviour {
        public float _oneMeterSpeed = 1;
        public float _maxDelayBeforeNextPosition = .5f;

        public Func<Vector3> PositionGetter;

        private Vector3 _targetPosition;
        private bool _update = true;
        private float _speed;
        private float _delay;

        private void OnEnable() {
            _targetPosition = transform.position;
        }

        private void Start() {
            _delay = Random.Range(.1f, _maxDelayBeforeNextPosition);
        }

        private void Update() {
            if (!_update) {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _speed);
            if (Vector3.Distance(transform.position, _targetPosition) < 0.05f) {
                StartCoroutine(GetNextPos());
            }
        }

        private IEnumerator GetNextPos() {
            _update = false;
            yield return new WaitForSeconds(_delay);
            _delay = Random.Range(.1f, _maxDelayBeforeNextPosition);
            _targetPosition = PositionGetter();
            _speed = _oneMeterSpeed / Vector3.Distance(transform.position, _targetPosition);
            _speed = Mathf.Clamp(_speed, .1f, 5);
            _update = true;
        }
    }
}