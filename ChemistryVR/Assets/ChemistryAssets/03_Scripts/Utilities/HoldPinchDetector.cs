using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

namespace ChemistryVR.Utility
{
    public class HoldPinchDetector : MonoBehaviour
    {
        //Right Hand
        public InputActionReference gripInputActionReference;
        public float holdDuration = 0.5f;

        [HideInInspector] public UnityEvent onHoldPinch;
        [HideInInspector] public UnityEvent onHoldRelease;
        private bool pinched = false;
        private float firstPinchTime;

        public TextMeshProUGUI textUI;
        public TextMeshProUGUI text2UI;
        private void OnEnable()
        {
            gripInputActionReference.action.started += PinchStarted;
            gripInputActionReference.action.canceled += PinchEnded;
        }
        private void OnDisable()
        {
            gripInputActionReference.action.started -= PinchStarted;
            gripInputActionReference.action.canceled -= PinchEnded;
        }
        private void PinchStarted(InputAction.CallbackContext context)
        {
            pinched = true;
            firstPinchTime = Time.time;
        }
        private void PinchEnded(InputAction.CallbackContext context)
        {
            pinched = false;
            onHoldRelease?.Invoke();

            text2UI.text = "";
        }

        private void Update()
        {
            if (!pinched)
            {
                return;
            }

            var timeDiff = Time.time - firstPinchTime;

            if (timeDiff > holdDuration)
            {
                onHoldPinch?.Invoke();
                pinched = false;

                Debug.Log("Hold Pinched");
                text2UI.text = "Hold Pinched";
            }
        }
    }
}