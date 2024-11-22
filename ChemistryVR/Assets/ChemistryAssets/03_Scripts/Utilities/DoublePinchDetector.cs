using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// This class is designed to be placed on the index fingertip.
/// It detects double pinching gestures and triggers an event when a double pinch is detected.
/// Additionally, it returns the transform of the detected object.
/// </summary>
/// 

namespace ChemistryVR.Utility
{

    [RequireComponent(typeof(SphereCollider))]
    public class DoublePinchDetector : MonoBehaviour
    {
        public InputActionReference gripInputActionReference;
        public float doublePinchMaxDuration = 0.6f;
        public float sphereRadius = 0.01f;

        [HideInInspector] public UnityEvent<Transform> onDoublePinch;
        private bool pinched = false;
        private float firstPinchTime;
        private SphereCollider sphereCollider;
        private bool isTriggered = false;
        private Transform triggeredTransform;

        private void Start()
        {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = sphereRadius;
        }
        private void OnEnable()
        {
            gripInputActionReference.action.canceled += Pinched;
        }
        private void OnDisable()
        {
            gripInputActionReference.action.canceled -= Pinched;
        }
        private void Pinched(InputAction.CallbackContext context)
        {
            if (pinched)
            {
                var timeDiff = Time.time - firstPinchTime;

                if (timeDiff <= doublePinchMaxDuration)
                {
                    CheckCollision();
                    pinched = false;
                    return;
                }
            }
            pinched = true;
            firstPinchTime = Time.time;
        }
        private void CheckCollision()
        {
            if (isTriggered)
            {
                onDoublePinch?.Invoke(triggeredTransform);
            }
            else
            {
                onDoublePinch?.Invoke(null);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            isTriggered = true;
            triggeredTransform = other.transform;
        }

        private void OnTriggerExit(Collider other)
        {
            isTriggered = false;
        }
    }
}