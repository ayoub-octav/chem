using ChemistryVR.Controller;
using ChemistryVR.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryVR.Manager
{
    public class BondManager : MonoBehaviour
    {
        [HideInInspector]public List<BondController> bonds = new List<BondController>();

        public void AddBond(BondController bond)
        {
            bonds.Add(bond);
        }
        public void RemoveBond(BondController bond)
        {
            bonds.Remove(bond);
        }

        public void RemoveAndUpdateBond(BondController bond)
        {
            RemoveBond(bond);
            RecalculateBondOrder();
        }

        public void StartElectronMovement(Transform secondAtom)
        {
            foreach (var bond in bonds)
            {
                if (Utils.CompareAtoms(bond.secondAtom, secondAtom))
                {
                    bond.StartElectronMovement();
                }
            }
        }

        public void RemoveAllBondsFromAtom(Transform atom)
        {
            List<BondController> burnerList = new List<BondController>();
            foreach (var bond in bonds)
            {
                if (Utils.CompareAtoms(bond.firstAtom, atom)
                 || Utils.CompareAtoms(bond.secondAtom, atom))
                {
                    burnerList.Add(bond);
                }
            }

            // Apply changes
            foreach (var bond in burnerList)
            {
                BondHandler bondHandler = bond.GetComponent<BondHandler>();
                bond.StopElectronMovement();
                bond.RemoveBond();
                ProgressManager.Instance.RemoveUserBond(bondHandler);
                Destroy(bond.gameObject);
            }
        }

        public int GetBondCountForAtom(Transform atom)
        {
            int count = 0;
            foreach (var bond in bonds)
            {
                if (bond.firstAtom == atom || bond.secondAtom == atom)
                {
                    count++;
                }
            }
            return count;
        }

        public int GetBondCount(BondController bond)
        {
            int count = -1;
            foreach (var b in bonds)
            {
                if ((b.firstAtom == bond.firstAtom && b.secondAtom == bond.secondAtom) ||
                    (b.secondAtom == bond.firstAtom && b.firstAtom == bond.secondAtom))
                {
                    count++;
                    if (b == bond)
                    {
                        return count;
                    }
                }
            }
            return count;
        }

        public int GetAtomBondCount(Transform atom)
        {
            int count = -1;
            foreach (var b in bonds)
            {
                if (b.firstAtom == atom || b.secondAtom == atom)
                {
                    count++;
                }
            }
            return count;
        }

        private void RecalculateBondOrder()
        {
            foreach (var bond in bonds)
            {
                bond.StopElectronMovement();
                bond.SetBondOrder();
            }
        }



        public void RemoveRemainBondsOnSwipe()
        {
            for (int i = bonds.Count - 1; i >= 0; i--)
            {
                if (bonds[i] != null)
                {
                    if (bonds[i].gameObject.activeSelf)
                    {
                        bonds[i].StopElectronMovement(); 
                    }
                    Destroy(bonds[i].gameObject);
                    bonds.RemoveAt(i); 
                }
            }
        }
    }
}