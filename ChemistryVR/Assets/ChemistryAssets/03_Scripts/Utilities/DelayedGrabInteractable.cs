using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace ChemistryVR.Utility
{
    public class DelayedGrabInteractable : XRGrabInteractable
    {
        public float pinchHoldTime = .2f;
        private bool canBeGrabbed = false;
        private Coroutine holdCoroutine;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (holdCoroutine == null)
            {
                holdCoroutine = StartCoroutine(EnableGrabAfterDelay());
            }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            if (holdCoroutine != null)
            {
                StopCoroutine(holdCoroutine);
                holdCoroutine = null;
                canBeGrabbed = false;
            }

            base.OnSelectExited(args);
        }

        private IEnumerator EnableGrabAfterDelay()
        {
            yield return new WaitForSeconds(pinchHoldTime);
            canBeGrabbed = true;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (canBeGrabbed)
            {
                base.ProcessInteractable(updatePhase);
            }
        }
    }
}