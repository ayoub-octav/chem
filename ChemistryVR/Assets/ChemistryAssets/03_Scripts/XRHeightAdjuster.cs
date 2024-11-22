using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class XRHeightAdjuster : MonoBehaviour
{
    public Transform xrOriginTransform;

    public float targetHeight = 1.75f;
    public float heightThreshold = 0.1f;

    private void Start()
    {
        AdjustHeight();
    }

    private void Update()
    {
        if (IsXRRunning())
        {
            AdjustHeight();
        }
    }

    private void AdjustHeight()
    {
        if (xrOriginTransform == null) return;

        float currentHeight = xrOriginTransform.position.y;
        if (Mathf.Abs(currentHeight - targetHeight) > heightThreshold)
        {
            Vector3 adjustedPosition = xrOriginTransform.position;
            adjustedPosition.y = targetHeight;
            xrOriginTransform.position = adjustedPosition;
        }
    }

    private bool IsXRRunning()
    {
        var displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displaySubsystems);

        foreach (var displaySubsystem in displaySubsystems)
        {
            if (displaySubsystem.running)
            {
                return true;
            }
        }

        return false;
    }
}