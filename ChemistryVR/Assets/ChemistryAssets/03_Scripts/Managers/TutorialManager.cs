using ChemistryVR.Controller;
using ChemistryVR.Model;
using ChemistryVR.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public AtomGridInitializer atomGridInitializer;
    public BookPageManager bookManager;
    public Transform bondAtomPostion;

    public Button ionicButton;
    public Button covalentButton;

    public Color bondButtonTutorialColor;
    public Color normalColor;

    [SerializeField] private Animator swipeTutorial;
    [SerializeField] private Animator pokeGridTutorial;
    [SerializeField] private Animator pinchMoveAtomTutorial;
    [SerializeField] private Animator pokeToSelectBondType;
    [SerializeField] private Animator pinchReleaseCreateBond;
    [SerializeField] private Animator pinchToRemoveBond;

    [SerializeField] private Animator pokeToSwipeRing;

    private bool hasSwipedLeft = false;
    private bool hasSwipedRight = false;
    private bool hasSpawnedAtom = false;
    private bool hasMovedAtom = false;
    private bool hasCreatedBond = false;
    private bool hasSwipedRing = false;
    private bool hasTransformed = false;
    private bool hasSelectedBondType = false;
    private bool isBondComplete = false;
    private bool hasRemovedBond = false;

    private bool isSkippable = false;
    private string tutorialKey = "TutorialSeen";
    private string firstTargetProgress = "FirstComplete";
    private bool isTutorial = false;

    private bool hasBookTutorialStarted = false;

    private LineRenderer lineRenderer;
    private void Awake()
    {
      
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (PlayerPrefs.HasKey(tutorialKey))
        {
            isTutorial = false;

            DisableAllChildren(); 
            //gameObject.SetActive(false);
        }
        else
        {
            Tutorial();
        }
        BondButtonHandler(isTutorial);
    }

    // Start is called before the first frame update
    void Start()
    {

        ProgressManager.Instance.onProgressComplete.AddListener(OnProgressComplete);
        ProgressManager.Instance.onProgressChange.AddListener(OnProgressNotComplete);


        StartTutorial();

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnEnable()
    {

        bookManager.onPageSwitched.AddListener(OnPageSwipedWhileInTutorial);

    }

    private void OnPageSwipedWhileInTutorial()
    {
        if (pokeToSwipeRing.gameObject.activeSelf && !bookManager.GetCurrentCompound().atomList.GetElements()[0].element.symbol.Equals("Na"))
        {
            SwipedRing = true;
        }
    }

    private void OnProgressComplete()
    {
        if (!PlayerPrefs.HasKey(firstTargetProgress))
        {
            if (hasBookTutorialStarted)
            {
                pokeToSwipeRing.gameObject.SetActive(true);
            }
            else
            {
                hasBookTutorialStarted = true;
            } 
        }
    }

    private void OnProgressNotComplete()
    {
        if (pokeToSwipeRing.gameObject.activeSelf)
        {
           pokeToSwipeRing.gameObject.SetActive(false);
        }
    }

    public void DisableAllChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
    private void BondButtonHandler(bool state)
    {
        ColorBlock colorBlock = ionicButton.colors;
        colorBlock.normalColor = state ? bondButtonTutorialColor : normalColor;
        ionicButton.colors = colorBlock;
        covalentButton.interactable = !state;
    }
    public void StartTutorial()
    {
        StartCoroutine(TutorialSequence());
    }
  

    private IEnumerator TutorialSequence()
    {

        if (isTutorial)
        {
            yield return new WaitUntil(SwipedLeftCondition);
            yield return new WaitForSeconds(0.2f);
            swipeTutorial.SetTrigger("swipe");
            yield return new WaitUntil(SwipedRightCondition);
            yield return new WaitForSeconds(1f);
            isSkippable = true;
            //bookManager.bookText.gameObject.SetActive(true);
            swipeTutorial.gameObject.SetActive(false);
            pokeGridTutorial.gameObject.SetActive(true);
            atomGridInitializer.GridState(true);
            yield return new WaitUntil(SpanwedAtomCondition);
            yield return new WaitForSeconds(0.5f);
            atomGridInitializer.GridState(false);
            pokeGridTutorial.gameObject.SetActive(false);
            pinchMoveAtomTutorial.gameObject.SetActive(true);
            yield return new WaitUntil(PinchMovedAtom);
            yield return new WaitForSeconds(1f);
            atomGridInitializer.SpawnNewAtomForTutorial();
            pinchMoveAtomTutorial.gameObject.SetActive(false);
            pinchReleaseCreateBond.gameObject.SetActive(true);
            yield return new WaitUntil(PinchBondCreation);
            yield return new WaitForSeconds(1f);
            pinchReleaseCreateBond.gameObject.SetActive(false);
            pokeToSelectBondType.gameObject.SetActive(true);
            yield return new WaitUntil(BondTypeSelection);
            yield return new WaitForSeconds(0.2f);
            pokeToSelectBondType.gameObject.SetActive(false);
            yield return new WaitUntil(BondComplete);
            pinchToRemoveBond.gameObject.SetActive(true);
            yield return new WaitUntil(RemoveBond);
            pinchToRemoveBond.gameObject.SetActive(false);
            yield return new WaitForSeconds(1f);
            isTutorial = false;
            PlayerPrefs.SetInt(tutorialKey, 1);
            PlayerPrefs.Save();
           // bookManager.bookText.gameObject.SetActive(false);
            atomGridInitializer.GridState(true);
            BondButtonHandler(isTutorial);
        }

        //yield return new WaitUntil(TransformationEnded);
        if (hasBookTutorialStarted)
        {
            yield return new WaitUntil(SwipedPoke);
            yield return new WaitForSeconds(0.2f);
            PlayerPrefs.SetInt(firstTargetProgress, 1);
            PlayerPrefs.Save();
            pokeToSwipeRing.gameObject.SetActive(false);
            hasBookTutorialStarted = false;
            DisableAllChildren(); 
        }




    }


    public GameObject HandToSelectBondTypeObject()
    {
        return pokeToSelectBondType.gameObject;
    }

    public void OnTutorialSkipped()
    {
        atomGridInitializer.GridState(true);
        isTutorial = false;
        PlayerPrefs.SetInt(tutorialKey, 1);
        PlayerPrefs.Save();
        BondButtonHandler(isTutorial);
        DisableAllChildren();
    }

    private bool SwipedLeftCondition()
    {
        return SwipedLeft;
    }

    public bool SwipedLeft
    {
        get { return hasSwipedLeft; }
        set { hasSwipedLeft = value; }
    }
    private bool SwipedRightCondition()
    {
        return SwipedRight;
    }

    public bool SwipedRight
    {
        get { return hasSwipedRight; }
        set { hasSwipedRight = value; }
    }

    private bool SpanwedAtomCondition()
    {
        return SpawnedAtom;
    }

    public bool SpawnedAtom
    {
        get { return hasSpawnedAtom; }
        set { hasSpawnedAtom = value; }
    }

    private bool PinchMovedAtom()
    {
        return MovedAtom;
    }

    public bool MovedAtom
    {
        get { return hasMovedAtom; }
        set { hasMovedAtom = value; }
    }

    private bool PinchBondCreation()
    {
        return BondCreated;
    }

    public bool BondCreated
    {
        get { return hasCreatedBond; }
        set { hasCreatedBond = value; }
    }

    private bool BondTypeSelection()
    {
        return BondTypeSelect;
    }

    public bool BondTypeSelect
    {
        get { return hasSelectedBondType; }
        set { hasSelectedBondType = value; }
    } 
    private bool BondComplete()
    {
        return BondCreationComplete;
    }

    public bool BondCreationComplete
    {
        get { return isBondComplete; }
        set { isBondComplete = value; }
    } 
    private bool RemoveBond()
    {
        return BondRemoved;
    }

    public bool BondRemoved
    {
        get { return hasRemovedBond; }
        set { hasRemovedBond = value; }
    }

    private bool SwipedPoke()
    {
        return SwipedRing;
    }

    public bool SwipedRing
    {
        get { return hasSwipedRing; }
        set { hasSwipedRing = value; }
    } 
    private bool TransformationEnded()
    {
        return Transfromed;
    }

    public bool Transfromed
    {
        get { return hasTransformed; }
        set { hasTransformed = value; }
    } 
    
    public LineRenderer CurrentLinee
    {
        get { return lineRenderer; }
        set { lineRenderer = value; }
    } 
    public GameObject HandBookTutorial()
    {
       return pinchToRemoveBond.gameObject;
    }
    public bool TutorialState
    {
        get { return isTutorial; }
        set { isTutorial = value; }
    }
    public bool CanSkipTutorial
    {
        get { return isSkippable; }
        set { isSkippable = value; }
    } 
    public bool BookTutorial
    {
        get { return hasBookTutorialStarted; }
        set { hasBookTutorialStarted = value; }
    }

    void Tutorial()
    {
        isTutorial = true;
        atomGridInitializer.GridState(false);
    }
}
