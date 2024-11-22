using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRDoublePinchInteractable : XRGrabInteractable
{
    public float doublePinchMaxDuration = 1f;

    [HideInInspector] public UnityEvent<Transform> onDoublePinch;
    private bool pinched = false;
    private float firstPinchTime;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        Pinched();
    }

    private void Pinched()
    {
        if (pinched)
        {
            var timeDiff = Time.time - firstPinchTime;

            if (timeDiff <= doublePinchMaxDuration)
            {
                onDoublePinch?.Invoke(transform);

                pinched = false;
                return;
            }
        }
        pinched = true;
        firstPinchTime = Time.time;
    }
}
