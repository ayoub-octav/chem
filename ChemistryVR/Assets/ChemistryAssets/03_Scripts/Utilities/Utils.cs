using ChemistryVR.Model;
using UnityEngine;

namespace ChemistryVR.Utility
{
    public class Utils
    {
        public static bool CompareAtoms(Transform atom, Transform targetAtom)
        {
            if (atom == null && targetAtom == null)
            {
                return false;
            }

            return atom == targetAtom;
        }

        public static void SetMaterialColor(Renderer bondRenderer, string name, Color color)
        {
            if (bondRenderer != null && bondRenderer.material.HasProperty(name))
            {
                bondRenderer.material.SetColor(name, color);
            }
        }
    }
}
