using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChemistryVR.Model
{
    [Serializable]
    public class AtomList
    {
        [SerializeField] private List<AtomWithQuantity> atoms;
        [HideInInspector] public UnityEvent onChange;
        public AtomList()
        {
            atoms = new List<AtomWithQuantity>();
        }
        public AtomList(List<AtomWithQuantity> atoms)
        {
            this.atoms = atoms;
        }

        public void Add(AtomWithQuantity atom)
        {
            atoms.Add(atom);
            onChange?.Invoke();
        }
        public void AddRange(List<AtomWithQuantity> atoms)
        {
            atoms.AddRange(atoms);
            onChange?.Invoke();
        }
        public void Remove(AtomWithQuantity atom)
        {
            atoms.Remove(atom);
            onChange?.Invoke();
        }
        public void Clear()
        {
            atoms.Clear();
            onChange?.Invoke();
        }
        public List<AtomWithQuantity> GetElements()
        {
            return atoms;
        }

        public bool Exists(Predicate<AtomWithQuantity> value)
        {
            return atoms.Exists(value);
        }
    }
}