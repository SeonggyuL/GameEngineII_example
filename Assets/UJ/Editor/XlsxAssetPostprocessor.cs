#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq; // Required for Linq
using UnityEditor;
using UJ.Converter;
using UnityEngine;

public class XlsxAssetPostprocessor : AssetPostprocessor
{
    private const string Extension = ".xlsx";
    private const string UsageSuffix = "_Usage.xlsx"; // Define the suffix for usage files

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        string projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

        // Load the config once if there are any relevant assets
        XlsxConvertConfig config = null;
        bool configLoaded = false;

        foreach (var assetPath in importedAssets)
        {
            // Load config only when needed and only once
            if (!configLoaded)
            {
                var guids = AssetDatabase.FindAssets("t:XlsxConvertConfig");
                if (guids.Length > 0)
                {
                    config = AssetDatabase.LoadAssetAtPath<XlsxConvertConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
                else
                {
                    Debug.LogWarning("[XlsxAssetPostprocessor] XlsxConvertConfig not found. Cannot process .xlsx files.");
                    return; // Exit if config is essential
                }
                configLoaded = true; // Mark as loaded (or attempted)
            }

            // --- Handle Source .xlsx Files ---
            if (assetPath.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) && !assetPath.EndsWith(UsageSuffix, StringComparison.OrdinalIgnoreCase))
            {
                string fullPath = Path.Combine(projectRoot, assetPath);

                // Check if it's a file listed in the config entries
                if (config != null && config.entries.Any(entry => entry.inputAssetPath == assetPath))
                {
                    // Call HandleFileChange which now internally decides how to process based on config
                    Converter.HandleFileChange(assetPath, fullPath);
                }
                // else: The imported .xlsx file is not in the config, so ignore it.
            }
            // --- Handle _Usage.xlsx Files ---
            else if (assetPath.EndsWith(UsageSuffix, StringComparison.OrdinalIgnoreCase))
            {
                // Find the corresponding source .xlsx file path
                string sourceAssetPath = assetPath.Substring(0, assetPath.Length - UsageSuffix.Length) + Extension;
                string sourceFullPath = Path.Combine(projectRoot, sourceAssetPath);

                // Check if the corresponding source file exists and is in the config
                if (File.Exists(sourceFullPath) && config != null && config.entries.Any(entry => entry.inputAssetPath == sourceAssetPath))
                {
                    // Trigger processing for the SOURCE file, which will use the updated usage data
                    Debug.Log($"[XlsxAssetPostprocessor] Detected change in '{assetPath}'. Triggering update for source file '{sourceAssetPath}'.");
                    Converter.HandleFileChange(sourceAssetPath, sourceFullPath);
                }
                // else: Corresponding source file doesn't exist or isn't configured, ignore the usage file change.
            }
        }
        // AssetDatabase.Refresh() is likely called by Converter methods when needed.
    }
}
#endif