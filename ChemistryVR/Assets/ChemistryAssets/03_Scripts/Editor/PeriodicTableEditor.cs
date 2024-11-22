using ChemistryVR.Model;
using ChemistryVR.ScriptableObjects;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PeriodicTableEditor : EditorWindow
{
    private List<Atom> atoms = new List<Atom>();
    private readonly string jsonFileName = "PeriodicTable";
    private readonly float cellSize = 50f;
    private readonly float cellSpacing = 5f;

    private List<Compound> allSelectedElements;
    private int selectedElementIndex = 0;
    private Vector2 scrollPosition = Vector2.zero;
    private int hoveredElementIndex = -1;
    private string newCustomName = "New Compound";
    private bool isRequestedOutside = false;
    private int currentTableIndex;
    private const string SELECTEDELEMENTSFOLDERPATH = "Assets/Resources/Compounds/";
    public static void OpenWindow(Compound comp)
    {
        var myTable = GetWindow<PeriodicTableEditor>("Periodic Table");
        myTable.isRequestedOutside = true;
        int index = myTable.allSelectedElements.IndexOf(comp);
        myTable.currentTableIndex = index;
        ShowWindow();

    }
    [MenuItem("Octav/Periodic Table")]
    public static void ShowWindow()
    {
        GetWindow<PeriodicTableEditor>("Periodic Table");
    }

    private void OnEnable()
    {
        LoadElementsFromJSON();
        LoadAllSelectedElements();

        // Create a new Compound asset if none exist
        if (allSelectedElements.Count == 0)
        {
            CreateNewSelectedElements("Default Compound");
        }
    }

    private void LoadElementsFromJSON()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);
        if (jsonFile != null)
        {
            string json = jsonFile.text;
            PeriodicTableData data = JsonUtility.FromJson<PeriodicTableData>(json);
            atoms = data.atoms;
        }
        else
        {
            Debug.LogError("Periodic Table JSON file not found in Resources: " + jsonFileName);
        }
    }

    private void LoadAllSelectedElements()
    {
        allSelectedElements = new List<Compound>();
        Compound[] selectedElementsArray = Resources.LoadAll<Compound>("Compounds");
        allSelectedElements.AddRange(selectedElementsArray);
    }

    private void OnGUI()
    {
        GUILayout.Label("Periodic Table", EditorStyles.boldLabel);

        if (atoms == null || atoms.Count == 0)
        {
            EditorGUILayout.HelpBox("No elements loaded. Please check your JSON file.", MessageType.Info);
            return;
        }

        DrawSelectedElementsDropdown();
        DrawCreateNewButton();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(600));

        DrawPeriodicTable();

        EditorGUILayout.EndScrollView();
        isRequestedOutside = false;
    }

    private void DrawSelectedElementsDropdown()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Selected Compound:", GUILayout.Width(150));

        string[] options = allSelectedElements.ConvertAll(se => se.compoundName).ToArray();
        if (!isRequestedOutside)
        {
            selectedElementIndex = EditorGUILayout.Popup(selectedElementIndex, options);

        }
        else
        {
            selectedElementIndex = EditorGUILayout.Popup(currentTableIndex, options); ;
        }

        if (allSelectedElements.Count > 0 && selectedElementIndex < allSelectedElements.Count)
        {
            Compound selectedElements = allSelectedElements[selectedElementIndex];
            if (selectedElements != null)
            {
                GUILayout.Label("Custom Name:");
                selectedElements.compoundName = EditorGUILayout.TextField(selectedElements.compoundName);
                if (GUILayout.Button("Save Name"))
                {
                    EditorUtility.SetDirty(selectedElements);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    private void DrawCreateNewButton()
    {
        GUILayout.BeginHorizontal();
        newCustomName = EditorGUILayout.TextField("New Custom Name:", newCustomName);
        if (GUILayout.Button("Create New Compound"))
        {
            CreateNewSelectedElements(newCustomName);
        }
        GUILayout.EndHorizontal();
    }

    private void DrawPeriodicTable()
    {
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10
        };

        GUIStyle symbolTexture = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };

        GUIStyle hoverStyle = new GUIStyle(GUI.skin.box)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };


        GUIStyle addButtonStyle = new GUIStyle(GUI.skin.button) { };
        addButtonStyle.normal.textColor = Color.green;
        addButtonStyle.fontSize = 12;
        addButtonStyle.alignment = TextAnchor.MiddleCenter;
        addButtonStyle.padding = new RectOffset(2, 2, 2, 2);


        GUIStyle removeButtonStyle = new GUIStyle(GUI.skin.button);
        removeButtonStyle.normal.textColor = Color.red;
        removeButtonStyle.fontSize = 12;
        removeButtonStyle.alignment = TextAnchor.MiddleCenter;
        removeButtonStyle.padding = new RectOffset(2, 2, 2, 2);


        GUIStyle quantityButtonStyle = new GUIStyle(GUI.skin.button);
        quantityButtonStyle.fontSize = 12;
        quantityButtonStyle.alignment = TextAnchor.MiddleCenter;
        quantityButtonStyle.padding = new RectOffset(2, 2, 2, 2);

        GUILayout.Space(5);

        for (int row = 0; row < periodicTableLayout.GetLength(0); row++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int col = 0; col < periodicTableLayout.GetLength(1); col++)
            {
                int elementIndex = periodicTableLayout[row, col] - 1;

                GUILayout.BeginVertical(GUILayout.Width(cellSize), GUILayout.Height(cellSize));

                if (elementIndex >= 0 && elementIndex < atoms.Count)
                {
                    Atom element = atoms[elementIndex];
                    int quantity = GetQuantityForElement(element);

                    bool isHovered = hoveredElementIndex == elementIndex;
                    GUIStyle currentStyle = isHovered ? hoverStyle : symbolTexture;

                    GUILayout.Label(element.symbol, currentStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    GUILayout.Label(element.atomicNumber.ToString(), centeredStyle);
                    GUILayout.Space(cellSpacing);

                    Rect colorRect = GUILayoutUtility.GetRect(15f, 15f);
                    EditorGUI.DrawRect(colorRect, element.color);

                    GUILayout.Space(cellSpacing);

                    // Quantity control
                    if (IsElementSelected(element))
                    {
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("-", quantityButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                        {
                            if (quantity > 1)
                            {
                                UpdateQuantity(element, quantity - 1);
                            }
                        }

                        GUILayout.Label(quantity.ToString(), centeredStyle, GUILayout.Width(15));

                        if (GUILayout.Button("+", quantityButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
                        {
                            UpdateQuantity(element, quantity + 1);
                        }

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(cellSpacing);

                    if (IsElementSelected(element))
                    {
                        if (GUILayout.Button("Remove", removeButtonStyle, GUILayout.Height(15)))
                        {
                            RemoveFromSelectedElements(element);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Add", addButtonStyle, GUILayout.Height(15)))
                        {
                            AddToSelectedElements(element);
                        }
                    }

                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    Event currentEvent = Event.current;
                    if (currentEvent.type == EventType.MouseMove || currentEvent.type == EventType.Repaint)
                    {
                        if (lastRect.Contains(currentEvent.mousePosition))
                        {
                            hoveredElementIndex = elementIndex;
                            Repaint();
                        }
                    }
                }
                else
                {
                    GUILayout.Box("", GUILayout.Width(20), GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.Space(cellSpacing);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(cellSpacing);
        }
    }


    private int GetQuantityForElement(Atom element)
    {
        Compound selectedElements = allSelectedElements[selectedElementIndex];
        AtomWithQuantity ewq = selectedElements.GetPeriodicElementBySymbol(element.symbol);
        return ewq != null ? ewq.quantity : 0;
    }

    private void UpdateQuantity(Atom element, int newQuantity)
    {
        Compound selectedElements = allSelectedElements[selectedElementIndex];
        AtomWithQuantity ewq = selectedElements.GetPeriodicElementBySymbol(element.symbol);
        if (ewq != null)
        {
            ewq.quantity = newQuantity;
            EditorUtility.SetDirty(selectedElements);
            AssetDatabase.SaveAssets();
        }
    }

    private void AddToSelectedElements(Atom element)
    {
        Compound selectedElements = allSelectedElements[selectedElementIndex];
        AtomWithQuantity ewq = selectedElements.GetPeriodicElementBySymbol(element.symbol);
        if (ewq == null)
        {
            ewq = new AtomWithQuantity(element, 1);
            selectedElements.atomList.Add(ewq);
            EditorUtility.SetDirty(selectedElements);
            AssetDatabase.SaveAssets();
        }
    }

    private void RemoveFromSelectedElements(Atom element)
    {
        Compound selectedElements = allSelectedElements[selectedElementIndex];
        AtomWithQuantity ewq = selectedElements.GetPeriodicElementBySymbol(element.symbol);
        if (ewq != null)
        {
            selectedElements.atomList.Remove(ewq);
            EditorUtility.SetDirty(selectedElements);
            AssetDatabase.SaveAssets();
        }
    }

    private bool IsElementSelected(Atom element)
    {
        Compound selectedElements = allSelectedElements[selectedElementIndex];
        return selectedElements.atomList.Exists(e => e.element.symbol == element.symbol);
    }

    private void CreateNewSelectedElements(string customName)
    {
        if (!Directory.Exists(SELECTEDELEMENTSFOLDERPATH))
        {
            Directory.CreateDirectory(SELECTEDELEMENTSFOLDERPATH);
        }

        Compound newSelectedElements = ScriptableObject.CreateInstance<Compound>();
        newSelectedElements.compoundName = customName;

        string path = SELECTEDELEMENTSFOLDERPATH + customName + ".asset";
        AssetDatabase.CreateAsset(newSelectedElements, path);
        AssetDatabase.SaveAssets();

        allSelectedElements.Add(newSelectedElements);
        selectedElementIndex = allSelectedElements.Count - 1;

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newSelectedElements;
    }

    private static readonly int[,] periodicTableLayout = new int[10, 18]
    {
        {  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  2},
        {  3,  4,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  5,  6,  7,  8,  9, 10},
        { 11, 12,  0,  0,  0,  0,  0,  0,  0,  0,  0,   0, 13, 14, 15, 16, 17, 18},
        { 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36},
        { 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54},
        { 55, 56, 57, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86},
        { 87, 88, 89,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118},
        {  0,  0,  0, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71,  0},
        {  0,  0,  0, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99,100,101,102,103,  0},
        {  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0}
    };
}

[System.Serializable]
public class PeriodicTableData
{
    public List<Atom> atoms;
}
