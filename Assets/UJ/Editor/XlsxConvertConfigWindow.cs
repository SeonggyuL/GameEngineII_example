#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System; // Required for Type
using System.IO;
using System.Collections.Generic;
using System.Linq; // Required for Linq methods
using ClosedXML.Excel;
using UJ.Converter;
using UJ.Attributes;

public class XlsxConvertConfigWindow : EditorWindow
{
    private XlsxConvertConfig config;
    private SerializedObject so;
    private ReorderableList entriesList;

    // Store valid scripts and their names
    private List<MonoScript> validScripts;
    private string[] validScriptNames;

    [MenuItem("UJ/Xlsx Convert Config Editor")]
    public static void ShowWindow() => GetWindow<XlsxConvertConfigWindow>("Xlsx Convert Config");

    private void OnEnable()
    {
        // load or create config asset
        var guids = AssetDatabase.FindAssets("t:XlsxConvertConfig");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            config = AssetDatabase.LoadAssetAtPath<XlsxConvertConfig>(path);
        }
        else
        {
            config = CreateInstance<XlsxConvertConfig>();
            AssetDatabase.CreateAsset(config, "Assets/XlsxConvertConfig.asset");
            AssetDatabase.SaveAssets();
        }

        so = new SerializedObject(config);

        // Find all MonoScripts with FromXlsxAttribute
        FindValidScripts();

        entriesList = new ReorderableList(so, so.FindProperty(nameof(XlsxConvertConfig.entries)), true, true, true, true);
        entriesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "XLSX Conversion Entries");
        // Adjust height: 3 lines (Input, Output, Target Script) + validation line + padding
        entriesList.elementHeightCallback = index =>
        {
            var elem = entriesList.serializedProperty.GetArrayElementAtIndex(index);
            var targetScriptProp = elem.FindPropertyRelative(nameof(XlsxConvertConfig.Entry.targetScript));
            var script = targetScriptProp.objectReferenceValue as MonoScript;
            bool needsValidationHelpBox = script == null || script.GetClass()?.GetCustomAttributes(typeof(FromXlsxAttribute), false).Length == 0;
            return EditorGUIUtility.singleLineHeight * (needsValidationHelpBox ? 4 : 3) + 8; // 3 fields + optional validation
        };
        entriesList.drawElementCallback = (rect, index, active, focused) =>
        {
            var elem = entriesList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            float h = EditorGUIUtility.singleLineHeight;
            var inputAssetPathProp = elem.FindPropertyRelative(nameof(XlsxConvertConfig.Entry.inputAssetPath));
            var outputFolderProp = elem.FindPropertyRelative(nameof(XlsxConvertConfig.Entry.outputFolder));
            var outputAssetNameProp = elem.FindPropertyRelative(nameof(XlsxConvertConfig.Entry.outputAssetName));
            var targetScriptProp = elem.FindPropertyRelative(nameof(XlsxConvertConfig.Entry.targetScript));

            // Input Path
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width * 0.8f, h), inputAssetPathProp);
            if (GUI.Button(new Rect(rect.x + rect.width * 0.82f, rect.y, rect.width * 0.18f, h), "Browse"))
            {
                var path = EditorUtility.OpenFilePanel("Select .xlsx Asset", Application.dataPath, "xlsx");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                    inputAssetPathProp.stringValue = "Assets" + path.Substring(Application.dataPath.Length);
            }
            rect.y += h + 2;
            // Output Folder & Asset Name
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width * 0.6f, h), outputFolderProp);
            EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.62f, rect.y, rect.width * 0.38f, h), outputAssetNameProp);
            rect.y += h + 2;

            // Target Script Popup
            var currentScript = targetScriptProp.objectReferenceValue as MonoScript;
            int currentIndex = 0; // Default to "None"
            if (currentScript != null)
            {
                // Find the index in our valid list (skip "None" at index 0)
                int foundIndex = validScripts.FindIndex(s => s == currentScript);
                if (foundIndex != -1)
                {
                    currentIndex = foundIndex + 1; // +1 because validScriptNames includes "None" at index 0
                }
            }

            EditorGUI.BeginChangeCheck();
            int selectedIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, h), "Target Script", currentIndex, validScriptNames);
            if (EditorGUI.EndChangeCheck())
            {
                if (selectedIndex == 0) // "None" selected
                {
                    targetScriptProp.objectReferenceValue = null;
                }
                else
                {
                    // Assign the selected script (adjust index for "None")
                    targetScriptProp.objectReferenceValue = validScripts[selectedIndex - 1];
                }
            }
            rect.y += h + 2; // Move down for potential validation message

            // Validation (only check if a script is assigned)
            var script = targetScriptProp.objectReferenceValue as MonoScript;
            Type scriptClass = script?.GetClass(); // Get class only if script is not null
            // Show warning if script is assigned but doesn't have the attribute OR if no script is assigned (user needs to select one)
            if (script == null || scriptClass == null || scriptClass.GetCustomAttributes(typeof(FromXlsxAttribute), false).Length == 0)
            {
                string message = script == null ? "Please select a Target Script." : "Target script must have [FromXlsxAttribute] class-level.";
                EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, h), message, MessageType.Warning);
            }
        };
        entriesList.onAddCallback = list =>
        {
            list.serializedProperty.arraySize++;
            so.ApplyModifiedProperties();
        };
    }

    private void FindValidScripts()
    {
        validScripts = new List<MonoScript>();
        var tempNames = new List<string> { "None" }; // Start with "None" option

        // Find all MonoScript assets in the project
        var guids = AssetDatabase.FindAssets("t:MonoScript");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null)
            {
                var type = script.GetClass();
                // Check if the class exists and has the attribute
                if (type != null && type.GetCustomAttributes(typeof(FromXlsxAttribute), false).Length > 0)
                {
                    validScripts.Add(script);
                    tempNames.Add($"{type.Name} ({type.Namespace})"); // Add name with namespace for clarity
                }
            }
        }
        validScriptNames = tempNames.ToArray();
    }

    private void OnGUI()
    {
        if (config == null || so == null)
        {
            // Try re-initializing if null, might happen during recompiles
            OnEnable();
            if (config == null || so == null) return; // If still null, exit
        }

        so.Update();

        entriesList.DoLayoutList();
        so.ApplyModifiedProperties();

        // Validate entries only
        bool allValid = true;

        for (int i = 0; i < config.entries.Count; i++)
        {
            var entry = config.entries[i];
            // Validation logic remains the same, checking the assigned script
            bool isEntryValid = !string.IsNullOrEmpty(entry.inputAssetPath) &&
                                !string.IsNullOrEmpty(entry.outputFolder) &&
                                !string.IsNullOrEmpty(entry.outputAssetName) &&
                                entry.targetScript != null && // Ensure a script is selected
                                entry.targetScript.GetClass()?.GetCustomAttributes(typeof(FromXlsxAttribute), false).Length > 0;

            if (!isEntryValid)
            {
                allValid = false;
                // No need to break here if we want to show the master path error as well
            }
        }

        if (!allValid)
        {
            // Simplified HelpBox message
            EditorGUILayout.HelpBox("Some entries are invalid. Please fill all required fields and select a valid Target Script with [FromXlsxAttribute].", MessageType.Error);
        }

        EditorGUI.BeginDisabledGroup(!allValid);
        GUILayout.Space(8);
        if (GUILayout.Button("Save Config"))
        {
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Saved", "Configuration saved.", "OK");
        }
        EditorGUI.EndDisabledGroup();
    }
}
#endif