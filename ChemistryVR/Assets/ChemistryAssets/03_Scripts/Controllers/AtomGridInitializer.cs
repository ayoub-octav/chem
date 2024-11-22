using ChemistryVR.Manager;
using ChemistryVR.Model;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryVR.Controller
{
    public class AtomGridInitializer : MonoBehaviour
    {
        public Transform holder;
        public BookPageManager bookPageManager;
        [SerializeField] private Color disableColorText;
        [SerializeField] private Color enableColorText;
        [SerializeField] private int sideAtomsLimit;

        private int atomQuantity;
        private AtomManager atomManager;
        private bool isOnTheBook = false;
        private Atom currentAtom;
        private int totalQuantity;

        private void Start()
        {
            atomManager = GetComponent<AtomManager>();
            atomManager.LoadAtoms();
            if (!TutorialManager.Instance.TutorialState)
            {
                InstantiateGrid(); 
            }
        }

        private void InstantiateGrid()
        {
            int index = 0;

            foreach (var atom in atomManager.Atoms)
            {
                if (TutorialManager.Instance.TutorialState)
                {
                    if (atom.symbol.Equals("Cl"))
                    {
                        SetAtomButton(atom, 0);
                    }
                }
                else
                {
                    SetAtomButton(atom, index++);
                }
            }
        }

        public void SpawnNewAtomForTutorial()
        {
            foreach (var atom in atomManager.Atoms)
            {
                if (TutorialManager.Instance.TutorialState)
                {
                    if (atom.symbol.Equals("Na"))
                    {
                        SpawnAtoms(atom);
                    }
                } 
            }
        }
        private void SetAtomButton(Atom atom, int index)
        {
            Transform row = holder.GetChild(index / 3);
            Transform button = row.GetChild(index % 3);

            button.name = atom.symbol;

            button.GetChild(0).GetChild(0).GetComponentInChildren<TMP_Text>().text = atom.symbol;
            button.GetChild(0).GetChild(1).GetComponentInChildren<TMP_Text>().text = atom.atomicNumber.ToString();
            button.GetChild(0).GetChild(2).GetComponentInChildren<TMP_Text>().text = $"[" + string.Join(",", atom.electronConfiguration) + $"]";

            button.GetChild(0).GetComponent<Image>().color = atom.color;

            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                SpawnAtoms(atom);
            }); 
        }

        private void SpawnAtoms(Atom atom)
        {
            if (totalQuantity == bookPageManager.atomMax)
            {
                GridState(false);
            }
            else
            {
                totalQuantity += 1;
                atomManager.OnAtomGridClicked(atom);

                if (totalQuantity == bookPageManager.atomMax)
                {
                    GridState(false);
                }
            }
        }

        public void GridState(bool isActive = true)
        {
            for (int i = 0; i < 9; i++)
            {
                Transform row = holder.GetChild(i / 3);
                Transform button = row.GetChild(i % 3);

                //button.gameObject.SetActive(state ? true : false);
                button.GetChild(0).GetChild(0).GetComponentInChildren<TMP_Text>().color = isActive ? enableColorText : disableColorText;
                button.GetChild(0).GetChild(1).GetComponentInChildren<TMP_Text>().color = isActive ? enableColorText : disableColorText;
                button.GetChild(0).GetChild(2).GetComponentInChildren<TMP_Text>().color = isActive ? enableColorText : disableColorText;

                button.GetChild(0).GetComponent<Image>().color = isActive ? enableColorText : disableColorText; ;

                button.GetComponent<Button>().interactable = isActive ? true : false;
                button.GetComponent<Button>().onClick.RemoveAllListeners();
            }
            if (isActive)
            {
                InstantiateGrid();
            }
        }

        public int Total
        {
            get { return totalQuantity; }
            set { totalQuantity = value; }
        }

    }
}
