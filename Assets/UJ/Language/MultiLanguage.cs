
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;



namespace UJ.Language
{

    [Serializable]
    public class EtcStr
    {
        public string code;
        public string kor, eng, jp, zht,zhs;
        public override string ToString()
        {
            switch (Str.CurrentLanguage)
            {
                case Str.Language.Kor:
                    return kor;
                case Str.Language.Eng:
                    return eng;
                case Str.Language.Jap:
                    return jp;
                case Str.Language.Zht:
                    return zht;
                case Str.Language.Zhs:
                    return zhs;
            }

            return kor;
        }

        static List<EtcStr> _Strs;

        public static void SetEtcStrs(List<EtcStr> strs)
        {
            _Strs = strs;
        }

        public static List<EtcStr> Strs => _Strs;

        public static string FindStr(string code, params object[] parameters)
        {
            try
            {
                if (_Strs == null)
                {
                    return $"[{code}]";
                }

                var str = _Strs.Find(l => l.code == code);

                if (str != null)
                {

                    return string.Format(str.ToString(), parameters);

                }

                return string.Format("[{0}]", code);
            }
            catch (Exception e)
            {

                var str = _Strs.Find(l => l.code == code);

                if (str != null)
                {

                    return str.ToString();

                }


                UJ.Log.PrintErr($"FindStr [{code}]  Error: {e.Message}");
                return $"[{code}]";
            }
        }
    }



    public class LanguageChangeEvent
    {

    }


    [Serializable]
    public class Str :IFromString
    {
        [ReadOnly]
        public int code;
    
        public enum Language
        {
            Kor,Eng,Jap,Zhs,Zht
        }
        public static Language CurrentLanguage;


        [ReadOnly]
        public string kor,eng;

        [ReadOnly]
        public string jp;

        [ReadOnly]
        public string zhs, zht;

        public override string ToString()
        {

            switch (CurrentLanguage)
            {
                case Language.Eng:
                    return eng;
                case Language.Kor:
                    return kor;
                case Language.Jap:
                    return jp;
                case Language.Zhs:
                   return zhs;
                case Language.Zht:
                    return zht;

            }

            return "";

        }

        public void LoadFromString(string str)
        {
            kor = str;
        }

        //implict string casting operation
        public static implicit operator string(Str d) => d.ToString();

    }


    [System.AttributeUsage(System.AttributeTargets.Field |
           System.AttributeTargets.Property)]
    public class IgnoreFromXlsx : System.Attribute
    {

    }




}
