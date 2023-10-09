using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PointsAnimator : MonoBehaviour {
    public float _startScale = .8f;
    public float _y = 1;
    public float _endScale = 1f;
    public Vector3 _rotation = new Vector3(0, 520, 0);
    public float _duration;
    public Ease _ease;

    public void Animate(Vector3 position) {
        gameObject.SetActive(true);
        this.DOKill();
        transform.position = position;
        Vector3 direction = Camera.main.transform.position - transform.position;
        var lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        var eulerAngles = new Vector3(0, lookRotation.eulerAngles.y, 0);
        transform.eulerAngles = eulerAngles;

        position.y += _y;

        var positionTween = transform.DOMove(position, _duration)
            .SetEase(_ease);

        var scaleTween = transform.DOScale(_endScale, _duration)
            .SetEase(_ease)
            .From(_startScale);
        transform.rotation = Quaternion.identity;
        var rotationTween = transform.DORotate(eulerAngles + _rotation, _duration * .5f,
                RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad)
            .SetLoops(1);


        var sequence = DOTween.Sequence();
        sequence.Join(positionTween);
        sequence.Join(scaleTween);
        sequence.Join(rotationTween);

        sequence.AppendInterval(1);
        sequence.AppendCallback(() => { gameObject.SetActive(false); });

        sequence.SetTarget(this);
    }
}