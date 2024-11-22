using TMPro;
using UnityEngine;
using ChemistryVR.Controller;
using System;
using ChemistryVR.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using ChemistryVR.Model;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class BookPageManager : MonoBehaviour
{
    //public TextMeshPro bookText;
    [Space(20)]
    public ChemistryVR.Controller.SwipeGesture swipeGestureRight;
    public ChemistryVR.Controller.SwipeGesture swipeGestureLeft;
    public AtomCreator atomCreator;
    public List<CompoundAndVisualHolder> compoundList;
    public Material liquidMaterial;
    public Material solidMaterial;
    public float fadeDuration = 2f;
    public List<AtomWithQuantity> elements;
    public UnityEvent onPageSwitched;

    public int atomMax;


    public Slider transformationSlider;
    public bool enableBook = false;


    private int previousIndex;
    private float elapsedTime = 0f;
    private float contentFadeAmount = 0.0f;
    private float colorElapsedTime = 0.0f;
    private float targetFadeAmount = 1f;
    private int compoundAtomTotal;


    private Color lerpedColor;
    private MeshRenderer meshRenderer;
    private int index;
    private float fadeAmount;
    private bool isSwiped = false;
    private bool isFadoutEnded = false;
    private bool isColorLerped = false;
    private int factsIndex;


    [Header("Hover effect")]
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Renderer hoverObject;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float radius;
    private TextMeshPro pageText;
    private int currentPage = 1;
    private Compound currentMolecol;
    private bool onStartUp = true;
    private Material currentMat;
    private void Start()
    {
        swipeGestureLeft.onSwipeLeft.AddListener(() => NextPage());
        swipeGestureLeft.onSwipeRight.AddListener(() => PreviousPage());

        swipeGestureRight.onSwipeLeft.AddListener(() => NextPage());
        swipeGestureRight.onSwipeRight.AddListener(() => PreviousPage());

        ProgressManager.Instance.onProgressComplete.AddListener(() => { meshRenderer.sharedMaterial.SetFloat("_EnableRing", 1); });
        ProgressManager.Instance.onProgressChange.AddListener(() => { meshRenderer.sharedMaterial.SetFloat("_EnableRing", 0); });

        transformationSlider.onValueChanged.AddListener(UpdateTransformation);
        pageText = GetComponentInChildren<TextMeshPro>();
        meshRenderer = GetComponent<MeshRenderer>();
        isSwiped = true;
        isFadoutEnded = true;
        onStartUp = true;

    }

    public void UpdateTransformation(float arg0)
    {
        Material currentMat = meshRenderer.material;
        currentMat.SetFloat("_Glow_Area", arg0);
    }



    public void NextPage()
    {
        int bookSwipeLimit = (compoundList.Count - 1);
        if (index < bookSwipeLimit && !isSwiped && !ProgressManager.Instance.Transforming)
        {
            ProgressManager.Instance.ClearUserActions();
            previousIndex = index;
            index++;
            isFadoutEnded = false;
            isSwiped = true;
            currentPage++;
            AudioManager.Instance.Play("book_swipe", GetComponent<AudioSource>());
        }
    }

    public void PreviousPage()
    {

        if (index != 0 && !isSwiped && !ProgressManager.Instance.Transforming)
        {
            ProgressManager.Instance.ClearUserActions();
            previousIndex = index;
            index--;
            isFadoutEnded = false;
            isSwiped = true;
            currentPage--;
            AudioManager.Instance.Play("book_swipe", GetComponent<AudioSource>());

        }

    }
    private void Update()
    {
        if (isSwiped == true)
        {
            ResetMaterial();
        }
    }

    private void ResetMaterial()
    {
        
        foreach (var item in compoundList)
        {
            item.visual.SetActive(false);
        }
        currentMolecol = compoundList[index].compound;
        compoundList[index].visual.SetActive(true);



        switch (currentMolecol.compoundType)
        {
            case Compound.CompoundType.Liquid:
                currentMat = liquidMaterial;
                break;
            case Compound.CompoundType.Solid:
                currentMat = solidMaterial;
                break;

        }
        onPageSwitched?.Invoke();
        if (isFadoutEnded)
        {
            currentMat.SetTexture("_Content_Texture", currentMolecol.bookContentTexture);
            currentMat.SetTexture("_Facts_Texture", currentMolecol.factsTexture);
        }
        else
        {
            Compound preCompound = compoundList[previousIndex].compound;
            Material swapMat = meshRenderer.material;
            swapMat.SetTexture("_Content_Texture", preCompound.bookContentTexture);
            swapMat.SetTexture("_Facts_Texture", preCompound.factsTexture);
        }
        FadeOutFadeIn();
        currentMat.SetInt("_Fact_Number", factsIndex);
        currentMat.SetFloat("_Content_fade_amount", fadeAmount);
        currentMat.SetFloat("_Fact_Fade_Amount", fadeAmount);
        currentMat.SetColor("_Panels_Color", LerpColor(currentMolecol));


        if (enableBook)
        {
            SetBookMaterial();
        }
    }
    public void SetBookMaterial()
    {

        meshRenderer.sharedMaterial = currentMat;
    }

    private void FadeOutFadeIn()
    {
        DestroyAllAtomsAndReset();
        if (isFadoutEnded == false)
        {
            if (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float percentageComplete = elapsedTime / fadeDuration;
                contentFadeAmount = Mathf.Lerp(targetFadeAmount, 0.0f, percentageComplete);
            }
            else
            {
                contentFadeAmount = 0.0f;
                isFadoutEnded = true;
                elapsedTime = 0.0f;

                RandomFactsNumber();
            }
        }
        else
        {

            if (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float percentageComplete = elapsedTime / fadeDuration;
                contentFadeAmount = Mathf.Lerp(0f, targetFadeAmount, percentageComplete);
            }
            else
            {
                CompoundAtomTotal();
                contentFadeAmount = targetFadeAmount;

                isSwiped = false;
                elapsedTime = 0.0f;
                AtomGridInitializer atomGridInitializer = atomCreator.GetComponent<AtomGridInitializer>();
                atomGridInitializer.Total = 0;
                if (!TutorialManager.Instance.TutorialState)
                {
                    atomGridInitializer.GridState(true); 
                }
            }
        }
        fadeAmount = contentFadeAmount;

        if(onStartUp) ProgressManager.Instance.CalculateProgress();

    }


    private void RandomFactsNumber()
    {
        Compound compound = compoundList[index].compound;

        int randomFactNum = UnityEngine.Random.Range(0, compound.factsLength);
        factsIndex = randomFactNum;
    }
    private Color LerpColor(Compound compound)
    {
        Compound previousCompound = compoundList[previousIndex].compound;

        if (previousCompound.compoundType != compound.compoundType)
        {
            Color previousCompoundColor = previousCompound.compoundColor;

            if (colorElapsedTime < fadeDuration)
            {
                if (isFadoutEnded)
                {
                    colorElapsedTime += Time.deltaTime;
                    float percentageComplete = colorElapsedTime / fadeDuration;
                    lerpedColor = Color.Lerp(previousCompoundColor, compound.compoundColor, percentageComplete);
                }
            }
            else
            {
                lerpedColor = compound.compoundColor;
                /* isColorLerped = true;*/
                previousIndex = index;
                colorElapsedTime = 0.0f;
            }
        }
        else
        {
            lerpedColor = compound.compoundColor;
        }

        return lerpedColor;

    }
    public void SetupHoverMaterial(RaycastHit hitPoint, float percentage)
    {
        if (!hoverObject.enabled)
        {
            hoverObject.enabled = true;
            trailRenderer.Clear();
            AudioManager.Instance.Play("book_hover", GetComponent<AudioSource>());
        }
        var trailRendererPos = hitPoint.point;
        trailRenderer.transform.rotation = Quaternion.LookRotation(-hitPoint.normal);
        trailRenderer.startWidth = (radius * percentage) / 2;
        hoverMaterial.SetVector("_HoverCenter", hitPoint.point);
        hoverMaterial.SetFloat("_Radius", radius * percentage);
        AudioManager.Instance.SetVolume(percentage,GetComponent<AudioSource>());
        trailRenderer.transform.position = trailRendererPos;
    }

    internal void DisableHover()
    {
        if (hoverObject.enabled)
        {
            hoverObject.enabled = false;
            trailRenderer.Clear();
            AudioManager.Instance.Stop(GetComponent<AudioSource>());
        }
    }


    private void DestroyAllAtomsAndReset()
    {
        for (int i = 0; i < atomCreator.spawnedAtoms.Count; i++)
        {
            /*AtomVisualizer atomVisualizer = atomCreator.spawnedAtoms[i].GetComponent<AtomVisualizer>();
            atomVisualizer.GetAtom().spawnCounter = 0;
            atomVisualizer.GetAtom().isSpawnlimit = false;*/
            atomCreator.DestroyAtom(atomCreator.spawnedAtoms[i].transform.position);
            Destroy(atomCreator.spawnedAtoms[i]);
            atomCreator.spawnedAtoms.Remove(atomCreator.spawnedAtoms[i]);
        }
        atomCreator.ResetColumnRow();



    }

    public Compound GetCurrentCompound() {
        return currentMolecol;
    }
    public int GetAtomsQuantityTotal()
    {
        return compoundAtomTotal;
    }
    public int CompoundAtomTotal()
    {
        elements = currentMolecol.atomList.GetElements();
        compoundAtomTotal = 0;
        foreach (var item in elements)
        {
            compoundAtomTotal += item.quantity;
        }
        atomMax = Mathf.Max(8, compoundAtomTotal);
        return atomMax;
    }

    public Material CurrentMaterial
    {
        get { return currentMat; }
        set { currentMat = value; }
    }

    public bool FlipTheBooksPage
    {
        get { return isSwiped; }
        set { isSwiped = value; }
    }

    private void OnDestroy()
    {
        ProgressManager.Instance.onProgressComplete.RemoveAllListeners();
        ProgressManager.Instance.onProgressChange.RemoveAllListeners();
    }

}
[Serializable]
public class CompoundAndVisualHolder
{
    public Compound compound;
    public GameObject visual;
}
