using ChemistryVR.Model;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryVR.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Compound", menuName = "ScriptableObjects/Compound", order = 1)]
    public class Compound : ScriptableObject
    {
        public string compoundName;
        public Texture2D bookContentTexture;
        public int sideAtomsLimit = 3;
        public Texture2D factsTexture;
        public int factsLength = 0;
        public Color compoundColor;
        public CompoundType compoundType;

        public enum CompoundType
        {
            Liquid,
            Solid
        }
        [HideInInspector]
        public AtomList atomList = new AtomList();
        public List<Bond> bonds = new List<Bond>();

        [HideInInspector] public List<string> validAtoms;

        private void OnValidate()
        {
            UpdateValidAtoms();
            atomList.onChange.AddListener(UpdateValidAtoms);
            InitializeBonds();
        }

        private void InitializeBonds()
        {
            if (atomList.GetElements().Count >= 2) // Ensure there are enough atoms to assign
            {
                for (int i = 0; i < bonds.Count; i++)
                {
                    Bond bond = bonds[i];

                    if (string.IsNullOrEmpty(bond.firstElement) || !validAtoms.Contains(bond.firstElement))
                    {
                        bond.firstElement = validAtoms[0]; // Assign the first valid atom
                    }

                    if (string.IsNullOrEmpty(bond.secondElement) || !validAtoms.Contains(bond.secondElement))
                    {
                        bond.secondElement = validAtoms[1]; // Assign the second valid atom, if available
                    }
                }
            }
        }
        public void UpdateValidAtoms()
        {
            validAtoms.Clear();
            foreach (var atom in atomList.GetElements())
            {
                for(int i = 1; i <= atom.quantity; i++)
                {
                    validAtoms.Add(atom.element.symbol + "| Number: " + i);
                }
            }
        }
        public override string ToString()
        {
            string output = $"Compound: {compoundName}\n";

            foreach (AtomWithQuantity element in atomList.GetElements())
            {
                output += $"Atoms: {element.element.name} | Quantity: {element.quantity}\n";
            }

            return output;
        }
        public AtomWithQuantity GetPeriodicElementBySymbol(string symbol)
        {
            foreach (AtomWithQuantity elementQuantity in atomList.GetElements())
            {
                if (elementQuantity.element.symbol == symbol)
                {
                    return elementQuantity;
                }
            }

            return null;
        }

        private void OnEnable()
        {
            foreach (var item in atomList.GetElements())
            {
                item.element.spawnCounter = 0;
                item.element.isSpawnlimit = false;
            }
          
        }
    }
}