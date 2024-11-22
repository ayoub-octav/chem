using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRInteractorInputOverride : MonoBehaviour
{
    enum XRInteractorAction
    {
        Select, Activate
    }

    [SerializeField] private XRInputButtonReader leftHandInputReader;
    [SerializeField] private XRInputButtonReader rightHandInputReader;
    [SerializeField] private XRBaseInputInteractor leftHandInteractor;
    [SerializeField] private XRBaseInputInteractor rightHandInteractor;
    [SerializeField] private XRInteractorAction actionToOverride;

    XRInputButtonReader backupLeftHandInputReader;
    XRInputButtonReader backupRightHandInputReader;

    private void OnEnable()
    {
        switch (actionToOverride)
        {
            case XRInteractorAction.Activate:
                backupLeftHandInputReader = leftHandInteractor.activateInput;
                leftHandInteractor.activateInput = leftHandInputReader;
                backupRightHandInputReader = rightHandInteractor.activateInput;
                rightHandInteractor.activateInput = rightHandInputReader;
                break;

            case XRInteractorAction.Select:
                backupLeftHandInputReader = leftHandInteractor.selectInput;
                leftHandInteractor.selectInput = leftHandInputReader;
                backupRightHandInputReader = rightHandInteractor.selectInput;
                rightHandInteractor.selectInput = rightHandInputReader;
                break;
        }
    }

    private void OnDisable()
    {
        if (backupLeftHandInputReader == null || leftHandInteractor == null ||
            backupRightHandInputReader == null || rightHandInteractor == null)
            return;

        switch (actionToOverride)
        {
            case XRInteractorAction.Activate:
                leftHandInteractor.activateInput = backupLeftHandInputReader;
                rightHandInteractor.activateInput = backupRightHandInputReader;
                break;

            case XRInteractorAction.Select:
                leftHandInteractor.selectInput = backupLeftHandInputReader;
                rightHandInteractor.selectInput = backupRightHandInputReader;
                break;
        }
    }
}
