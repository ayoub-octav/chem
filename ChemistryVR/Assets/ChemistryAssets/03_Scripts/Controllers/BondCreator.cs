using ChemistryVR.Manager;
using ChemistryVR.Enum;
using ChemistryVR.Utility;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using ChemistryVR.Model;

namespace ChemistryVR.Controller
{
    public class BondCreator : MonoBehaviour
    {
        public GameObject bondTypeUIPanel;
        public LineRenderer lineRenderer;

        [SerializeField] private PinchDetector leftPinchInteractable;
        [SerializeField] private PinchDetector rightPinchInteractable;

        private BondManager bondManager;

        private Transform firstAtom;
        private AtomVisualizer firstAtomVisualizer;
        private Transform secondAtom;
        private AtomVisualizer secondAtomVisualizer;
        private LineRenderer bond;
        private BondController bondController;
        private Transform sourceToFollow;

        private bool isBondFrom = true;
        private BondType bondType;
        private string hand;
        private Bond newBond;

        private void Start()
        {
            bondManager = GetComponent<BondManager>();
            GameObject.Find("Book").GetComponent<BookPageManager>().onPageSwitched.AddListener(() => {
                if (bondTypeUIPanel.activeSelf)
                {
                    ResetAndHideUIPanel();
                }
            });
            rightPinchInteractable.onPinchInsideAtom.AddListener(() =>{
                if(isBondFrom)
                {
                    ResetAndHideUIPanel();

                }
            });

            leftPinchInteractable.onPinchInsideAtom.AddListener(() =>{
                if (isBondFrom)
                {
                    ResetAndHideUIPanel();

                }
            });

            leftPinchInteractable.onPinch.AddListener((atom) => CreateBond(atom, leftPinchInteractable.transform));
            rightPinchInteractable.onPinch.AddListener((atom) => CreateBond(atom, rightPinchInteractable.transform));
        }

        private void Update()
        {
            UpdateBondPosition();
        }

        public void CreateBond(Transform obj, Transform objectToFollow)
        {



            if (bondTypeUIPanel.activeSelf)
            {

                if (obj == null)
                {
                    ResetAndHideUIPanel();
                }
                return;
            }
            sourceToFollow = objectToFollow;
            hand = objectToFollow.name;

            if (obj == null)
            {
                Debug.Log("Remove Bond!");
                isBondFrom = true;

                RemoveBond();

                return;
            }

            if (obj.CompareTag("Atom"))
            {


                if (isBondFrom)// && !ProgressManager.Instance.IsAboutToTransfrom)
                {
                    if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.BondCreationComplete)
                    {
                        if (!obj.GetComponent<AtomVisualizer>().GetAtom().symbol.Equals("Na"))
                        {
                            return;
                        }
                    }
                    Debug.Log("Create Bond From: " + obj.name);
                    firstAtom = obj;
                    firstAtomVisualizer = obj.GetComponent<AtomVisualizer>();
                    isBondFrom = false;
                    ShowUIPanel(obj.position);
                    if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.BondCreated)
                    {
                        TutorialManager.Instance.BondCreated = true;
                    }
                }
                else if (firstAtom != obj)
                {
                    Debug.Log("Create Bond To: " + obj.name);
                    Debug.Log("With Bond Type: " + bondType.ToString());

                    secondAtom = obj;
                    secondAtomVisualizer = obj.GetComponent<AtomVisualizer>();
                    isBondFrom = true;

                    bond.name = "Bond from " + firstAtom.name + " To " + secondAtom.name;
                    SetBondColor(firstAtomVisualizer.GetAtom().color, secondAtomVisualizer.GetAtom().color);

                    bondController = bond.GetComponent<BondController>();
                    bondManager.AddBond(bondController);
                    bondController.SetBond(firstAtom, secondAtom, bondType);
                    newBond = SetTheCurrentBond();
                    int index = firstAtomVisualizer.GetID + secondAtomVisualizer.GetID;
                    bondController.bondID = index;
                    bond.GetComponent<BondHandler>().Bond = newBond;
                    BondHandler bondHandler = bond.GetComponent<BondHandler>();
                    ProgressManager.Instance.AddUserBond(bondHandler,index);

                    if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.BondCreationComplete)
                    {
                        TutorialManager.Instance.CurrentLinee = bond;
                        TutorialManager.Instance.BondCreationComplete = true;
                    }
                    //SFX
                    AudioManager.Instance.Play("atom_bond_create", secondAtomVisualizer.GetComponent<AudioSource>());
                    Reset();
                }
            }
            else if (obj.CompareTag("Bond") && isBondFrom)
            {
                BondController currentBond = obj.parent.GetComponent<BondController>();
                currentBond.StopElectronMovement();
                currentBond.RemoveBond();

                //Find closest atom to index tip
                float firstAtomDIstance = Vector3.Distance(FingerTipPosition(), currentBond.firstAtom.position);
                float secondAtomDIstance = Vector3.Distance(FingerTipPosition(), currentBond.secondAtom.position);
                if (firstAtomDIstance <= secondAtomDIstance)
                {
                    secondAtom = currentBond.secondAtom = null;
                    firstAtom = currentBond.firstAtom;
                }
                else
                {
                    secondAtom = currentBond.firstAtom = null;
                    firstAtom = currentBond.secondAtom;
                }

                firstAtomVisualizer = firstAtom.GetComponent<AtomVisualizer>();
                isBondFrom = false;

                bondType = currentBond.bondType;

                bond = currentBond.GetComponent<LineRenderer>();
                UpdateBondInfo();
                if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.BondRemoved)
                {
                    TutorialManager.Instance.HandBookTutorial().SetActive(false);
                }
            }
        }

        private void InstantiateBond()
        {
            bond = Instantiate(lineRenderer, Vector3.zero, Quaternion.identity, transform);

            UpdateBondInfo();
   
        }
        private void UpdateBondInfo()
        {
            bond.name = "Bond from " + firstAtom.name + " To " + hand + " hand finger";
            SetBondColor(firstAtomVisualizer.GetAtom().color, Color.gray);
        
            //Link bond to first atom and finger tip
            UpdateBondPosition();

        }
        private void UpdateBondPosition()
        {
            if (firstAtom != null && secondAtom == null && !bondTypeUIPanel.activeSelf)
            {
                //Calculate position and add offset
                Vector3 position = MathUtils.CalculateBorderPosition(firstAtom.position, FingerTipPosition());
                position = MathUtils.AddOffsetToPosition(position, firstAtom.position, FingerTipPosition(), bondManager.GetBondCountForAtom(firstAtom), Constants.offsetBondAmount);

                //Link bond to first atom and finger tip
                bond.SetPosition(0, position);
                bond.SetPosition(1, FingerTipPosition());
            }
        }
        private void RemoveBond()
        {
            if (bond == null)
            {
                return;
            }
            
            var source = firstAtomVisualizer;
            if (source != null)
            {
                AudioManager.Instance.Play("atom_bond_link", firstAtomVisualizer?.GetComponent<AudioSource>());
            }
            
            RemoveBondFromList();
            Destroy(bond.gameObject);
            Reset();
            if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.BondRemoved && TutorialManager.Instance.BondCreationComplete)
            {
                TutorialManager.Instance.BondRemoved = true;
            }
        }

        private Vector3 FingerTipPosition()
        {
            return sourceToFollow.position;
        }

        private void SetBondColor(Color firstColor, Color secondColor)
        {
            Renderer bondRenderer = bond.GetComponent<Renderer>();

            Utils.SetMaterialColor(bondRenderer, Constants.bondColorFirstName, firstColor);

            Utils.SetMaterialColor(bondRenderer, Constants.bondColorSecondName, secondColor);
        }
        public void ResetAndHideUIPanel()
        {
            HideUIPanel();
            isBondFrom = true;
            Reset();
        }
        private void Reset()
        {
            firstAtom = secondAtom = null;
            bond = null;
        }

        // UI Panel Methods
        private void ShowUIPanel(Vector3 position)
        {
            if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.BondTypeSelect)
            {
                TutorialManager.Instance.HandToSelectBondTypeObject().SetActive(true);
   /*         }
            if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.BondCreated)
            {*/
                bondTypeUIPanel.GetComponent<FollowUIButton>().enabled = true;
            }

            bondTypeUIPanel.transform.position = position - new Vector3(0.18f, 0, 0);
            bondTypeUIPanel.SetActive(true);

            //Desactivate atom grab interactable
            //firstAtom.GetComponent<XRGrabInteractable>().enabled = false;
        }
        private void HideUIPanel()
        {
            if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.BondTypeSelect)
            {
               TutorialManager.Instance.HandToSelectBondTypeObject().SetActive(false);
               bondTypeUIPanel.GetComponent<FollowUIButton>().enabled = false;
            }
            if (firstAtom == null)
            {
                return;
            }

            bondTypeUIPanel.SetActive(false);

            //Activate atom grab interactable
            //firstAtom.GetComponent<XRGrabInteractable>().enabled = true;
        }

        public void IonicButtonClicked()
        {
            if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.BondTypeSelect)
            {
                TutorialManager.Instance.BondTypeSelect = true;
            }
            bondType = BondType.Ionic;
            Debug.Log("Ionic Selected");
            InstantiateBond();
            HideUIPanel();
            
            //SFX
            AudioManager.Instance.Play("atom_bond_create", firstAtomVisualizer.GetComponent<AudioSource>());

        }
        public void CovalentButtonClicked()
        {
            bondType = BondType.Covalent;
            Debug.Log("Covalent Selected");
            InstantiateBond();
            HideUIPanel();
            
            //SFX
            AudioManager.Instance.Play("atom_bond_create", firstAtomVisualizer.GetComponent<AudioSource>());
        }

        private Bond SetTheCurrentBond()
        {
            Bond newBond = new Bond();
            newBond.firstElement = firstAtomVisualizer.GetAtom().symbol + "| ID: " + firstAtomVisualizer.GetID;
            newBond.secondElement = secondAtomVisualizer.GetAtom().symbol + "| ID: " + secondAtomVisualizer.GetID;
            newBond.bondType = bondType;

            return newBond;
        }

        private void RemoveBondFromList()
        {
            BondHandler currentBond = bond.GetComponent<BondHandler>();
            BondController bc = bond.GetComponent<BondController>();
            ProgressManager.Instance.RemoveUserBond(currentBond);
        }

    }
}