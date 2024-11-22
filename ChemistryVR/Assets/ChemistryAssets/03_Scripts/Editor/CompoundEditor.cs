using ChemistryVR.Model;
using ChemistryVR.ScriptableObjects;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Compound))]
public class CompoundEditor : Editor
{
    private SerializedProperty compoundName;
    private SerializedProperty bookContentTexture;
    private SerializedProperty factsTexture;
    private SerializedProperty  factsLength;
    private SerializedProperty  compoundColor;
    private SerializedProperty  compoundType;
    private SerializedProperty atomList;
    private SerializedProperty bonds;

    private void OnEnable()
    {
        compoundName = serializedObject.FindProperty("compoundName");
        bookContentTexture = serializedObject.FindProperty("bookContentTexture");
        factsTexture = serializedObject.FindProperty("factsTexture");
        factsLength = serializedObject.FindProperty("factsLength");
        compoundColor = serializedObject.FindProperty("compoundColor");
        compoundType = serializedObject.FindProperty("compoundType");
        atomList = serializedObject.FindProperty("atomList");
        bonds = serializedObject.FindProperty("bonds");
    }

    public override void OnInspectorGUI()
    {
        Compound manager = (Compound)target;
        serializedObject.Update();

        GUIStyle symbolText = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 15
        };

        GUIStyle nameText = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 15
        };

        if (GUILayout.Button("Open Periodic Table"))
        {
            PeriodicTableEditor.OpenWindow(manager);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Compound Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(compoundName);


        EditorGUILayout.PropertyField(bookContentTexture);
        if (bookContentTexture.objectReferenceValue != null)
        {
            Texture2D sprite = bookContentTexture.objectReferenceValue as Texture2D;
            if (sprite != null)
            {
                EditorGUILayout.LabelField("Book Content Preview");
                Rect rect = GUILayoutUtility.GetRect(50, 50);
                EditorGUI.DrawPreviewTexture(rect, sprite);
            }
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(factsTexture);
        if (factsTexture.objectReferenceValue != null)
        {
            Texture2D sprite = factsTexture.objectReferenceValue as Texture2D;
            if (sprite != null)
            {
                EditorGUILayout.LabelField("Facts Image Preview");
                Rect rect = GUILayoutUtility.GetRect(50, 50);
                EditorGUI.DrawPreviewTexture(rect, sprite);
            }
        }
        EditorGUILayout.PropertyField(factsLength);
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(compoundColor);
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(compoundType);
        if (manager.atomList != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Selected Element Symbols", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", EditorStyles.miniBoldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField("Symbol", EditorStyles.miniBoldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Quantity", EditorStyles.miniBoldLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            //EditorGUILayout.PropertyField(atomList, true); // 'true' to show children
            if (manager.atomList.GetElements().Count == 0)
            {
                EditorGUILayout.LabelField("No elements selected.");
            }
            else
            {
                foreach (AtomWithQuantity element in manager.atomList.GetElements())
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(element.element.name, nameText, GUILayout.Width(100));
                    GUILayout.Label(element.element.symbol, symbolText, GUILayout.Width(50));
                    GUILayout.Label(element.quantity.ToString(), symbolText, GUILayout.Width(50));

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("+", GUILayout.Height(20), GUILayout.Width(20)))
                    {
                        Undo.RecordObject(manager, "Change Quantity");
                        element.quantity++;
                        EditorUtility.SetDirty(manager);
                    }
                    if (GUILayout.Button("-", GUILayout.Height(20), GUILayout.Width(20)))
                    {
                        Undo.RecordObject(manager, "Change Quantity");
                        if (element.quantity > 1)
                        {
                            element.quantity--;
                            EditorUtility.SetDirty(manager);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.Space();
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a SelectedElements ScriptableObject.", MessageType.Info);
        }

        EditorGUILayout.PropertyField(bonds, true);

        serializedObject.ApplyModifiedProperties();
    }
}
