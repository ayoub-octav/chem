using ChemistryVR.Enum;
using ChemistryVR.Manager;
using ChemistryVR.Utility;
using UnityEngine;

namespace ChemistryVR.Controller
{
    public class BondController : MonoBehaviour
    {
        private BondManager bondManager;

        [HideInInspector] public Transform firstAtom;
        [HideInInspector] public Transform secondAtom;
        [HideInInspector] public BondType bondType;
        [HideInInspector] public int bondOrderIndex;
        [HideInInspector] public bool nonPossibleBond;

        public int bondID;

        private LineRenderer bond;
        private BoxCollider bondCollider;

        private BondHandler bondHandler;

        public AtomVisualizer firstAtomVisualizer;
        public AtomVisualizer secondAtomVisualizer;

        private void Start()
        {
            bondManager = GetComponentInParent<BondManager>();
            bond = GetComponent<LineRenderer>();
            bondHandler = GetComponent<BondHandler>();
            bondCollider = GetComponentInChildren<BoxCollider>();
            bondCollider.isTrigger = true;
        }
        public void SetBond(Transform firstAtom, Transform secondAtom, BondType bondType)
        {
            this.firstAtom = firstAtom;
            this.secondAtom = secondAtom;
            this.bondType = bondType;

            firstAtomVisualizer = firstAtom.GetComponent<AtomVisualizer>();
            secondAtomVisualizer = secondAtom.GetComponent<AtomVisualizer>();

            SetBondOrder();
        }

        public void RemoveBond()
        {
            AudioManager.Instance.Play("atom_bond_link", firstAtomVisualizer.GetComponent<AudioSource>());
            bondManager.RemoveBond(this);
        }

        public void SetBondOrder()
        {
            bondOrderIndex = bondManager.GetBondCount(this);

            CheckPossibleBond();
        }

        private void CheckPossibleBond()
        {
            nonPossibleBond = !bondHandler.InitializeBond(firstAtom, secondAtom, bondType, bond);

            if (nonPossibleBond)
            {
                SetBondColor(Color.white, Color.white);
            }
            else
            {
                SetBondColor(firstAtomVisualizer.GetAtom().color, secondAtomVisualizer.GetAtom().color);
                StartElectronMovement();
            }
        }

        private void Update()
        {
            if (firstAtom == null || secondAtom == null)
            {
                return;
            }

            Vector3 position = MathUtils.CalculateBorderPosition(firstAtom.position, secondAtom.position);
            position = MathUtils.AddOffsetToPosition(position, firstAtom.position, secondAtom.position, bondOrderIndex, Constants.offsetBondAmount);
            bond.SetPosition(0, position);

            position = MathUtils.CalculateBorderPosition(secondAtom.position, firstAtom.position);
            position = MathUtils.AddOffsetToPosition(position, secondAtom.position, firstAtom.position, bondOrderIndex, Constants.offsetBondAmount);
            bond.SetPosition(1, position);

            SetCollider();
        }
        private void SetBondColor(Color firstColor, Color secondColor)
        {
            Renderer bondRenderer = bond.GetComponent<Renderer>();

            Utils.SetMaterialColor(bondRenderer, Constants.bondColorFirstName, firstColor);

            Utils.SetMaterialColor(bondRenderer, Constants.bondColorSecondName, secondColor);
        }

        private void SetCollider()
        {
            Vector3 pointA = bond.GetPosition(0);
            Vector3 pointB = bond.GetPosition(1);

            Vector3 centerPoint = (pointA + pointB) / 2;

            bondCollider.transform.position = centerPoint;

            Vector3 direction = pointB - pointA;

            if (direction != Vector3.zero)
            {
                bondCollider.transform.rotation = Quaternion.LookRotation(direction);
            }

            float lineLength = direction.magnitude;
            bondCollider.size = new Vector3(0.01f, 0.01f, lineLength);
        }

        private void ResetCollider()
        {
            bondCollider.size = new Vector3(0f, 0f, 0f);
        }

        public void StartElectronMovement()
        {
            bondHandler.ElectronReset();
            bondHandler.StartBondMovement();
        }

        public void StopElectronMovement()
        {
            bondHandler.ElectronReset();
            bondHandler.ElectronInitialState();
            ResetCollider();
        }
    }
}