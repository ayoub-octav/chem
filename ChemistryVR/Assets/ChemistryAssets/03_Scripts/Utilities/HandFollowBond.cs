using UnityEngine;

public class HandFollowBond : MonoBehaviour
{
    private LineRenderer bondLine;
    [SerializeField] private Vector3 offset;

    private void Start()
    {
        bondLine = TutorialManager.Instance.CurrentLinee;
    }

    private void Update()
    {
        if (bondLine != null && bondLine.positionCount >= 2)
        {
            Vector3 startPoint = bondLine.GetPosition(0);
            Vector3 endPoint = bondLine.GetPosition(1);
            Vector3 midpoint = (startPoint + endPoint) / 2;
            transform.position = midpoint + offset;
        }
    }
}
