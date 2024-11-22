using ChemistryVR.Model;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ChemistryVR.Controller
{
    public class AtomCreator : MonoBehaviour
    {
        public Transform spawnArea;
        public Vector3 spawnAreaSize = new Vector3(2, 0, 2);
        public Vector2 gridCellSize = new Vector2(0.5f, 0.5f);
        public int maxColumns = 4; 
        public List<GameObject> spawnedAtoms = new List<GameObject>();
        private AtomGridInitializer gridInitializer;

        private Dictionary<Vector2, GameObject> grid = new Dictionary<Vector2, GameObject>();

        public GameObject atomPrefab;

        private int currentRow = 0;
        private int currentColumn = 0;

        private AtomVisualizer atomVisual;
        private void Start()
        {
                gridInitializer = GetComponent<AtomGridInitializer>(); 
        }
        public void CreateAtom(Atom atom)
        {
            Vector2 gridPosition = GetNextAvailableGridPosition();

            if (gridPosition != Vector2.negativeInfinity)
            {
                Vector3 worldPosition = GetWorldPositionFromGrid(gridPosition);
                if (TutorialManager.Instance.TutorialState && TutorialManager.Instance.MovedAtom && !TutorialManager.Instance.BondCreated)
                {
                    atomVisual = Instantiate(atomPrefab, TutorialManager.Instance.bondAtomPostion.position, Quaternion.identity).GetComponent<AtomVisualizer>();
                }
                else
                {
                    atomVisual = Instantiate(atomPrefab, worldPosition, Quaternion.identity).GetComponent<AtomVisualizer>();
                }
                spawnedAtoms.Add(atomVisual.gameObject);
                atomVisual.name = atom.name;
                atomVisual.SetData(atom);
                ProgressManager.Instance.AddUserAtom(atomVisual);
                grid[gridPosition] = atomVisual.gameObject;
                atomVisual.GetID = gridInitializer.Total;

                if (TutorialManager.Instance.TutorialState && atomVisual.GetAtom().symbol.Equals("Cl") && !TutorialManager.Instance.SpawnedAtom)
                {
                    TutorialManager.Instance.SpawnedAtom = true;
                }
                UpdateGridPosition(); 
            }
            else
            {
                Debug.LogWarning("No more space to spawn new items within the defined grid and surface area.");
            }
        }

        Vector2 GetNextAvailableGridPosition()
        {
            
            foreach (var kvp in grid)
            {
                if (kvp.Value == null)
                {
                    return kvp.Key;
                }
            }

            
            Vector2 potentialGridPosition = new Vector2(currentColumn * gridCellSize.x, currentRow * gridCellSize.y);
            Vector3 worldPosition = GetWorldPositionFromGrid(potentialGridPosition);
            if (IsWithinSpawnArea(worldPosition))
            {
                return potentialGridPosition;
            }

            return Vector2.negativeInfinity; 
        }

        Vector3 GetWorldPositionFromGrid(Vector2 gridPosition)
        {
            float xPos = gridPosition.x;
            float zPos = gridPosition.y;

            return spawnArea.position + new Vector3(xPos, 0, zPos);
        }

        bool IsWithinSpawnArea(Vector3 position)
        {
            Vector3 localPosition = position - spawnArea.position;

            return localPosition.x >= 0 && localPosition.x <= spawnAreaSize.x &&
                   localPosition.z >= 0 && localPosition.z <= spawnAreaSize.z;
        }

        public void DestroyAtom(Vector2 gridPosition)
        {
            if (grid.ContainsKey(gridPosition) && grid[gridPosition] != null)
            {
                //Destroy(grid[gridPosition]);
                grid[gridPosition] = null; 
            }
        }

        void UpdateGridPosition()
        {
            currentColumn++;
            if (currentColumn * gridCellSize.x >= spawnAreaSize.x) 
            {
                currentColumn = 0;
                currentRow++;
            }
        }

        public void ResetColumnRow()
        {
            currentColumn = 0;
            currentRow = 0;
        }

    }
}
