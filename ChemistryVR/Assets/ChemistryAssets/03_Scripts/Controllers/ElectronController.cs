using ChemistryVR.Enum;
using ChemistryVR.Model;
using ChemistryVR.Utility;
using System.Collections;
using UnityEngine;

namespace ChemistryVR.Controller
{
    public class ElectronController : MonoBehaviour
    {
        private bool freeElectron = true;
        private Transform atomObject;
        private Transform targetAtomObject;

        private Atom atom;
        private Atom targetAtom;
        private BondType bondType;

        private AtomVisualizer atomVisual;
        private AtomVisualizer targetAtomVisual;

        private WaitHandler waitHandler;
        private Coroutine movementCoroutine;
        private LineRenderer bondLineRenderer;
        private BondHandler bondHandler;
        private bool movingElectron = false;
        private bool isSecondElectron = false;
        private void Start()
        {
            waitHandler = GetComponent<WaitHandler>();
        }
        public void Initialize(Transform atomObject, Transform targetAtomObject, BondType bondType, LineRenderer bondLineRenderer, BondHandler bondHandler, bool isSecondElectron = false)
        {
            this.atomObject = atomObject;
            atomVisual = atomObject.GetComponent<AtomVisualizer>();
            atom = atomVisual.GetAtom();

            this.targetAtomObject = targetAtomObject;
            targetAtomVisual = targetAtomObject.GetComponent<AtomVisualizer>();
            targetAtom = targetAtomVisual.GetAtom();
            this.bondHandler = bondHandler;
            this.bondLineRenderer = bondLineRenderer;
            this.bondType = bondType;

            this.isSecondElectron = isSecondElectron;
            freeElectron = false;
        }
        private IEnumerator MoveToPosition(Transform obj, Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = obj.localPosition;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                obj.position = Vector3.Lerp(obj.parent.TransformPoint(startPosition), obj.parent.TransformPoint(targetPosition), elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            obj.position = obj.parent.TransformPoint(targetPosition);
        }
        private IEnumerator MoveToPosition(Transform obj, LineRenderer lineRenderer, int positionIndex, float duration)
        {
            Vector3 startPosition = obj.localPosition;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                obj.position = Vector3.Lerp(obj.parent.TransformPoint(startPosition), lineRenderer.GetPosition(positionIndex), elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            obj.position = lineRenderer.GetPosition(positionIndex);
        }

        private IEnumerator MoveToPosition(Transform obj, LineRenderer lineRenderer, int startPositionIndex, int targetPositionIndex, float duration)
        {
            float elapsedTime = 0;
            obj.position = lineRenderer.GetPosition(startPositionIndex);
            while (elapsedTime < duration)
            {
                obj.position = Vector3.Lerp(lineRenderer.GetPosition(startPositionIndex), lineRenderer.GetPosition(targetPositionIndex), elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            obj.position = lineRenderer.GetPosition(targetPositionIndex);
        }
        private IEnumerator MoveToPositionInArc(Transform obj, float angle, float duration)
        {
            Vector3 startPosition = obj.localPosition;

            float angleDiff = angle - (Mathf.Atan2(startPosition.y, startPosition.x) * Mathf.Rad2Deg);
            angleDiff %= 360;
            angleDiff = angleDiff > 180 ? angleDiff - 360 : angleDiff;

            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                Quaternion electronAngularRotation = Quaternion.Euler(0, 0, angleDiff * (elapsedTime / duration));
                obj.localPosition = electronAngularRotation * startPosition;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            obj.localPosition = Quaternion.Euler(0, 0, angleDiff) * startPosition;
        }
        public void InsertIntoRing(AtomOrigin atomOrigin)
        {
            int lastRingIndex = (atomOrigin == AtomOrigin.from) ? atom.electronConfiguration.Length - 1 : targetAtom.electronConfiguration.Length - 1;
            Transform lastRing = (atomOrigin == AtomOrigin.from) ? atomVisual.rings[lastRingIndex] : targetAtomVisual.rings[lastRingIndex];
            transform.SetParent(lastRing, true);
            transform.localRotation = Quaternion.identity;
        }
        public IEnumerator MoveBetweenAtoms(AtomOrigin atomOrigin, float duration = 1f)
        {
            if (atomOrigin == AtomOrigin.from)
            {
                transform.SetParent(atomObject, true);
            }
            else
            {
                transform.SetParent(targetAtomObject, true);
            }

            transform.localRotation = Quaternion.identity;

            int endPosition = (int)atomOrigin;
            int startPosition = 1 - endPosition;
            if (isSecondElectron)
            {
                endPosition = startPosition + endPosition;
                startPosition = endPosition - startPosition;
                endPosition = endPosition - startPosition;
            }
            yield return MoveToPosition(transform, bondLineRenderer, startPosition, endPosition, duration);
        }
        public IEnumerator MoveToAtomBorder(AtomOrigin atomOrigin, float duration = 1f)
        {
            if (atomOrigin == AtomOrigin.from)
            {
                transform.SetParent(atomObject, true);
            }
            else
            {
                transform.SetParent(targetAtomObject, true);
            }

            transform.localRotation = Quaternion.identity;

            int positionIndex = (int)atomOrigin;
            if (isSecondElectron)
            {
                positionIndex = 1 - positionIndex;
            }

            yield return MoveToPosition(transform, bondLineRenderer, positionIndex, duration);
        }
        public IEnumerator UpdateElectronsPosition(AtomOrigin atomOrigin, float duration = 0.5f)
        {
            int lastRingIndex = (atomOrigin == AtomOrigin.from) ? atom.electronConfiguration.Length - 1 : targetAtom.electronConfiguration.Length - 1;
            Transform lastRing = (atomOrigin == AtomOrigin.from) ? atomVisual.rings[lastRingIndex] : targetAtomVisual.rings[lastRingIndex];
            float radius = (atomOrigin == AtomOrigin.from) ? atomVisual.radius[lastRingIndex] : targetAtomVisual.radius[lastRingIndex];
            MeshRenderer ringRenderer = lastRing.GetComponent<MeshRenderer>();

            int electronsCount = lastRing.childCount;
            if (electronsCount == 0)
            {
                ringRenderer.enabled = false;
            }
            else
            {
                ringRenderer.enabled = true;
                Vector3 initialPosition = new Vector3(radius, 0, 0);
                for (int i = 0; i < electronsCount; i++)
                {
                    // Ring configuration electrons
                    float angle = i * 360 / electronsCount;
                    Quaternion electronAngularRotation = Quaternion.Euler(0, 0, angle);

                    if (duration == 0)
                    {
                        lastRing.GetChild(i).transform.localPosition = electronAngularRotation * initialPosition;
                    }
                    // set moving electron inside the ring
                    else if (movingElectron)
                    {
                        StartCoroutine(MoveToPosition(lastRing.GetChild(i), electronAngularRotation * initialPosition, duration));
                    }
                    else
                    {
                        StartCoroutine(MoveToPositionInArc(lastRing.GetChild(i), angle, duration));
                    }
                }
            }
            yield return null;
        }

        // Bond
        private IEnumerator ElectronMoveToTarget()
        {
            movingElectron = true;

            yield return MoveToAtomBorder(AtomOrigin.from, .3f);
            yield return UpdateElectronsPosition(AtomOrigin.from, .3f);

            yield return MoveBetweenAtoms(AtomOrigin.to, .3f);
            InsertIntoRing(AtomOrigin.to);
            yield return UpdateElectronsPosition(AtomOrigin.to, .3f);

            movingElectron = false;
        }
        private IEnumerator ElectronReturnFromTarget()
        {
            movingElectron = true;

            yield return MoveToAtomBorder(AtomOrigin.to, .3f);
            yield return UpdateElectronsPosition(AtomOrigin.to, .3f);

            yield return MoveBetweenAtoms(AtomOrigin.from, .3f);
            InsertIntoRing(AtomOrigin.from);
            yield return UpdateElectronsPosition(AtomOrigin.from, .3f);

            movingElectron = false;
        }
        public IEnumerator ElectronReturnToInitialPosition()
        {
            InsertIntoRing(AtomOrigin.from);
            yield return UpdateElectronsPosition(AtomOrigin.from, 0f);
            movingElectron = false;
        }

        private IEnumerator IonicBond()
        {
            yield return ElectronMoveToTarget();
        }

        private IEnumerator CovalentBond(bool invoke)
        {
            while (true)
            {
                yield return ElectronMoveToTarget();

                yield return waitHandler.WaitFor(4f);

                // In other atom
                if (invoke)
                {
                    bondHandler.ReleaseSecondAtom();
                }

                yield return ElectronReturnFromTarget();

                yield return waitHandler.WaitFor(4f);
            }
        }
        public IEnumerator StartMovement(bool invoke, float duration)
        {
            yield return new WaitForSeconds(duration);
            yield return ElectronReturnToInitialPosition();

            if (bondType == BondType.Ionic)
            {
                yield return IonicBond();
            }
            else if (bondType == BondType.Covalent)
            {
                yield return CovalentBond(invoke);
            }
        }

        public void StartBondMovement(bool invoke = false, float duration = 0)
        {
            movementCoroutine = StartCoroutine(StartMovement(invoke, duration));
        }
        public void ElectronInitialState()
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(ElectronReturnToInitialPosition());
                freeElectron = true;
            }

        }
        public void ElectronReset()
        {
            if (movementCoroutine == null)
            {
                return;
            }

            StopCoroutine(movementCoroutine);
            freeElectron = true;
        }

        public bool IsFreeElectron()
        {
            return freeElectron;
        }
    }
}