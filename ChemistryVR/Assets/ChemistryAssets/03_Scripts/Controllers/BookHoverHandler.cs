using UnityEngine;

namespace ChemistryVR.Controller
{
    public class BookHoverHandler : MonoBehaviour
    {
        [SerializeField] private BookPageManager bookPageManager;
        [SerializeField] private float maxHoverDistance = 1f;
        [SerializeField] private LayerMask bookLayer;
        private RaycastHit hitPoint;
        private bool detected = false;
        private bool isPlayerPointing;

        private void Update()
        {
            CheckDistanceToBook();
        }
        private void CheckDistanceToBook()
        {

            var forwardDirection = bookPageManager.transform.forward;
            forwardDirection.y = 0f;
            var raycastOriginalPos = transform.position - forwardDirection * maxHoverDistance / 4;
            if (isPlayerPointing && Physics.Raycast(raycastOriginalPos, forwardDirection, out hitPoint, maxHoverDistance, bookLayer))
            {
                float percentage = 1 - (Vector3.Distance(transform.position, hitPoint.point) / maxHoverDistance);

                bookPageManager.SetupHoverMaterial(hitPoint, percentage);
                detected = true;
                return;
            }
            if (detected)
            {
                bookPageManager.DisableHover();
                detected = false;
            }

        }
        public void ChangePointGesture(bool newState)
        {
            isPlayerPointing = newState;

        }
        private void OnDrawGizmos()
        {
            if (bookPageManager != null)
            {
                var forwardDirection = bookPageManager.transform.forward;
                forwardDirection.y = 0f;
                var raycastOriginalPos = transform.position - forwardDirection * maxHoverDistance / 4;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(raycastOriginalPos, raycastOriginalPos + forwardDirection * maxHoverDistance);
            }

        }
    }
}