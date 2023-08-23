using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallThrower : MonoBehaviour {
    public Camera _camera;
    public GameObject _ballPrefab;
    public float _forceAmount;
    public ForceMode _forceMode;
    public Vector2 _heightRange = new(.1f, .5f);
    public Transform _ballPosition;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            var ball = Instantiate(_ballPrefab);
            var rb = ball.GetComponent<Rigidbody>();
            rb.position = _ballPosition.position;
            rb.rotation = _ballPosition.rotation;
            rb.AddForce(
                (_ballPosition.forward + (Random.Range(_heightRange.x, _heightRange.y) * _ballPosition.up)).normalized *
                _forceAmount,
                _forceMode
            );
        }
    }
}