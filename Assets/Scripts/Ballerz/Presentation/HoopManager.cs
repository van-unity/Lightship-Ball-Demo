using System;
using UnityEngine;

namespace Ballerz.Presentation {
    public class HoopManager : MonoBehaviour {
        [SerializeField] private Transform _container;

        [SerializeField] private Transform _hoopCenterTransform;
        [SerializeField] private Cloth _netCloth;

        /// <summary>
        /// used to calculate the force needed to get the ball into the net
        /// </summary>
        [SerializeField] private Transform _idealShotPosition;

        /// <summary>
        /// used to block ball to go through the net 
        /// </summary>
        [SerializeField] private GameObject _netFrontCollider;

        /// <summary>
        /// colliders in this array will interact with the cloth 
        /// </summary>
        private readonly CapsuleCollider[] _clothColliders = { null };

        public Vector3 GetHoopPosition() => _hoopCenterTransform.position;
        public void SetNetFrontColliderEnabled() => _netFrontCollider.SetActive(true);
        public void SetNetFrontColliderDisabled() => _netFrontCollider.SetActive(false);
        public Vector3 GetIdealShotPosition() => _idealShotPosition.position;

        public string Id { get; } = Guid.NewGuid().ToString();

        public void AddBallCollider(CapsuleCollider ballCollider) {
            SetNetFrontColliderDisabled();
            _clothColliders[0] = ballCollider;
            _netCloth.capsuleColliders = _clothColliders;
        }
    }
}