using ChemistryVR.Enum;

namespace ChemistryVR.Model
{
    [System.Serializable]
    public class Bond
    {
        public BondType bondType;
        [Dropdown("validAtoms")]
        public string firstElement;
        [Dropdown("validAtoms")]
        public string secondElement;

        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Bond otherBond= (Bond) obj;
            // TODO: write your implementation of Equals() here

            return (bondType == otherBond.bondType) && (firstElement == otherBond.firstElement) && (secondElement == otherBond.secondElement);
        }
    }
}