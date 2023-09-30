using UnityEngine;

namespace Basektball.Scripts {
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour {
        [Header("Movement Settings")] public float moveSpeed = 5.0f;

        [Header("Rotation Settings")] public float mouseSensitivity = 2.0f;
        private float rotationX = 0.0f;
        private float rotationY = 0.0f;

        void Update() {
            HandleMovement();
            HandleRotation();
        }

        void HandleMovement() {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        }

        void HandleRotation() {
            // Only rotate when the mouse button is pressed
            if (Input.GetMouseButton(1)) // Right mouse button by default, use 0 for left mouse button if preferred
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                rotationX -= mouseY * mouseSensitivity;
                rotationY += mouseX * mouseSensitivity;

                // Clamp rotation to prevent flipping
                rotationX = Mathf.Clamp(rotationX, -90, 90);

                transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
            }
        }
    }
}