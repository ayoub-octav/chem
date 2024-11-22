using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands.Samples.GestureSample;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class HandGestureInputReader : MonoBehaviour, IXRInputButtonReader
{
    [SerializeField] private OctavStaticHandGesture _gestureDetector;

    bool _currentlyPerformed, _newIsPerformed;
    bool _wasDetectedThisFrame, _wasReleasedThisFrame;

    private void OnEnable()
    {
        _gestureDetector.gesturePerformed.AddListener(OnGesturePerformed);
        _gestureDetector.gestureEnded.AddListener(OnGestureEnded);
    }

    private void OnDisable()
    {
        _gestureDetector.gesturePerformed.RemoveListener(OnGesturePerformed);
        _gestureDetector.gestureEnded.RemoveListener(OnGestureEnded);
    }

    private void OnGesturePerformed()
    {
        _newIsPerformed = true;
    }

    private void OnGestureEnded()
    {
        _newIsPerformed = false;
    }

    private void Update()
    {
        _wasDetectedThisFrame = !_currentlyPerformed && _newIsPerformed;
        _wasReleasedThisFrame = _currentlyPerformed && !_newIsPerformed;
        _currentlyPerformed = _newIsPerformed;
    }

    // Interface Methods

    public bool ReadIsPerformed()
    {
        Debug.Log($"Read performed: {_currentlyPerformed}");
        return _currentlyPerformed;
    }

    public float ReadValue()
    {
        return _currentlyPerformed ? 1 : 0;
    }

    public bool ReadWasCompletedThisFrame()
    {
        return _wasReleasedThisFrame;
    }

    public bool ReadWasPerformedThisFrame()
    {
        return _wasDetectedThisFrame;
    }

    public bool TryReadValue(out float value)
    {
        value = _currentlyPerformed ? 1 : 0;
        return _currentlyPerformed;
    }
}