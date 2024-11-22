using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshRateHandler : MonoBehaviour
{
    [SerializeField, Range(72, 120)] private int targetRefreshRate = 90;
    // Start is called before the first frame update
    private void Awake()
    {
        OVRManager.DisplayRefreshRateChanged += DisplayRefreshRateChanged;
        OVRPlugin.systemDisplayFrequency = targetRefreshRate;
    }
    private void DisplayRefreshRateChanged(float fromRefreshRate, float toRefreshRate)
    {
        // Handle display refresh rate changes
        Debug.Log(string.Format("Refresh rate changed from {0} to {1}", fromRefreshRate, toRefreshRate));
    }

}
