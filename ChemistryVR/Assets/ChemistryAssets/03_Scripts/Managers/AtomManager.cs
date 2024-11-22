using ChemistryVR.Controller;
using ChemistryVR.Model;
using ChemistryVR.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryVR.Manager
{
    public class AtomManager : MonoBehaviour
    {
        private AtomCreator atomCreator;
        private List<Atom> atoms = new List<Atom>();

        public List<Atom> Atoms
        {
            get { return atoms; }
            set { atoms = value; }
        }

        private void Start()
        {
            atomCreator = GetComponent<AtomCreator>();
        }

        public void LoadAtoms()
        {
            Atom[] allAtoms = new Atom[119];
            Compound[] selectedElementsArray = Resources.LoadAll<Compound>("Compounds");

            foreach (Compound element in selectedElementsArray)
            {
                foreach (AtomWithQuantity atom in element.atomList.GetElements())
                {
                    allAtoms[atom.element.atomicNumber] = atom.element;
                }
            }

            foreach (Atom atom in allAtoms)
            {
                if (atom != null)
                {
                    atoms.Add(atom);
                }
            }
        }
        public void OnAtomGridClicked(Atom atom)
        {
            atomCreator.CreateAtom(atom);
        }
    }
}