using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Basektball.Scripts {
    public class Cloud : MonoBehaviour {
        public float _maxDistance = 1;
        public float _speed = 5;
        private Vector3 _targetPosition;
        private Vector3 _startPos;
        private bool _update = true;
        private void OnEnable() {
            _targetPosition = transform.position;
            _startPos = transform.position;
        }

        private Vector3 GetTargetPosition() {
            var random = Random.insideUnitCircle;
            random *= _maxDistance;
            var pos = _startPos + transform.right * random.x + transform.forward * (Mathf.Abs(random.y) * Random.Range(-1f, 0));

            return pos;
        }

        private void Update() {
            if (!_update) {
                return;
            }
            
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _speed);
            if (Vector3.Distance(transform.position, _targetPosition) < 0.01f) {
                StartCoroutine(GetNextPos());
            }
        }

        private IEnumerator GetNextPos() {
            _update = false;
            yield return new WaitForSeconds(Random.Range(.1f, 1f));
            _targetPosition = GetTargetPosition();
            _update = true;
        }
    }
}