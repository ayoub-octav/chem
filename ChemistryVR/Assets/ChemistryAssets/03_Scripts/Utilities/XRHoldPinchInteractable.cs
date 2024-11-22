using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRHoldPinchInteractable : XRGrabInteractable
{
    public float holdDuration = 0.5f;

    [HideInInspector] public UnityEvent<Transform> onHoldPinch;
    [HideInInspector] public UnityEvent onHoldRelease;
    private bool pinched = false;
    private float firstPinchTime;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        PinchStarted();
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        PinchEnded();
    }

    private void PinchStarted()
    {
        pinched = true;
        firstPinchTime = Time.time;
    }
    private void PinchEnded()
    {
        pinched = false;
        onHoldRelease?.Invoke();
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
            onHoldPinch?.Invoke(transform);
            pinched = false;
        }
    }
}
