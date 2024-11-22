using System;

namespace ChemistryVR.Model
{
    [Serializable]
    public class AtomWithQuantity
    {
        public Atom element;
        public int quantity;

        public AtomWithQuantity(Atom element, int quantity = 1)
        {
            this.element = element;
            this.quantity = quantity;
        }
    }
}