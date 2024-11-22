using ChemistryVR.Model;
using TMPro;
using UnityEngine;

namespace ChemistryVR.Controller
{
    public class AtomVisualizer : MonoBehaviour
    {
        public GameObject electronPrefab;
        public GameObject staticRing1Prefab;
        public GameObject staticRing2Prefab;
        public Transform[] rings = new Transform[3];
        public float[] radius = new float[3];

        private int myid;
        private Atom atom;
        private Renderer haloAtom;
        private Renderer haloNucleus;
        private TextMeshPro symbol;

        private readonly string colorName = "_Outer_Color";
        private Vector3 initialPosition;
        private void Start()
        {
            initialPosition = transform.position;
        }

        private void Update()
        {
            if (TutorialManager.Instance.TutorialState && !TutorialManager.Instance.MovedAtom)
            {
                if(transform.position != initialPosition)
                {
                    TutorialManager.Instance.MovedAtom = true;
                }
            }
        }
        private void Initialize()
        {
            haloAtom = transform.GetChild(0).GetComponent<Renderer>();
            haloNucleus = transform.GetChild(1).GetComponent<Renderer>();
            symbol = GetComponentInChildren<TextMeshPro>();
        }

        private void SetSymbol(string symbolName)
        {
            symbol.text = symbolName;
        }

        private void SetColor(Color color)
        {
            if (haloAtom != null && haloAtom.material.HasProperty(colorName))
            {
                haloAtom.material.SetColor(colorName, color);
            }
            if (haloNucleus != null && haloNucleus.material.HasProperty(colorName))
            {
                haloNucleus.material.SetColor(colorName, color);
            }
        }

        private void CreateElectrons(Atom atom)
        {
            int len = atom.electronConfiguration.Length;
            for (int i = 0; i < len; i++)
            {
                rings[i].gameObject.SetActive(true);

                int electronsCount = atom.electronConfiguration[i];
                if ((i == 0 && electronsCount == 2) || (i == 1 && electronsCount == 8))
                {
                    var ringPrefab = (i == 0) ? staticRing1Prefab : staticRing2Prefab;
                    var ringInstance = Instantiate(ringPrefab, rings[i]);
                    var ringRenderer = ringInstance.GetComponent<Renderer>();
                    if (ringRenderer != null)
                    {
                        ringRenderer.material.color = atom.color;
                    }
                    continue;
                }

                Vector3 initialPosition = new Vector3(radius[i], 0, 0);
                for (int j = 0; j < electronsCount; j++)
                {
                    var electronInstance = Instantiate(electronPrefab, rings[i]);
                    var electronRenderer = electronInstance.GetComponent<Renderer>();
                    if (electronRenderer != null)
                    {
                        electronRenderer.material.color = atom.color;
                    }
                    Quaternion electronAngularRotation = Quaternion.Euler(0, 0, j * 360 / electronsCount);
                    electronInstance.transform.localPosition = electronAngularRotation * initialPosition;
                }
            }
        }



        public void SetData(Atom data)
        {
            Initialize();
            atom = data;
            SetSymbol(data.symbol);
            SetColor(data.color);
            CreateElectrons(data);
        }

        public Atom GetAtom()
        {
            return atom;
        }

        public int GetID
        {
            get { return myid; }
            set { myid = value; }
        }
    }
}
