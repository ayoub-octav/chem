using ChemistryVR.Controller;
using ChemistryVR.Enum;
using ChemistryVR.Manager;
using ChemistryVR.Model;
using ChemistryVR.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ProgressManager : MonoBehaviour
{

    public static ProgressManager Instance { get; private set; }
    public BondManager bondManager;
    public float smoothTime = 0.5f;
    public float progress = 0f;
    public UnityEvent onProgressComplete;
    public UnityEvent onProgressChange;
    public bool isDebugMod = false;

    private List<AtomVisualizer> userAtoms = new List<AtomVisualizer>();
    private List<BondHandler> userBonds = new List<BondHandler>();
    [HideInInspector] public List<string> validAtoms;

    private BookPageManager bookPageManager;
    private List<AtomWithQuantity> correctAtoms = new List<AtomWithQuantity>();
    private List<Bond> correctBonds = new List<Bond>();
    private Dictionary<int, int> atomIdNumberMap = new Dictionary<int, int>();
    private float targetProgress = 0f;
    private bool[] userBondsValidated;
    private bool isTransforming = false;
    private bool isPreTransformation = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        bookPageManager = GameObject.Find("Book").GetComponent<BookPageManager>();
        if(isDebugMod)
        {
            onProgressComplete?.Invoke();
        }

    }

    private void Update()
    {

        progress = Mathf.Lerp(progress, targetProgress, Time.deltaTime / smoothTime);
    }
    public void CalculateProgress()
    {
        correctAtoms = bookPageManager.GetCurrentCompound().atomList.GetElements();
        correctBonds = bookPageManager.GetCurrentCompound().bonds;

        int totalCorrectActions = bookPageManager.GetAtomsQuantityTotal() + correctBonds.Count;
        float actionWeight = 100f / totalCorrectActions;

        float correctAtomProgress = 0f;
        float incorrectAtomProgress = 0f;

        Dictionary<int, int> atomCountTracker = new Dictionary<int, int>();
        atomIdNumberMap.Clear();
        // Reset extraAtomCount
        //extraAtomCount.Clear();

        // Calculate progress for atoms
        foreach (var userAtomVisual in userAtoms)
        {
            var userAtom = userAtomVisual.GetAtom();
            if (correctAtoms.Any(myAtom => myAtom.element.atomicNumber == userAtom.atomicNumber))
            {
                AtomWithQuantity correctAtom = correctAtoms.FirstOrDefault(myAtom => myAtom.element.atomicNumber == userAtom.atomicNumber);

                if (correctAtom != null)
                {
                    int atomCount = atomCountTracker.ContainsKey(userAtom.atomicNumber) ? atomCountTracker[userAtom.atomicNumber] : 0;

                    if (atomCount < correctAtom.quantity)
                    {
                        correctAtomProgress += actionWeight;
                        atomCountTracker[userAtom.atomicNumber] = atomCount + 1;

                    }
                    else
                    {
                        incorrectAtomProgress += actionWeight;
                        // Track extra atoms properly

                    }
                }
            }
            else
            {
                incorrectAtomProgress += actionWeight;
            }
        }


        float correctBondProgress = 0f;
        float incorrectBondProgress = 0f;

        userBondsValidated = new bool[userBonds.Count];
        for (int i = 0; i < correctBonds.Count; i++)
        {
            Bond correctBond = correctBonds[i];
            for (int j = 0; j < userBonds.Count; j++)
            {
                if (userBondsValidated[j])
                {
                    continue;
                }
                Bond userBond = userBonds[j].Bond;
                bool isCorrectBond = VerifyBonds(userBond, correctBond);
                if (isCorrectBond)
                {
                    userBondsValidated[j] = true;
                    correctBondProgress += actionWeight;
                    break;
                }

            }
            /*if (i < correctBonds.Count)
            {
                Bond correctBond = correctBonds[i];

                bool correctOrder = userBond.firstElement == correctBond.firstElement && userBond.secondElement == correctBond.secondElement;
                bool reverseOrder = userBond.firstElement == correctBond.secondElement && userBond.secondElement == correctBond.firstElement;

                if (correctBond.bondType == BondType.Ionic)
                {
                    if (correctOrder && userBond.bondType == correctBond.bondType)
                    {
                        correctBondProgress += actionWeight;
                    }
                    else
                    {
                        incorrectBondProgress += actionWeight;
                    }
                }
                else
                {
                    if ((correctOrder || reverseOrder) && userBond.bondType == correctBond.bondType)
                    {

                        correctBondProgress += actionWeight;

                    }
                    else
                    {
                        incorrectBondProgress += actionWeight;
                    }
                }
            }
            else
            {
                incorrectBondProgress += actionWeight;
            }*/
        }

        for (int i = 0; i < userBondsValidated.Length; i++)
        {
            if (!userBondsValidated[i])
            {
                incorrectBondProgress += actionWeight;
            }
        }
        targetProgress = correctAtomProgress + correctBondProgress - incorrectAtomProgress - incorrectBondProgress;
        targetProgress = Mathf.Clamp(targetProgress, 0f, 100f);

        if (Mathf.Approximately(targetProgress, 100f))
        {
            isPreTransformation = true;
            onProgressComplete?.Invoke();
            //bookPageManager.CurrentMaterial.SetFloat("_EnableRing", 1);
            //AtomsColliderHandler(false);
        }
        else
        {
            isPreTransformation = false;
            onProgressChange?.Invoke();
            //bookPageManager.CurrentMaterial.SetFloat("_EnableRing", 0);
            //AtomsColliderHandler(true);
        }
  
        StartCoroutine(SmoothAdjustProgress(progress, targetProgress));
    }

    private bool CheckForTheBonds(int firstElementID, int firstElementNumber,int secondElementID,int secondElementNumber)
    {
        if (atomIdNumberMap.ContainsKey(firstElementID))
        {
            if (!atomIdNumberMap[firstElementID].Equals(firstElementNumber))
            {
                return false;
            }

        }
        else
        {
            atomIdNumberMap.Add(firstElementID, firstElementNumber);
        }

        if (atomIdNumberMap.ContainsKey(secondElementID))
        {
            if (!atomIdNumberMap[secondElementID].Equals(secondElementNumber))
            {
                return false;
            }

        }
        else
        {
            atomIdNumberMap.Add(secondElementID, secondElementNumber);
        }



        return true;
    }
    private bool VerifyBonds(Bond userBond, Bond correctBond)
    {
        if (!userBond.bondType.Equals(correctBond.bondType))
        {
            return false;
        }


        int firstElementID = GetAtomNumberFromBond(userBond.firstElement);
        int secondElementID = GetAtomNumberFromBond(userBond.secondElement);
        int firstElementNumber = GetAtomNumberFromBond(correctBond.firstElement);
        int secondElementNumber = GetAtomNumberFromBond(correctBond.secondElement);
        //bool sameNumber = SameNumberID(userBond.firstElement, correctBond.firstElement) && SameSymbolElement(userBond.secondElement, correctBond.secondElement);
        bool sameSymbol = SameSymbolElement(userBond.firstElement, correctBond.firstElement) &&
                          SameSymbolElement(userBond.secondElement, correctBond.secondElement) &&
                          userBond.bondType == correctBond.bondType;


        if (userBond.bondType.Equals(BondType.Ionic))
        {
            if (sameSymbol)
            {
                return CheckForTheBonds(firstElementID, firstElementNumber, secondElementID, secondElementNumber);
            }

        }
        else
        {
            if (sameSymbol)
            {
                if (CheckForTheBonds(firstElementID, firstElementNumber, secondElementID, secondElementNumber))
                {
                    return true;
                }
            }
            bool sameSymbolReverseOrder = SameSymbolElement(userBond.firstElement, correctBond.secondElement) &&
                    SameSymbolElement(userBond.secondElement, correctBond.firstElement) &&
                    userBond.bondType == correctBond.bondType;
            if (sameSymbolReverseOrder)
            {
                return CheckForTheBonds(firstElementID, secondElementNumber, secondElementID, firstElementNumber);
            }
        }
/*
        if (sameSymbol)
        {
            if (!CheckForTheBonds(firstElementID, firstElementNumber, secondElementID, secondElementNumber))
            {
                if (userBond.bondType.Equals(BondType.Covalent))
                {
                    bool sameSymbolReverseOrder = SameSymbolElement(userBond.firstElement, correctBond.secondElement) &&
                                           SameSymbolElement(userBond.secondElement, correctBond.firstElement) &&
                                           userBond.bondType == correctBond.bondType;
                    if (sameSymbolReverseOrder)
                    {
                        return CheckForTheBonds(firstElementID, secondElementNumber, secondElementID, firstElementNumber);
                    }
                }
            }
            else
            {
                return true;
            }

        }
        if (userBond.bondType.Equals(BondType.Covalent))
        {
            bool sameSymbolReverseOrder = SameSymbolElement(userBond.firstElement, correctBond.secondElement) &&
                                   SameSymbolElement(userBond.secondElement, correctBond.firstElement) &&
                                   userBond.bondType == correctBond.bondType;
            if (sameSymbolReverseOrder)
            {
                return CheckForTheBonds(firstElementID, secondElementNumber, secondElementID, firstElementNumber);
            }
        }*/
        return false;
    }

    private bool SameSymbolElement(string firstElement, string secondElement)
    {
        return GetAtomSymbolFromBond(firstElement).Equals(GetAtomSymbolFromBond(secondElement));
    }
    private bool SameNumberID(string firstElement, string secondElement)
    {
        return GetAtomNumberFromBond(firstElement).Equals(GetAtomSymbolFromBond(secondElement));
    }
    private IEnumerator SmoothAdjustProgress(float currentProgress, float targetProgress)
    {
        /*  float elapsedTime = 0f;

          while (elapsedTime < smoothTime)
          {
              progress = Mathf.Lerp(currentProgress, targetProgress, elapsedTime / smoothTime);
              elapsedTime += Time.deltaTime;
              yield return null;
          }

          progress = targetProgress;*/

        float elapsedTime = 0f;

        while (elapsedTime < smoothTime)
        {
            progress = Mathf.Lerp(currentProgress, targetProgress, elapsedTime / smoothTime);
            elapsedTime += Time.deltaTime;
            UpdateProgreesAmountONBook();
            yield return null;
        }

        // Ensure final value is set
        progress = targetProgress;
        UpdateProgreesAmountONBook();
    }


    private void UpdateProgreesAmountONBook()
    {
        switch (bookPageManager.GetCurrentCompound().compoundType)
        {
            case Compound.CompoundType.Liquid:
                bookPageManager.CurrentMaterial.SetFloat("_Liquid_Amount", progress / 100f);
                break;
            case Compound.CompoundType.Solid:
                bookPageManager.CurrentMaterial.SetFloat("_Solid_Amount", progress / 100f);
                break;

        }
    }
    private void ResetProgressOnBook()
    {
        switch (bookPageManager.GetCurrentCompound().compoundType)
        {
            case Compound.CompoundType.Liquid:
                bookPageManager.CurrentMaterial.SetFloat("_Liquid_Amount",0);
                break;
            case Compound.CompoundType.Solid:
                bookPageManager.CurrentMaterial.SetFloat("_Solid_Amount", 0);
                break;

        }
    }


    private void AtomsColliderHandler(bool state)
    {
        foreach (var item in userAtoms)
        {
            SphereCollider atomCol = item.GetComponent<SphereCollider>();
            atomCol.enabled = state;
        }
        
    }
    public void AddUserAtom(AtomVisualizer atom)
    {
        userAtoms.Add(atom);
        CalculateProgress();
    }

    public void RemoveUserAtom(AtomVisualizer atom)
    {

        userAtoms.Remove(atom);
        //CalculateProgress();
    }

    public void AddUserBond(BondHandler newBond, int index)
    {
        if (CheckDuplicationInList(newBond))
        {
            CalculateProgress();
            return;
        }
        if (!validAtoms.Contains(newBond.Bond.firstElement))
        {
            validAtoms.Add(newBond.Bond.firstElement);
        }
        if (!validAtoms.Contains(newBond.Bond.secondElement))
        {
            validAtoms.Add(newBond.Bond.secondElement);
        }
        userBonds.Add(newBond);
        CalculateProgress();
    }

    private bool CheckDuplicationInList(BondHandler newBond)
    {
        for (int i = 0; i < userBonds.Count; i++)
        {
           

            if (newBond.GetInstanceID() == userBonds[i].GetInstanceID())
            {
                return true;
            }
        }
        return false;
    }
    public void RemoveUserBond(BondHandler bondToRemove)
    {
        if (bondToRemove != null && userBonds.Contains(bondToRemove))
        {
      
            userBonds.Remove(bondToRemove);
            CalculateProgress();
        }
    }

    public void ClearUserAtoms()
    {
        userAtoms.Clear();
    }

    public void ClearUserBonds()
    {
        userBonds.Clear();
    }


    public float GetProgress()
    {
        return progress;
    }

    public void ResetProgress()
    {
        progress = 0f;
        targetProgress = 0f;
    }

    public void ClearUserActions()
    {
        ResetProgressOnBook();
        ResetProgress();
        ClearUserAtoms();
        ClearUserBonds();
        CalculateProgress();
        bondManager.RemoveRemainBondsOnSwipe();
    }

    private string GetAtomSymbolFromBond(string atomSymbol)
    {
        return atomSymbol.Split('|')[0];
    }

    private int GetAtomNumberFromBond(string atomSymbol)
    {
        var numberText = atomSymbol.Split('|')[1];
        return int.Parse(numberText.Split(":")[1]);
    }

    public bool Transforming
    {
        get { return isTransforming; }
        set { isTransforming = value; }
    }  
    public bool IsAboutToTransfrom
    {
        get { return isPreTransformation; }
        set { isPreTransformation = value; }
    }
}
