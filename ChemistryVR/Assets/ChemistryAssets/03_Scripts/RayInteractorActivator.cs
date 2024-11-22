using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RayInteractorActivator : MonoBehaviour
{
    public LayerMask gridLayerMask;
    public Transform rayOrigin;
    public float maxDistance = 3f;

    private NearFarInteractor nearFarInteractor;

    private void Start()
    {
        nearFarInteractor = GetComponent<NearFarInteractor>();
        if (nearFarInteractor == null)
        {
            Debug.LogError("NearFarInteractor component not found.");
        }
    }

    private void Update()
    {
        if (nearFarInteractor == null)
        {
            Debug.LogWarning("NearFarInteractor is not assigned.");
            return;
        }

        RaycastHit hit;
        Debug.Log($"Ray Origin: {rayOrigin.position}, Direction: {rayOrigin.forward}, Distance: {maxDistance}");

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, maxDistance, gridLayerMask))
        {
            Debug.Log("Ray hit: " + hit.collider.gameObject.name);
            Debug.Log($"Hit Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            Debug.Log("Hit object is on the GridLayer.");
            nearFarInteractor.enableFarCasting = true;
        }
        else
        {
            Debug.Log("Ray did not hit any object2.");
            nearFarInteractor.enableFarCasting = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (rayOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + rayOrigin.forward * maxDistance);
        }
    }
}
