using System.Collections;
using UnityEngine;

namespace ChemistryVR.Utility
{
    public class WaitHandler : MonoBehaviour
    {
        public IEnumerator WaitFor(float waitTime)
        {
            yield return WaitCoroutine(waitTime);
        }

        private IEnumerator WaitCoroutine(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
        }
    }
}