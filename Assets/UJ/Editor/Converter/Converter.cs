using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using System.Collections;
using UJ.Language; // Required for Str, StrUsage, StrUsageKey, IFromString, Str.Language
using UJ.Language.Editor; // Required for StrUsageKey etc. if defined there
using System.Security.Cryptography;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UJ.Converter{

    public class Converter
    {
        private const string SchemaKeyPrefix = "XlsxConverter_Schema_";
        private const string VersionKeyPrefix = "XlsxConverter_Version_";

#if UNITY_EDITOR
        private const string HashKeyPrefix = "XlsxConverter_Hash_";

        private static string ComputeHash(string filePath)
        {
            try
            {
                using (var md5 = MD5.Create())
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var hashBytes = md5.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to compute hash for {filePath}: {ex}");
                return string.Empty;
            }
        }

        public static XLWorkbook CreateWorkbook(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return null;
            }

            try
            {
                var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return new XLWorkbook(fs);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating workbook: {ex.Message}");
                return null;
            }
        }

        public static IList ImportFromExcel<TItem>(
            IXLWorksheet worksheet,
            Dictionary<StrUsageKey, StrUsage> strUsages,
            string hostName
            ) where TItem : new()
        {
            var list = new List<TItem>();
            var firstRow = worksheet.FirstRowUsed();
            if (firstRow == null) return list;

            var headers = firstRow.CellsUsed().Select(c => c.Value.ToString().Trim()).ToList();
            var properties = typeof(TItem).GetFields(BindingFlags.Public | BindingFlags.Instance)
                                        .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var headerMap = new Dictionary<int, FieldInfo>();
            int codeColIndex = -1;
            for (int i = 0; i < headers.Count; i++)
            {
                string headerName = headers[i];
                if (properties.TryGetValue(headerName, out var prop))
                {
                    headerMap.Add(i + 1, prop);
                }
                else
                {
                    Debug.LogWarning($"Header '{headerName}' in worksheet '{worksheet.Name}' does not match any public instance field in type '{typeof(TItem).Name}'. Ignoring column.");
                }
                if (string.Equals(headerName, "code", StringComparison.OrdinalIgnoreCase))
                {
                    codeColIndex = i + 1;
                }
            }

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                if (IsRowEmpty(row)) continue;

                var item = new TItem();
                int currentCode = -1;

                if (codeColIndex != -1)
                {
                    var codeCell = row.Cell(codeColIndex);
                    if (codeCell.Value != null && int.TryParse(codeCell.Value.ToString(), out int parsedCode))
                    {
                        currentCode = parsedCode;
                    }
                    else
                    {
                        Debug.LogWarning($"Row {row.RowNumber()} in worksheet '{worksheet.Name}' has missing or invalid code in column {codeColIndex}. Str fields might not be processed correctly.");
                    }
                }

                foreach (var kvp in headerMap)
                {
                    int colIndex = kvp.Key;
                    FieldInfo field = kvp.Value;
                    var cell = row.Cell(colIndex);
                    var cellValue = cell.Value;

                    try
                    {
                        if (cellValue != null)
                        {
                            string stringValue = cellValue.ToString();

                            if (field.FieldType == typeof(Str))
                            {
                                if (currentCode == -1)
                                {
                                    Debug.LogWarning($"Cannot process Str field '{field.Name}' in row {row.RowNumber()} of '{worksheet.Name}' because the 'code' is missing or invalid for this row.");
                                    continue;
                                }

                                string korValue = stringValue;
                                var key = new StrUsageKey { code = currentCode, hostName = hostName, fieldName = field.Name };
                                Str strInstance = null;

                                if (strUsages.TryGetValue(key, out StrUsage existingUsage))
                                {
                                    if (existingUsage.kr != korValue)
                                    {
                                        strInstance = new Str { code = currentCode, kor = korValue };
                                        existingUsage.kr = korValue;
                                        existingUsage.ResetLanguesExcept(Str.Language.Kor);
                                        if (!existingUsage.isNew) existingUsage.updated = true;
                                        Debug.Log($"StrUsage updated for Code={currentCode}, Host={hostName}, Field={field.Name}. New KR='{korValue}'. Other languages reset.");
                                    }
                                    else
                                    {
                                        strInstance = existingUsage.ToStr();
                                    }
                                }
                                else
                                {
                                    strInstance = new Str { code = currentCode, kor = korValue };
                                    var newUsage = new StrUsage
                                    {
                                        code = currentCode,
                                        hostName = hostName,
                                        fieldName = field.Name,
                                        kr = korValue,
                                        isNew = true
                                    };
                                    strUsages[key] = newUsage;
                                    Debug.Log($"New StrUsage added for Code={currentCode}, Host={hostName}, Field={field.Name}. KR='{korValue}'");
                                }
                                field.SetValue(item, strInstance);
                            }
                            else if (typeof(IFromString).IsAssignableFrom(field.FieldType))
                            {
                                if (!string.IsNullOrWhiteSpace(stringValue))
                                {
                                    var instance = Activator.CreateInstance(field.FieldType);
                                    ((IFromString)instance).LoadFromString(stringValue);
                                    field.SetValue(item, instance);
                                }
                                else
                                {
                                    field.SetValue(item, null);
                                }
                            }
                            else
                            {
                                if (cell.TryGetValue(out object convertedValue))
                                {
                                    var targetType = field.FieldType;
                                    var underlyingType = Nullable.GetUnderlyingType(targetType);
                                    var finalValue = Convert.ChangeType(convertedValue, underlyingType ?? targetType);
                                    field.SetValue(item, finalValue);
                                }
                                else if (!string.IsNullOrWhiteSpace(stringValue))
                                {
                                    var targetType = field.FieldType;
                                    var underlyingType = Nullable.GetUnderlyingType(targetType);
                                    var fallbackValue = Convert.ChangeType(stringValue, underlyingType ?? targetType);
                                    field.SetValue(item, fallbackValue);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error converting value '{cell.Value}' to type '{field.FieldType.Name}' for field '{field.Name}' in row {row.RowNumber()}, worksheet '{worksheet.Name}'. Error: {ex.Message}\n{ex.StackTrace}");
                    }
                }
                list.Add(item);
            }
            return list;
        }

        private static bool IsRowEmpty(IXLRow row)
        {
            return row.CellsUsed().All(cell =>
                cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()));
        }

        public static void HandleFileChange(string assetPath, string fullPath)
        {
            /*
           string hashPrefKey = HashKeyPrefix + assetPath;
           string currentHash = ComputeHash(fullPath);


           string lastHash = EditorPrefs.GetString(hashPrefKey, string.Empty);

           if (currentHash == lastHash && !string.IsNullOrEmpty(lastHash))
           {
               return;
           }
           */

            XlsxConvertConfig config = LoadConfig();
            if (config == null) return;

            if (config.entries.Any(e => e.inputAssetPath == assetPath))
            {
                SyncExcelAssetWithConfig(assetPath, config);
            }

      //      EditorPrefs.SetString(hashPrefKey, currentHash);
        }

        public static void SyncExcelAssetWithConfig(string assetPath, XlsxConvertConfig config = null)
        {
            if (config == null)
            {
                config = LoadConfig();
                if (config == null) return;
            }

            XlsxConvertConfig.Entry entry = config.entries.FirstOrDefault(e => e.inputAssetPath == assetPath);

            if (entry != null)
            {
                SyncExcelAsset(assetPath, entry);
            }
        }

        private static void SyncExcelAsset(string assetPath, XlsxConvertConfig.Entry entry)
        {
            string projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
            string fullPath = Path.Combine(projectRoot, assetPath);

            string strUsageAssetPath = Path.ChangeExtension(assetPath, null) + "_Usage.xlsx";
            string strUsageFullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), strUsageAssetPath);

            Dictionary<StrUsageKey, StrUsage> strUsages = StrProcessor.LoadStrUsages(strUsageFullPath);
            foreach (var usage in strUsages.Values) { usage.Reset(); }

            ScriptableObject obj = null;
            bool isNewAsset = false;

            string outFolder = entry.outputFolder;
            if (!outFolder.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError($"[XlsxConverter] Output folder '{outFolder}' must be inside the Assets folder.");
                return;
            }
            string outAssetName = string.IsNullOrEmpty(entry.outputAssetName) ? Path.GetFileNameWithoutExtension(assetPath) : entry.outputAssetName;
            string outAssetPath = Path.Combine(outFolder, $"{outAssetName}.asset").Replace("\\", "/");

            var targetType = entry.targetScript?.GetClass();
            if (targetType == null || !typeof(ScriptableObject).IsAssignableFrom(targetType))
            {
                Debug.LogError($"[XlsxConverter] Invalid target script or type for '{assetPath}'. Skipping.");
                return;
            }

            obj = AssetDatabase.LoadAssetAtPath(outAssetPath, targetType) as ScriptableObject;

            if (obj == null)
            {
                obj = ScriptableObject.CreateInstance(targetType);
                isNewAsset = true;
                string absOutFolder = Path.Combine(projectRoot, outFolder);
                if (!Directory.Exists(absOutFolder)) { try { Directory.CreateDirectory(absOutFolder); } catch (Exception ex) { Debug.LogError($"[XlsxConverter] Failed to create output directory '{absOutFolder}': {ex.Message}"); return; } }
            }

            using (var workbook = CreateWorkbook(fullPath))
            {
                if (workbook == null) return;

                var listFields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.FieldType.IsGenericType &&
                                f.FieldType.GetGenericTypeDefinition() == typeof(List<>) &&
                                f.GetCustomAttribute<IgnoreFromXlsxAttribute>() == null);

                foreach (var field in listFields)
                {
                    string sheetName = field.Name;
                    if (!workbook.Worksheets.TryGetWorksheet(sheetName, out var worksheet))
                    {
                        Debug.LogWarning($"[XlsxConverter] Worksheet '{sheetName}' not found in '{assetPath}'. Skipping field '{field.Name}'.");
                        continue;
                    }
                    var itemType = field.FieldType.GetGenericArguments()[0];
                    var importMethod = typeof(Converter).GetMethod("ImportFromExcel", BindingFlags.Static | BindingFlags.Public);
                    var genericImportMethod = importMethod.MakeGenericMethod(itemType);
                    var listData = genericImportMethod.Invoke(null, new object[] { worksheet, strUsages, sheetName }) as IList;
                    if (listData != null)
                    {
                        field.SetValue(obj, listData);
                    }
                    else
                    {
                        Debug.LogError($"[XlsxConverter] ImportFromExcel returned null for worksheet '{sheetName}' and type '{itemType.Name}' in '{assetPath}'. Field '{field.Name}' will be empty.");
                    }
                }
            }

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(obj, outAssetPath);
                UJ.Log.Print($"[XlsxConverter] Created new ScriptableObject at '{outAssetPath}'.");
            }
            else
            {
                EditorUtility.SetDirty(obj);
                UJ.Log.Print($"[XlsxConverter] Updated existing ScriptableObject at '{outAssetPath}'.");
            }
            AssetDatabase.SaveAssets();

            bool usagesChanged = strUsages.Values.Any(u => u.isNew || u.updated);
            if (usagesChanged)
            {
                if (File.Exists(strUsageFullPath))
                {
                    string backupPath = strUsageFullPath + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak";
                    try { File.Copy(strUsageFullPath, backupPath, true); }
                    catch (Exception ex) { Debug.LogError($"[XlsxConverter] Failed to create backup for {strUsageFullPath}: {ex.Message}"); }
                }
                StrProcessor.SaveStrUsages(strUsageFullPath, strUsages);
                AssetDatabase.ImportAsset(strUsageAssetPath, ImportAssetOptions.ForceUpdate);
                UJ.Log.Print($"[XlsxConverter] Updated StrUsage file at '{strUsageAssetPath}'.");

                UpdateSourceExcelFromUsages(assetPath, strUsages);
            }

            AssetDatabase.Refresh();
        }

        private static void UpdateSourceExcelFromUsages(string sourceAssetPath, Dictionary<StrUsageKey, StrUsage> strUsages)
        {
            string sourceFullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), sourceAssetPath);

            if (!File.Exists(sourceFullPath))
            {
                Debug.LogError($"[XlsxConverter] Cannot update source Excel file: File not found at '{sourceFullPath}'.");
                return;
            }

            var relevantUsages = strUsages.Values
                                          .Where(u => u.isNew || u.updated)
                                          .GroupBy(u => u.hostName);

            if (!relevantUsages.Any()) return;

            bool fileModified = false;
            try
            {
                using (var fs = new FileStream(sourceFullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var workbook = new XLWorkbook(fs))
                {
                    foreach (var sheetGroup in relevantUsages)
                    {
                        string sheetName = sheetGroup.Key;
                        if (!workbook.Worksheets.TryGetWorksheet(sheetName, out var sheet))
                        {
                            Debug.LogWarning($"[XlsxConverter] Cannot update sheet '{sheetName}' in '{sourceAssetPath}': Worksheet not found.");
                            continue;
                        }

                        var codeColumn = sheet.ColumnsUsed(c => string.Equals(c.FirstCell().Value.ToString(), "code", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (codeColumn == null)
                        {
                            Debug.LogWarning($"[XlsxConverter] Cannot update sheet '{sheetName}' in '{sourceAssetPath}': 'code' column not found.");
                            continue;
                        }
                        var rowLookup = codeColumn.CellsUsed().Skip(1).Where(c => c.Value != null && int.TryParse(c.Value.ToString(), out _)).ToDictionary(c => int.Parse(c.Value.ToString()), c => c.Address.RowNumber);

                        var headerRow = sheet.FirstRowUsed();
                        if (headerRow == null) continue;
                        var columnLookup = headerRow.CellsUsed().Where(c => c.Value != null).ToDictionary(c => c.Value.ToString().Trim(), c => c.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);

                        foreach (var usage in sheetGroup)
                        {
                            if (rowLookup.TryGetValue(usage.code, out int rowNumber))
                            {
                                if (columnLookup.TryGetValue(usage.fieldName, out int colNumber))
                                {
                                    var cell = sheet.Cell(rowNumber, colNumber);
                                    string currentValue = cell.Value?.ToString() ?? "";
                                    string newValue = usage.kr;

                                    if (currentValue != (newValue ?? ""))
                                    {
                                        cell.SetValue(newValue ?? "");
                                        fileModified = true;
                                    }
                                }
                                else
                                {
                                     Debug.LogWarning($"[XlsxConverter] Column '{usage.fieldName}' not found in sheet '{sheetName}' of '{sourceAssetPath}' for code {usage.code}.");
                                }
                            }
                        }
                    }

                    if (fileModified)
                    {
                        try
                        {
                            fs.Position = 0; fs.SetLength(0); workbook.SaveAs(fs);
                            UJ.Log.Print($"[XlsxConverter] Updated source Excel file '{sourceAssetPath}' with string changes.");
                        }
                        catch (IOException ex) { Debug.LogError($"[XlsxConverter] Failed to save updated source Excel file '{sourceAssetPath}' (file might be locked): {ex.Message}"); }
                    }
                }
            }
            catch (IOException ex) { Debug.LogError($"[XlsxConverter] Error accessing source Excel file '{sourceFullPath}' for update: {ex.Message}"); }
            catch (Exception ex) { Debug.LogError($"[XlsxConverter] Error updating source Excel file '{sourceFullPath}': {ex.ToString()}"); }

            if (fileModified)
            {
                AssetDatabase.ImportAsset(sourceAssetPath, ImportAssetOptions.ForceUpdate);
            }
        }

        private static XlsxConvertConfig LoadConfig()
        {
            var guids = AssetDatabase.FindAssets("t:XlsxConvertConfig");
            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<XlsxConvertConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            Debug.LogError("[XlsxConverter] XlsxConvertConfig asset not found.");
            return null;
        }

#endif
    }

    public class IgnoreFromXlsxAttribute : Attribute { }
}