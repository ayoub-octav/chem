using ChemistryVR.Controller;
using ChemistryVR.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryVR.Manager
{
    public class TransformationManager : MonoBehaviour
    {
        [SerializeField] private Slider transformationSlider;
        [SerializeField] private float animationDuration = 2;
        [SerializeField] private float curvinessTiming = 2;
        [SerializeField, Range(0, 1)] private float transformationPercentage;
        [SerializeField, Range(0, 10)] private float curvinessFactor = 1;
        [SerializeField] private float transformationSpeed = 1;
        [SerializeField] private Transform transformationCenter;
        public GameObject transformationEffect;
        public GameObject preTransformationEffect;

        private bool isRevers = false;
        private Coroutine animationCoroutine;
        private Animator animator;

        private AtomVisualizer[] atomsObjects;
        private BondController[] bondObject;
        private AtomVisualizer[] atomColliders;
        private AtomGridInitializer atomGridInitializer;
        private BookPageManager bookPageManager;
        private bool isSliderValueCHnaged = false;
        [Serializable]
        private class AtomData
        {
            public Vector3 initialPos;
            public Transform transform;

            public AtomData(Vector3 initialPos, Transform transform)
            {
                this.initialPos = initialPos;
                this.transform = transform;
            }
        }
      
        private AtomData[] atoms;


        private void Start()
        {
            transformationSlider.onValueChanged.AddListener(OnValueChange);
            bookPageManager = transformationSlider.GetComponentInParent<BookPageManager>();

            atomGridInitializer = bookPageManager.atomCreator.GetComponent<AtomGridInitializer>();
        }

        private void OnValueChange(float newValue)
        {
            ResetCoroutine();

            if (newValue > 0.9f && transformationPercentage != 1)
            {
                PrepareTransformation();
                isSliderValueCHnaged = true;
                atomGridInitializer.GridState(false);
                bookPageManager.UpdateTransformation(1);
                isRevers = false;
                if(!TutorialManager.Instance.TutorialState && !TutorialManager.Instance.SwipedRing)
                {
                    TutorialManager.Instance.SwipedRing = true;
                    TutorialManager.Instance.BookTutorial = true;
                }
                animationCoroutine = StartCoroutine(AnimateTransformation(1, animationDuration));
            }
            else if (newValue < 0.1f && transformationPercentage != 0)
            {

                isSliderValueCHnaged = true;
                bookPageManager.UpdateTransformation(0);
                isRevers = true;
                animationCoroutine = StartCoroutine(AnimateTransformation(0, animationDuration));
            }
        }
        private void ResetCoroutine()
        {
            if(animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = null;
        }
        private void OnEnable()
        {
            animator = transformationEffect.GetComponent<Animator>();
            animator.enabled = false;
            transformationPercentage = 0;

        
        }

        private IEnumerator AnimateTransformation(float targetValue, float completeDuration)
        {
            
            ProgressManager.Instance.Transforming = false;
            transformationSlider.interactable = false;

            if (isRevers)
            {
                preTransformationEffect.SetActive(true);
                AudioManager.Instance.Play("transformation_phase1_reverse", preTransformationEffect.GetComponent<AudioSource>());
                yield return new WaitForSeconds(1f);
                animator.enabled = true;
                animator.SetTrigger("TransformReverse");
                transformationEffect.SetActive(true);
                yield return new WaitForSeconds(0.3f);
                preTransformationEffect.SetActive(false);
                AudioManager.Instance.Stop(preTransformationEffect.GetComponent<AudioSource>());
                yield return new WaitForSeconds(0.19f);
                HideShowAtomAndBons(true);
                yield return new WaitForSeconds((animator.GetCurrentAnimatorStateInfo(0).length - 1));
                
                //transformationEffect.SetActive(false);
            }
            else
            {
                preTransformationEffect.SetActive(true);
                AudioManager.Instance.Play("transformation_phase1", preTransformationEffect.GetComponent<AudioSource>());
            }
            AtomsColliderHandler(isRevers);
            var startTime = Time.time;
            var duration = completeDuration * Mathf.Abs(transformationPercentage - targetValue);
            float startValue = transformationPercentage;
            while (true)
            {
                transformationPercentage = startValue + (targetValue - startValue) * ((Time.time - startTime) / duration);
                if(Time.time - startTime > duration)
                {
                    isSliderValueCHnaged = false;
                    preTransformationEffect.SetActive(false);
                    break;
                }
                yield return null;
            }
            transformationPercentage = targetValue;
            if(!isRevers)
            {
                animator.enabled = true;
                animator.SetTrigger("Transform");
                transformationEffect.SetActive(true);
                yield return new WaitForSeconds(0.3f);
                preTransformationEffect.SetActive(false);
                AudioManager.Instance.Stop(preTransformationEffect.GetComponent<AudioSource>());
                yield return new WaitForSeconds((animator.GetCurrentAnimatorStateInfo(0).length / 1.5f));
                HideShowAtomAndBons(false);
                //transformationEffect.SetActive(false);
            }
          
            yield return new WaitForEndOfFrame();
            transformationSlider.value = targetValue;
            ProgressManager.Instance.Transforming = false;
            transformationSlider.interactable = true;

            if(targetValue <= 0)
            {
                atomGridInitializer.GridState(true);
            }

        }

        public void PrepareTransformation()
        {
            var atomsObjects = FindObjectsByType<AtomVisualizer>(FindObjectsSortMode.None);
            atoms = new AtomData[atomsObjects.Length];
            for (int i = 0; i < atoms.Length; i++)
            {
                var atomTransform = atomsObjects[i].transform;
                atoms[i] = new AtomData(atomTransform.position, atomTransform);
            }
        }
        private void AtomsColliderHandler(bool state)
        {
            if(!state)
            {
                atomColliders = FindObjectsByType<AtomVisualizer>(FindObjectsSortMode.None);
            }
            else
            {
                for (int i = 0;i < atomColliders.Length;i++)
                {
                    SphereCollider atomCol = atomColliders[i].GetComponent<SphereCollider>();
                    atomCol.enabled = state;
                }
            }
        }

        private void HideShowAtomAndBons(bool state)
        {
            if (!state)
            {
                atomsObjects = FindObjectsByType<AtomVisualizer>(FindObjectsSortMode.None);
                bondObject = FindObjectsByType<BondController>(FindObjectsSortMode.None);
            }

            if (!state)
            {
                foreach (var item in bondObject)
                {
                    item.StopElectronMovement();
                    item.gameObject.SetActive(state);
                }
                foreach (var atomsObject in atomsObjects)
                {
                    atomsObject.gameObject.SetActive(state);
                }
            }
            else
            {
                foreach (var atomsObject in atomsObjects)
                {
                    atomsObject.gameObject.SetActive(state);
                }
                foreach (var item in bondObject)
                {
                    item.StartElectronMovement();
                    item.gameObject.SetActive(state);
                }
         
            }


        }
        private void Update()
        {
            if (isSliderValueCHnaged)
            {
                foreach (AtomData atom in atoms)
                {
                    if (atom != null)
                    {
                        Vector3 directonalVector = atom.initialPos - transformationCenter.position;
                        float distance = directonalVector.magnitude * (1 - transformationPercentage);
                        directonalVector.Normalize();

                        var targetPos = transformationCenter.position + Quaternion.AngleAxis(360 * curvinessFactor * Mathf.Pow(transformationPercentage, curvinessTiming), Vector3.up) * directonalVector * distance;
                        atom.transform.position = Vector3.Lerp(atom.transform.position, targetPos, transformationSpeed * Time.deltaTime);
/*                        if(distance <= 0 && !isRevers)
                        {
                            isSliderValueCHnaged = false;
                        }                       
                        if(distance == 1 && isRevers)
                        {
                            isSliderValueCHnaged = false;
                        }*/
                    }

                    

                }

            }
        }

       

    }

   
}