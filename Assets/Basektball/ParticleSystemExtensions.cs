using DG.Tweening;
using UnityEngine;

namespace Basektball {
    public static class ParticleSystemExtensions {
        public static Tween DOSimulationSpeed(this ParticleSystem particleSystem, float endValue, float duration) {
            var main = particleSystem.main;

            return DOTween.To(() => main.simulationSpeed, speed => main.simulationSpeed = speed, endValue,
                duration);
        }
    }
}