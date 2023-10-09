using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HoopInitialScaleAnimation : MonoBehaviour {
    public float initialScale = .8f;
    public float _duration = .3f;
    public Ease _ease = Ease.OutCubic;

    private void Awake() {
        transform.localScale = Vector3.one * initialScale;
    }

    private void Start() {
        transform.DOScale(1, _duration)
            .SetEase(_ease);
    }
}
