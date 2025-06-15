using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using UJ.Language;
using UJ.Language.Editor;
using System.Collections; // Added for Str, StrUsage, StrUsageKey, Str.Language enum

namespace UJ.Converter
{
    /// <summary>
    /// Handles processing specific to Strs.xlsx and StrUsage data.
    /// </summary>
    public static class StrProcessor
    {
        public static Dictionary<StrUsageKey, StrUsage> LoadStrUsages(string filePath)
        {
            var result = new Dictionary<StrUsageKey, StrUsage>();

            if (!File.Exists(filePath))
            {
                // If Strs.xlsx doesn't exist yet, we might be creating StrUsage for the first time.
                // If StrUsage.xlsx exists, load from there instead.
                string usageFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", "StrUsage.xlsx");
                if (File.Exists(usageFilePath))
                {
                    Debug.Log($"Strs.xlsx not found at {filePath}. Loading existing StrUsage data from {usageFilePath}.");
                    filePath = usageFilePath; // Use StrUsage.xlsx as the source
                }
                else
                {
                    Debug.LogWarning($"Strs.xlsx not found at {filePath} and no existing StrUsage.xlsx found. Starting with empty StrUsage data.");
                    return result;
                }
            }


            try
            {
                // Use FileShare.ReadWrite to avoid locking issues if Excel is open
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var workbook = new XLWorkbook(fs))
                {
                    // Try loading from "StrUsage" sheet first (preferred), then fallback to "Strs" sheet if needed.
                    if (!workbook.Worksheets.TryGetWorksheet("StrUsage", out IXLWorksheet worksheet))
                    {
                         if (!workbook.Worksheets.TryGetWorksheet("Strs", out worksheet)) // Fallback for initial creation?
                         {
                            Debug.LogWarning("Neither 'StrUsage' nor 'Strs' worksheet found in the source Excel file.");
                            return result;
                         }
                         else {
                             Debug.LogWarning("'StrUsage' worksheet not found. Attempting to read structure from 'Strs' sheet. This might be incomplete.");
                         }
                    }


                    var firstRow = worksheet.FirstRowUsed();
                    if (firstRow == null) return result; // Empty sheet

                    var headers = new List<string>();
                    foreach (var cell in firstRow.CellsUsed())
                    {
                        headers.Add(cell.Value.ToString().Trim());
                    }

                    // Map headers to column indices (1-based)
                    var headerMap = headers.Select((h, i) => new { Name = h.ToLowerInvariant(), Index = i + 1 })
                                        .ToDictionary(x => x.Name, x => x.Index);

                    // Essential columns for StrUsage identification
                    int codeCol = -1, hostNameCol = -1, fieldNameCol = -1;
                    if (headerMap.ContainsKey("code")) codeCol = headerMap["code"];
                    if (headerMap.ContainsKey("hostname")) hostNameCol = headerMap["hostname"]; // Case-insensitive check
                    if (headerMap.ContainsKey("fieldname")) fieldNameCol = headerMap["fieldname"]; // Case-insensitive check

                    if (codeCol == -1 || hostNameCol == -1 || fieldNameCol == -1)
                    {
                        // If loading from Strs.xlsx which lacks host/field, we can't fully populate StrUsage yet.
                        // This case should ideally be handled during ProcessStrWorksheet where context is available.
                        // For now, log a warning if loading directly from a file missing these.
                         if (worksheet.Name == "StrUsage")
                         {
                            Debug.LogError("StrUsage sheet must contain 'code', 'hostName', and 'fieldName' columns.");
                            return result; // Cannot proceed without key identifiers
                         }
                         // If it's the 'Strs' sheet, we might proceed but won't load existing usages fully.
                    }


                    // Get Fields from StrUsage that represent language columns
                    var strUsageLangFields = typeof(StrUsage).GetFields(BindingFlags.Public | BindingFlags.Instance)
                        .Where(f => f.FieldType == typeof(string) &&
                                    !new[] { "hostname", "fieldname" }.Contains(f.Name.ToLowerInvariant())) // Exclude key fields
                        .ToDictionary(f => f.Name.ToLowerInvariant(), f => f);


                    foreach (var row in worksheet.RowsUsed().Skip(1)) // Skip header row
                    {
                        if (IsRowEmpty(row)) continue;

                        var usage = new StrUsage();
                        bool validRow = true;

                        // --- Read Code ---
                        if (codeCol != -1 && int.TryParse(row.Cell(codeCol).Value.ToString(), out int code))
                        {
                            usage.code = code;
                        }
                        else
                             // If code is missing or invalid, skip row entirely
                        {
                            Debug.LogWarning($"Skipping row {row.RowNumber()} in {worksheet.Name}: Invalid or missing code.");
                            validRow = false;
                            continue; // Cannot create a key without a code
                        }

                        // --- Read HostName and FieldName (if columns exist) ---
                         if (hostNameCol != -1) usage.hostName = row.Cell(hostNameCol).Value.ToString();
                         if (fieldNameCol != -1) usage.fieldName = row.Cell(fieldNameCol).Value.ToString();

                        // If loading from StrUsage sheet, hostName and fieldName are required
                        if (worksheet.Name == "StrUsage" && (string.IsNullOrWhiteSpace(usage.hostName) || string.IsNullOrWhiteSpace(usage.fieldName)))
                        {
                            Debug.LogWarning($"Skipping row {row.RowNumber()} in StrUsage: Missing hostName or fieldName.");
                            validRow = false;
                            continue;
                        }


                        // --- Read Language Columns ---
                        foreach (var kvp in strUsageLangFields)
                        {
                            string fieldNameLower = kvp.Key; // e.g., "kr", "en"
                            FieldInfo fieldInfo = kvp.Value; // The FieldInfo for StrUsage.kr, StrUsage.en etc.

                            // Find the column index for this language field in the Excel sheet
                            if (headerMap.TryGetValue(fieldNameLower, out int langColIndex))
                            {
                                var cellValue = row.Cell(langColIndex).Value;
                                if (cellValue != null)
                                {
                                    fieldInfo.SetValue(usage, cellValue.ToString());
                                }
                            }
                            // else: Column for this language doesn't exist in the sheet, leave StrUsage field as default (null)
                        }

                        // Reset flags when loading initially
                        usage.isNew = false;
                        usage.updated = false;

                        // --- Add to Dictionary ---
                        if (validRow && !string.IsNullOrWhiteSpace(usage.hostName) && !string.IsNullOrWhiteSpace(usage.fieldName))
                        {
                            var key = usage.CreateKey();
                            if (!result.ContainsKey(key))
                            {
                                result.Add(key, usage);
                            }
                            else
                            {
                                // This can happen if loading from Strs.xlsx which might have duplicate codes before processing
                                Debug.LogWarning($"Duplicate StrUsageKey found while loading: Code={key.code}, Host={key.hostName}, Field={key.fieldName}. Overwriting with data from row {row.RowNumber()}.");
                                result[key] = usage; // Overwrite with the later row's data
                            }
                        }
                        // If hostName/fieldName are missing (e.g., loading from 'Strs' sheet), we can't create a valid key yet.
                        // These entries will be created/updated during ProcessStrWorksheet.
                    }
                }
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"Error loading StrUsages from {filePath} (File possibly locked?): {ioEx.Message}\n{ioEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading StrUsages from {filePath}: {ex.ToString()}");
            }

            return result;
        }


        public static IList ProcessStrWorksheet(IXLWorksheet worksheet, Dictionary<StrUsageKey, StrUsage> strUsages, string worksheetName)
        {
            var resultList = new List<UJ.Language.Str>(); // List of Str objects for the ScriptableObject
            var firstRow = worksheet.FirstRowUsed();
            if (firstRow == null)
            {
                Debug.LogWarning($"Worksheet '{worksheetName}' is empty or has no header row.");
                return resultList;
            }

            // --- Get Headers ---
            var headers = new List<string>();
            foreach (var cell in firstRow.CellsUsed())
            {
                headers.Add(cell.Value.ToString().Trim());
            }

            // Map header names (lowercase) to column indices (1-based)
            var headerMap = headers.Select((h, i) => new { Name = h.ToLowerInvariant(), Index = i + 1 })
                                .ToDictionary(x => x.Name, x => x.Index);

            // --- Check for 'code' column ---
            if (!headerMap.TryGetValue("code", out int codeColIndex))
            {
                Debug.LogError($"Worksheet '{worksheetName}' must contain a 'code' column for Str processing.");
                return resultList; // Cannot proceed without 'code'
            }

            // --- Get Reflection Info for StrUsage and Str classes ---
            // Fields in StrUsage that represent language strings (e.g., kr, en, jp)
            var strUsageLangFields = typeof(StrUsage).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.FieldType == typeof(string) &&
                            !new[] { "hostname", "fieldname" }.Contains(f.Name.ToLowerInvariant()))
                .ToDictionary(f => f.Name.ToLowerInvariant(), f => f); // Key: "kr", "en", etc.

            // Fields in Str that represent language strings (e.g., kor, eng, jap)
            var strLangFields = typeof(Str).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.FieldType == typeof(string))
                .ToDictionary(f => f.Name.ToLowerInvariant(), f => f); // Key: "kor", "eng", etc.


            // --- Process Rows ---
            foreach (var row in worksheet.RowsUsed().Skip(1)) // Skip header row
            {
                if (IsRowEmpty(row)) continue;

                // --- Get Code ---
                var codeCell = row.Cell(codeColIndex);
                if (codeCell.Value == null || !int.TryParse(codeCell.Value.ToString(), out int currentCode))
                {
                    Debug.LogWarning($"Skipping row {row.RowNumber()} in '{worksheetName}': Invalid or missing code.");
                    continue;
                }

                // Create the Str object for this row
                var strInstance = new UJ.Language.Str { code = currentCode };

                // --- Process Language Columns ---
                foreach (var headerEntry in headerMap)
                {
                    string headerLower = headerEntry.Key; // e.g., "code", "kr", "en"
                    int colIndex = headerEntry.Value;     // Column number (1-based)
                    string originalHeader = headers[colIndex - 1]; // Original case header, e.g., "KR", "EN"

                    if (headerLower == "code") continue; // Skip the code column itself

                    // Get cell value
                    var cell = row.Cell(colIndex);
                    string cellStringValue = cell.Value?.ToString() ?? ""; // Use empty string if cell is null/empty

                    // --- Update StrUsage Dictionary ---
                    var key = new StrUsageKey
                    {
                        code = currentCode,
                        hostName = worksheetName, // The name of the sheet being processed (e.g., "Item", "Quest")
                        fieldName = originalHeader // Use the original header name as the field identifier
                    };

                    // Try to find existing StrUsage or create a new one
                    if (!strUsages.TryGetValue(key, out var usage))
                    {
                        usage = new StrUsage
                        {
                            code = currentCode,
                            hostName = key.hostName,
                            fieldName = key.fieldName,
                            isNew = true // Mark as newly created
                        };
                        strUsages[key] = usage; // Add the new usage to the dictionary
                    }
                    else
                    {
                        // Mark existing usage as potentially updated (we'll check field values)
                         usage.isNew = false; // Ensure it's not marked as new if found
                         // We will set usage.updated = true below if the value actually changes
                    }


                    // Find the corresponding language field in StrUsage (e.g., StrUsage.kr for header "kr")
                    if (strUsageLangFields.TryGetValue(headerLower, out FieldInfo usageFieldInfo))
                    {
                        // Get the current value from the StrUsage object
                        string existingValue = usageFieldInfo.GetValue(usage) as string;

                        // Compare with the value from the Excel cell
                        if (existingValue != cellStringValue)
                        {
                            // If different, update the StrUsage object and mark it as updated
                            usageFieldInfo.SetValue(usage, cellStringValue);
                            if (!usage.isNew) // Only mark as updated if it wasn't newly created in this run
                            {
                                usage.updated = true;
                            }
                        }

                        // --- Update Str Object ---
                        // Map the header name to the corresponding field name in the Str class
                        // (e.g., "kr" -> "kor", "jp" -> "jap", "en" -> "eng")
                        string strFieldName = headerLower;
                        if (strFieldName == "kr") strFieldName = "kor";
                        if (strFieldName == "jp") strFieldName = "jap";
                        // Add other mappings if necessary (e.g., "en" -> "eng")

                        // Find the field in the Str class (e.g., Str.kor)
                        if (strLangFields.TryGetValue(strFieldName, out FieldInfo strFieldInfo))
                        {
                            // Set the value in the Str object instance
                            strFieldInfo.SetValue(strInstance, cellStringValue);
                        }
                        else
                        {
                            // This indicates a mismatch between Excel headers and Str class fields
                            Debug.LogWarning($"Could not find matching field '{strFieldName}' in Str class for header '{originalHeader}' in worksheet '{worksheetName}'.");
                        }
                    }
                    else
                    {
                        // This indicates an Excel header that doesn't map to a known language field in StrUsage
                        Debug.LogWarning($"Header '{originalHeader}' in worksheet '{worksheetName}' does not correspond to a known language field in StrUsage. Ignoring column for StrUsage update.");
                    }
                } // End foreach headerEntry

                // Add the populated Str object to the list for the ScriptableObject
                resultList.Add(strInstance);

            } // End foreach row

            return resultList;
        }


        public static void SaveStrUsages(string filePath, Dictionary<StrUsageKey, StrUsage> strUsages)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("StrUsage");

                    // --- Define Header Order ---
                    // Start explicitly with code, hostName, fieldName
                    var fieldOrder = new List<string> { "code", "hostname", "fieldname" }; // Use lowercase for matching

                    // Add language fields based on Str.Language enum order
                    foreach (Str.Language lang in Enum.GetValues(typeof(Str.Language)))
                    {
                        string fieldName = lang.ToString().ToLower();
                        // Handle specific naming conventions (e.g., kor -> kr)
                        if (fieldName == "kor") fieldName = "kr";
                        if (fieldName == "jap") fieldName = "jp";
                        // Add only if not already in the initial list (shouldn't happen for languages)
                        if (!fieldOrder.Contains(fieldName))
                        {
                            fieldOrder.Add(fieldName);
                        }
                    }

                    // Get all public instance fields from StrUsage, excluding NonSerialized ones
                    var strUsageFields = typeof(StrUsage).GetFields(BindingFlags.Public | BindingFlags.Instance)
                        .Where(f => !Attribute.IsDefined(f, typeof(NonSerializedAttribute)))
                        .ToList();

                    // Add any remaining fields from StrUsage that weren't already added
                    foreach (var field in strUsageFields)
                    {
                        string fieldNameLower = field.Name.ToLowerInvariant();
                        if (!fieldOrder.Contains(fieldNameLower))
                        {
                            fieldOrder.Add(fieldNameLower); // Add at the end
                        }
                    }

                    // Map lowercase field names to FieldInfo objects
                    var fieldMap = strUsageFields.ToDictionary(f => f.Name.ToLowerInvariant(), f => f);

                    // --- Write Headers ---
                    int col = 1;
                    var orderedFields = new List<FieldInfo>(); // Store FieldInfo in the desired order
                    foreach (string fieldNameLower in fieldOrder)
                    {
                        // Use the lowercase name for lookup in the map
                        if (fieldMap.TryGetValue(fieldNameLower, out FieldInfo fieldInfo))
                        {
                            // Use the actual field name (preserving case) for the header
                            worksheet.Cell(1, col).Value = fieldInfo.Name;
                            orderedFields.Add(fieldInfo);
                            col++;
                        }
                        else
                        {
                             // This might happen if fieldOrder contains names not in StrUsage (e.g., typo)
                             Debug.LogWarning($"Field '{fieldNameLower}' defined in order but not found in StrUsage class.");
                        }
                    }

                    // --- Write Data Rows ---
                    int row = 2;
                    var sortedUsages = strUsages.Values
                                                .OrderBy(u => u.hostName)
                                                .ThenBy(u => u.code)
                                                .ThenBy(u => u.fieldName);

                    foreach (var usage in sortedUsages)
                    {
                        col = 1;
                        foreach (var fieldInfo in orderedFields)
                        {
                            object value = fieldInfo.GetValue(usage);
                            worksheet.Cell(row, col).SetValue(value ?? "");
                            col++;
                        }
                        row++;
                    }

                    // Adjust column widths to fit content
                    worksheet.Columns().AdjustToContents();

                    // --- Save Workbook ---
                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        workbook.SaveAs(fs);
                    }
                }

                Debug.Log($"Successfully saved StrUsages to {filePath}");
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"Error saving StrUsages to {filePath} (File possibly locked or permission issue?): {ioEx.Message}\n{ioEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving StrUsages: {ex.ToString()}");
            }
        }

        private static bool IsRowEmpty(IXLRow row)
        {
            // Check if all cells used in the row are null or contain only whitespace
            return row.CellsUsed().All(cell =>
                cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()));
        }
    }
}
