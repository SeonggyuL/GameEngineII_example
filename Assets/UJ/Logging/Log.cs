using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UJ
{

   
    public class Log
    {
        public static bool LogOn = true;
        public static void ColorLog(Color color, params object[] args)
        {
            string message = string.Join(string.Empty, args);
            Debug.Log("<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + message + "</color>");
        }

        public static void Print(params object[] args)
        {
            Debug.Log(string.Join(string.Empty, args));
        }

        public static void PrintErr(params object[] args)
        {
            Debug.LogError(string.Join(string.Empty, args));
        }
    }
}
