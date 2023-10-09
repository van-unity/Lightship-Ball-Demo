using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Basektball.Scripts {
    public class Cloud : MonoBehaviour {
        public float _oneMeterSpeed = 1;
        public float _maxDelayBeforeNextPosition = .5f;
        public float _punchAmount;
        public float _punchDuration;
        public Ease _punchEase;
        public ParticleSystem _particleSystem;
        public float _particlesSpeedRecoverDuration = .5f;
        public Ease _particlesRecoverEase = Ease.InOutQuad;
        public float _particlesHideDuration = .5f;
        public Ease _particlesHideEase = Ease.InOutQuad;
        public float _particlesHideSimulationSpeed = 6;

        public Func<Vector3> PositionGetter;

        private Vector3 _targetPosition;
        private bool _update = true;
        private float _speed;
        private float _delay;

        private void OnEnable() {
            _targetPosition = transform.localPosition;
            _update = true;
        }

        private void Start() {
            _delay = Random.Range(.1f, _maxDelayBeforeNextPosition);
        }

        private void Update() {
            if (!_update) {
                return;
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPosition, Time.deltaTime * _speed);
            if (Vector3.Distance(transform.localPosition, _targetPosition) < 0.05f) {
                StartCoroutine(GetNextPos());
            }
        }

        private IEnumerator GetNextPos() {
            _update = false;
            yield return new WaitForSeconds(_delay);
            _delay = Random.Range(.1f, _maxDelayBeforeNextPosition);
            _targetPosition = PositionGetter();
            _speed = _oneMeterSpeed / Vector3.Distance(transform.localPosition, _targetPosition);
            _speed = Mathf.Clamp(_speed, .1f, 5);
            _update = true;
        }

        public void Bounce() {
            transform.DOPunchPosition(new Vector3(0, _punchAmount, 0), _punchDuration)
                .SetEase(_punchEase).OnComplete(Hide);
        }

        private bool _isVisible;

        public void Hide() {
            if (!_isVisible) {
                return;
            }

            _isVisible = false;
            _particleSystem.Stop(true);
            _particleSystem.DOSimulationSpeed(_particlesHideSimulationSpeed, _particlesHideDuration)
                .SetEase(_particlesHideEase)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void Show() {
            _isVisible = true;
            gameObject.SetActive(true);
            _particleSystem
                .DOSimulationSpeed(1, _particlesSpeedRecoverDuration)
                .SetEase(_particlesRecoverEase);
            _particleSystem.Play();
        }
    }
}