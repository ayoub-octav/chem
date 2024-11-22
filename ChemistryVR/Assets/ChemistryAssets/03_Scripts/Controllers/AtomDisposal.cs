using ChemistryVR.Manager;
using ChemistryVR.Model;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace ChemistryVR.Controller
{
    public class AtomDisposal : MonoBehaviour
    {
        public Transform disposalArea;
        public float moveSpeed = 0.06f;
        public float scaleSpeed = 3f;

        public AtomCreator atomCreator;

        public BondManager bondManager;


        public AtomGridInitializer atomGridInitializer;

        private AtomVisualizer atomVisualizer;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Atom"))
            {
                XRGrabInteractable grabInteractable = other.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.enabled = false;
                }

                StartCoroutine(DisposeAtom(other.transform));
            }

            if (other.CompareTag("Glassware_Seperate"))
            {
                XRGrabInteractable grabInteractable = other.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.enabled = false;
                }
                StartCoroutine(DisposeGlassware(other.transform));

            }
        }
        private IEnumerator DisposeGlassware(Transform item)
        {
            while (item != null && Vector3.Distance(item.position, disposalArea.position) > 0.05f)
            {
                item.position = Vector3.MoveTowards(item.position, disposalArea.position, moveSpeed * Time.deltaTime);
                item.localScale = Vector3.Lerp(item.localScale, Vector3.zero, scaleSpeed * Time.deltaTime);

                yield return null;
            }

            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        private IEnumerator DisposeAtom(Transform atom)
        {
            bondManager.RemoveAllBondsFromAtom(atom);
            AudioManager.Instance.Play("blackhole",GetComponent<AudioSource>());
            while (atom != null && Vector3.Distance(atom.position, disposalArea.position) > 0.05f)
            {
                atom.position = Vector3.MoveTowards(atom.position, disposalArea.position, moveSpeed * Time.deltaTime);
                atom.localScale = Vector3.Lerp(atom.localScale, Vector3.zero, scaleSpeed * Time.deltaTime);

                yield return null;
            }

            if (atom != null)
            {
                var atomVisualizer = atom.GetComponent<AtomVisualizer>();

                if (atomVisualizer != null && atomCreator.spawnedAtoms.Contains(atomVisualizer.gameObject))
                {
                   
                    ProgressManager.Instance.RemoveUserAtom(atomVisualizer);
               
                    atomCreator.spawnedAtoms.Remove(atomVisualizer.gameObject);
                }

                atomCreator.DestroyAtom(atom.position);
                atomGridInitializer.Total -= 1;
                atomGridInitializer.GridState(true);
                DestroyImmediate(atom.gameObject);
                ProgressManager.Instance.CalculateProgress();

            }
        }
    }
}