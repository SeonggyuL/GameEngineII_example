#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "XlsxConvertConfig", menuName = "UJ/Xlsx Convert Config", order = 100)]
public class XlsxConvertConfig : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string inputAssetPath;
        public MonoScript targetScript;
        public string outputFolder;
        public string outputAssetName;
    }

    public List<Entry> entries = new List<Entry>();
}
#endif