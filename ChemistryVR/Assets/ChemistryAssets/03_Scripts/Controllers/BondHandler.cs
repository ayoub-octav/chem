using ChemistryVR.Enum;
using ChemistryVR.Manager;
using ChemistryVR.Model;
using UnityEngine;

namespace ChemistryVR.Controller
{
    public class BondHandler : MonoBehaviour
    {
        [HideInInspector] public BondType bondType;
        [HideInInspector] public Transform initialAtomObject;
        [HideInInspector] public Transform targetAtomObject;

        private Atom atom;
        private AtomVisualizer atomVisual;

        private ElectronController electron;
        private ElectronController firstElectronMovement;
        private ElectronController secondElectronMovement;

        private BondManager bondManager;
        private bool secondAtomReleased;

        private Bond bond;

        private void Start()
        {
            bondManager = GetComponentInParent<BondManager>();
        }
        private ElectronController GetElectron(Transform atomObject, int bondOrderIndex)
        {
            atomVisual = atomObject.GetComponent<AtomVisualizer>();
            atom = atomVisual.GetAtom();

            int lastRingIndex = atom.electronConfiguration.Length - 1;
            Transform lastRing = atomVisual.rings[lastRingIndex];

            if (bondOrderIndex > lastRing.childCount - 1)
            {
                return null;
            }
            for (int i = lastRing.childCount - 1; i >= 0; i--)
            {
                ElectronController electronController = lastRing.GetChild(i).GetComponent<ElectronController>();
                if (electronController.IsFreeElectron())
                {
                    return electronController;
                }
            }
            return null;
        }

        public bool InitializeBond(Transform initialAtom, Transform targetAtom, BondType type, LineRenderer bondLineRenderer)
        {
            initialAtomObject = initialAtom;
            targetAtomObject = targetAtom;
            secondAtomReleased = false;
            bondType = type;

            // First electron
            electron = GetElectron(initialAtomObject, bondManager.GetAtomBondCount(initialAtomObject));
            if (electron == null)
            {
                return false;
            }
            firstElectronMovement = electron;
            firstElectronMovement.Initialize(initialAtomObject, targetAtomObject, bondType, bondLineRenderer, this);

            // Second electron if type is covalent
            if (type == BondType.Covalent)
            {
                electron = GetElectron(targetAtomObject, bondManager.GetAtomBondCount(targetAtomObject));
                if (electron == null)
                {
                    return false;
                }
                secondElectronMovement = electron;
                secondElectronMovement.Initialize(targetAtomObject, initialAtomObject, bondType, bondLineRenderer, this, isSecondElectron: true);
            }

            return true;
        }

        public void StartBondMovement()
        {
            if (firstElectronMovement != null)
            {
                firstElectronMovement.StartBondMovement(bondType == BondType.Covalent);
            }
        }
        public void ReleaseSecondAtom()
        {
            if (secondElectronMovement != null && !secondAtomReleased)
            {
                secondElectronMovement.StartBondMovement(duration: .02f);
            }
            secondAtomReleased = true;
        }
        public void ElectronInitialState()
        {
            secondAtomReleased = false;
            if (firstElectronMovement != null)
            {
                firstElectronMovement.ElectronInitialState();
            }

            if (bondType == BondType.Covalent)
            {
                if (secondElectronMovement != null)
                {
                    secondElectronMovement.ElectronInitialState();
                }
            }
        }
        public void ElectronReset()
        {
            secondAtomReleased = false;
            if (firstElectronMovement != null)
            {
                firstElectronMovement.ElectronReset();
            }

            if (bondType == BondType.Covalent)
            {
                if (secondElectronMovement != null)
                {
                    secondElectronMovement.ElectronReset();
                }
            }
        }
        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            BondHandler otherBond= (BondHandler) obj;
            // TODO: write your implementation of Equals() here

            return (bondType == otherBond.bondType) && (bond == otherBond.bond);
        }


        public Bond Bond
        {
            get { return bond; }
            set { bond = value; }
        }
    }
}