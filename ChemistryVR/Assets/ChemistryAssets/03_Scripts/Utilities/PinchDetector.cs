using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// This class is designed to be placed on the index fingertip.
/// It detects pinching gestures and triggers an event when a pinch is detected.
/// Additionally, it returns the transform of the detected object.
/// </summary>
/// 

namespace ChemistryVR.Utility
{

    [RequireComponent(typeof(SphereCollider))]
    public class PinchDetector : MonoBehaviour
    {
        public InputActionReference gripInputActionReference;
        public float pinchMaxDuration = 0.4f;
        public float sphereRadius = 0.01f;

        [HideInInspector] public UnityEvent<Transform> onPinch;
        [HideInInspector] public UnityEvent onPinchInsideAtom;
        private SphereCollider sphereCollider;
        private bool isTriggered = false;
        private Transform triggeredTransform;
        private float firstPinchTime;

        private void Start()
        {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = sphereRadius;
        }
        private void OnEnable()
        {
            gripInputActionReference.action.started += PinchStart;
            gripInputActionReference.action.canceled += PinchEnd;
        }
        private void OnDisable()
        {
            gripInputActionReference.action.started -= PinchStart;
            gripInputActionReference.action.canceled -= PinchEnd;
        }

        private void PinchStart(InputAction.CallbackContext context)
        {
            onPinchInsideAtom?.Invoke();
            firstPinchTime = Time.time;
        }

        private void PinchEnd(InputAction.CallbackContext context)
        {
            var timeDiff = Time.time - firstPinchTime;

            if (timeDiff <= pinchMaxDuration)
            {
                CheckCollision();
            }
        }
        private void CheckCollision()
        {
            if (isTriggered)
            {
                onPinch?.Invoke(triggeredTransform);
            }
            else
            {
                onPinch?.Invoke(null);
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