using UnityEngine;

namespace ChemistryVR.Controller
{
    public class RingRotater : MonoBehaviour
    {
        [SerializeField] private Vector3 ringRotationAxis;
        [SerializeField] private float ringSpeed = 1f;
        [SerializeField] private float electronSpeed = 1f;
        [SerializeField] private float pausingDuration = 3.0f;
        private float timeOffset;
        private Transform parent;
        private void Start()
        {
            timeOffset = Time.time;
            parent = transform.parent;
        }

        private void Update()
        {
            Rotate();
        }

        private void Rotate()
        {
            var extraTime = (Time.time - timeOffset) % ((180 / ringSpeed) + pausingDuration);
            if (extraTime < (180 / ringSpeed))
            {
                transform.RotateAround(transform.position, parent.localRotation * ringRotationAxis, ringSpeed * Time.deltaTime);
            }
            transform.Rotate(Vector3.forward, electronSpeed * Time.deltaTime);
        }
    }
}