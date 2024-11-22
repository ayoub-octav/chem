using UnityEngine;

namespace ChemistryVR.Utility
{
    public class LookAtPlayer : MonoBehaviour
    {
        private Transform playerTransform;

        private void Start()
        {
            playerTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (playerTransform != null)
            {
                transform.LookAt(playerTransform.position);
            }
        }
    }
}
