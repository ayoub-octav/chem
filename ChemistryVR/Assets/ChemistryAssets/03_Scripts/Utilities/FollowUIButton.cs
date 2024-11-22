using UnityEngine;
using UnityEngine.UI;

public class FollowUIButton : MonoBehaviour
{
    public GameObject target3DObject;
    public RectTransform uiButton;
    public Camera xrCamera;
    public Vector3 positionOffset = Vector3.zero;


    void Update()
    {
        Vector3 worldPosition = RectTransformToWorldPosition(uiButton);
        worldPosition += positionOffset;
        target3DObject.transform.position = worldPosition;
        Vector3 directionToUI = (worldPosition - target3DObject.transform.position).normalized;
        target3DObject.transform.rotation = Quaternion.LookRotation(directionToUI);
    }

    Vector3 RectTransformToWorldPosition(RectTransform uiElement)
    {
        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(xrCamera, uiElement.position);
        Vector3 worldPosition;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(uiElement, screenPoint, xrCamera, out worldPosition))
        {
            return worldPosition;
        }
        return uiElement.position;
    }
}
