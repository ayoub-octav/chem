using UnityEngine;

namespace ChemistryVR.Utility
{
    public static class MathUtils
    {
        public static Vector3 CalculateBorderPosition(Vector3 firstPosition, Vector3 secondPosition)
        {
            Vector3 direction = (firstPosition - secondPosition).normalized;

            float radius = Constants.atomRadius - Constants.electronRadius;
            Vector3 newPosition = new Vector3(direction.x * radius, direction.y * radius, direction.z * radius);

            return firstPosition - newPosition;
        }

        public static Vector3 AddOffsetToPosition(Vector3 originalPosition, Vector3 firstPosition, Vector3 secondPosition, int offsetType, float offsetAmount)
        {
            Vector3 newPosition = originalPosition;
            Vector3 direction = (firstPosition - secondPosition).normalized;

            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
            {
                //Objects are more aligned along the Y-axis, apply offset to X-axis
                switch (offsetType)
                {
                    case 1:
                        newPosition.x += offsetAmount;
                        break;
                    case 2:
                        newPosition.x -= offsetAmount;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //Objects are more aligned along the X-axis, apply offset to Y-axis
                switch (offsetType)
                {
                    case 1:
                        newPosition.y += offsetAmount;
                        break;
                    case 2:
                        newPosition.y -= offsetAmount;
                        break;
                    default:
                        break;
                }
            }

            return newPosition;
        }
    }
}