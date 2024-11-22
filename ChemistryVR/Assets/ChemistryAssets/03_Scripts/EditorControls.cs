using ChemistryVR.Controller;
using ChemistryVR.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EditorControls : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float rotationSpeed = 30.0f;
    [SerializeField] private float movementSpeed = 30.0f;
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float scrollFraction = 1f;
    [SerializeField] private InputActionReference mouseXIA;
    [SerializeField] private InputActionReference mouseYIA;
    [SerializeField] private InputActionReference mouseScrollIA;
    [SerializeField] private InputActionReference mouseRightClickIA;
    [SerializeField] private InputActionReference mouseMiddleClickIA;
    [SerializeField] private Transform xROrigin;
    private Vector2 mouseDelta;
    private Camera mainCamera;
    private float scrollValue;
    private bool canMove;
    private bool canRotate;

    [Header("Control Objects")]
    [SerializeField] private InputActionReference mouseLeftClickIA;
    [SerializeField] private InputActionReference mousePositionIA;
    [SerializeField] private float followSpeed = 30.0f;
    [SerializeField] private float objectScrollSpeed = 30.0f;
    [SerializeField] private float minDistanceToScreen = 2f;
    [SerializeField] private LayerMask rayCastObjectsLayer;
    [SerializeField] private LayerMask movableObjectsLayer;
    private Vector2 mousePosition;
    private Transform selectedObject;
    private float selectedObjectDistanceToCamera;

    [Header("Swiping Section")]
    [SerializeField] private InputActionReference swipeLeftIA;
    [SerializeField] private InputActionReference swipeRightIA;
    [SerializeField] private BookPageManager bookPageManager;

    [Header("Atoms Interaction")]
    [SerializeField] private float pinchMaxDuration = 0.2f;
    [SerializeField] private BondCreator bondCreator;
    [SerializeField] private Transform mouseInWorld;
    private float selectedObjectTimeStamp;


    private void OnEnable()
    {
        mouseXIA.action.performed += MouseX;
        mouseYIA.action.performed += MouseY;
        mouseLeftClickIA.action.started += MouseLeftClickStarted;
        mouseLeftClickIA.action.canceled += MouseLeftClickCanceled;
        mouseRightClickIA.action.started += MouseRightClickStarted;
        mouseRightClickIA.action.canceled += MouseRightClickCanceled;
        mouseMiddleClickIA.action.started += MouseMiddleClickStarted;
        mouseMiddleClickIA.action.canceled += MouseMiddleClickCanceled;
        mouseScrollIA.action.performed += MouseScroll;
        mousePositionIA.action.performed += MousePosition;
        swipeLeftIA.action.performed += OnSwipeLeft;
        swipeRightIA.action.performed += OnSwipeRight;
    }
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Vector3 direction = ray.direction;
        Vector3 origin = ray.origin;
        mouseInWorld.position = (origin + direction * selectedObjectDistanceToCamera * (1 + scrollValue * objectScrollSpeed));

        if (canMove)
        {
            //Lerp the camera rig to a new move target position
            xROrigin.position = Vector3.Lerp(xROrigin.position, xROrigin.position - (transform.rotation * (Vector3)mouseDelta).normalized, Time.deltaTime * movementSpeed);
        }

        if (canRotate)
        {
            //Set the target rotation based on the mouse delta position and our rotation speed
            //Pitch
            xROrigin.rotation *= Quaternion.AngleAxis(mouseDelta.y * Time.deltaTime * rotationSpeed, Vector3.left);
            //Yaw
            xROrigin.rotation = Quaternion.Euler(
                xROrigin.eulerAngles.x,
                xROrigin.eulerAngles.y + mouseDelta.x * Time.deltaTime * rotationSpeed,
            xROrigin.eulerAngles.z
            );
        }
        if (selectedObject != null && movableObjectsLayer == (movableObjectsLayer | (1 << selectedObject.gameObject.layer)))
        {

            selectedObject.position = Vector3.MoveTowards(selectedObject.position, mouseInWorld.position, followSpeed * Time.deltaTime);
            selectedObjectDistanceToCamera = Vector3.Distance(selectedObject.position, origin);
            selectedObjectDistanceToCamera = selectedObjectDistanceToCamera < minDistanceToScreen ? minDistanceToScreen : selectedObjectDistanceToCamera;
        }
        else
        {
            //Move the _actualCamera's local position based on the new zoom factor
            xROrigin.transform.localPosition = Vector3.Lerp(xROrigin.transform.localPosition, xROrigin.transform.localPosition + mainCamera.transform.forward * scrollValue, Time.deltaTime * scrollSpeed);
        }


        mouseDelta = Vector2.zero;
        if (scrollValue > 0)
        {
            scrollValue -= scrollFraction;
        }
        else if(scrollValue < 0)
        {
            scrollValue += scrollFraction;
        }
    }

    private void MouseLeftClickStarted(InputAction.CallbackContext obj)
    {
        
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (bondCreator.bondTypeUIPanel.activeSelf)
            {
                bondCreator.ResetAndHideUIPanel();
            }
        }
        OnLeftMouseClick();
    }
    private void MouseLeftClickCanceled(InputAction.CallbackContext obj)
    {
        var timeDiff = Time.time - selectedObjectTimeStamp;
        if (timeDiff <= pinchMaxDuration)
        {
            OnPinchOnObject(selectedObject);
        }
        selectedObject = null;
    }

    private void OnPinchOnObject(Transform selectedObject)
    {
        bondCreator.CreateBond(selectedObject, mouseInWorld);
    }

    private void MouseScroll(InputAction.CallbackContext obj)
    {
        scrollValue += obj.ReadValue<float>();
    }

    private void MousePosition(InputAction.CallbackContext obj)
    {
        mousePosition = obj.ReadValue<Vector2>();
    }

    private void MouseRightClickStarted(InputAction.CallbackContext obj)
    {
        canRotate = true;
    }
    private void MouseRightClickCanceled(InputAction.CallbackContext obj)
    {
        canRotate = false;
    }

    private void MouseMiddleClickStarted(InputAction.CallbackContext obj)
    {
        canMove = true;
    }
    private void MouseMiddleClickCanceled(InputAction.CallbackContext obj)
    {
        canMove = false;
    }

    private void MouseX(InputAction.CallbackContext obj)
    {
        mouseDelta.x = obj.ReadValue<float>();
    }
    private void MouseY(InputAction.CallbackContext obj)
    {
        mouseDelta.y = obj.ReadValue<float>();
    }

    private void OnPinch(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void OnSwipeLeft(InputAction.CallbackContext context)
    {
        bookPageManager.swipeGestureLeft.onSwipeLeft?.Invoke();
    }
    private void OnSwipeRight(InputAction.CallbackContext context)
    {
        bookPageManager.swipeGestureLeft.onSwipeRight?.Invoke();
    }

    private void OnLeftMouseClick()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, rayCastObjectsLayer))
        {
            selectedObject = hit.transform;
            selectedObjectDistanceToCamera = Vector3.Distance(ray.origin, selectedObject.position);
        }
        selectedObjectTimeStamp = Time.time;
    }

    private void OnDisable()
    {
        mouseXIA.action.performed -= MouseX;
        mouseYIA.action.performed -= MouseY;
        mouseLeftClickIA.action.started -= MouseLeftClickStarted;
        mouseLeftClickIA.action.canceled -= MouseLeftClickCanceled;
        mouseRightClickIA.action.started -= MouseRightClickStarted;
        mouseRightClickIA.action.canceled -= MouseRightClickCanceled;
        mouseMiddleClickIA.action.started -= MouseMiddleClickStarted;
        mouseMiddleClickIA.action.canceled -= MouseMiddleClickCanceled;
        mouseScrollIA.action.performed -= MouseScroll;
        mousePositionIA.action.performed -= MousePosition;
        swipeLeftIA.action.performed -= OnSwipeLeft;
        swipeRightIA.action.performed -= OnSwipeRight;
    }
}
