using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Basektball.Scripts {
    public class CloudSpawner : MonoBehaviour {
        [Header("Spawn Settings")] public GameObject _cloudPrefab;

        public int _cloudCount;
        public float _length = 5; // X-axis
        public float _height = 3; // Y-axis
        public float _width = 5; // Z-axis
        public float _resetTime = 10;

        private List<GameObject> _clouds;
        private float _timer;

        private int _activeCloudsCount = 2;

        private void Start() {
            _clouds = new List<GameObject>(_cloudCount);

            for (int i = 0; i < _cloudCount; i++) {
                var go = SpawnCloud();
                _clouds.Add(go);
                if (i >= _activeCloudsCount) {
                    go.SetActive(false);
                }
            }
        }

        private GameObject SpawnCloud() {
            if (_cloudPrefab == null) {
                Debug.LogError("Cloud prefab is not assigned!");
                return null;
            }


            var cloudGo = Instantiate(_cloudPrefab, GetPositionInsideTheCube(), Quaternion.identity, transform);
            cloudGo.GetComponent<Cloud>().PositionGetter = GetPositionInsideTheCube;

            return cloudGo;
        }

        private Vector3 GetPositionInsideTheCube() {
            var center = transform.position;
            float randomX = center.x + Random.Range(-_length * 0.5f, _length * 0.5f);
            float randomY = center.y + Random.Range(-_height * 0.5f, _height * 0.5f);
            float randomZ = center.z + Random.Range(-_width * 0.5f, _width * 0.5f);
            Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

            return spawnPosition;
        }

        private void Update() {
            _timer += Time.deltaTime;
            if (_timer < _resetTime) {
                return;
            }

            _timer = 0;
            
            _activeCloudsCount = Random.Range(2, _cloudCount + 1);
            for (int i = 0; i < _cloudCount; i++) {
                _clouds[i].SetActive(i < _activeCloudsCount);
            }
        }

        // Draw the rectangle in the Scene view using Gizmos
        private void OnDrawGizmos() {
            var center = transform.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(center, new Vector3(_length, _height, _width));
        }
    }
}