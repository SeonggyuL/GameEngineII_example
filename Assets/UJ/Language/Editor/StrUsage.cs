
using ClosedXML.Excel;
using ExcelDataReader;
using ExcelDataReader.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UJ.Language;

namespace UJ.Language.Editor
{

    public class StrUsage
    {
        public int code;
        public string hostName;
        public string fieldName;

        public string kr, en, jp, zht, zhs;

        public void Reset()
        {
            isNew = false;
            updated = false;
            deleted = false;

        }

        [NonSerialized]
        public bool isNew;

        [NonSerialized]
        public bool updated;

        [NonSerialized]
        public bool deleted;

        public StrUsageKey CreateKey()
        {
            return new StrUsageKey
            {
                code = this.code,
                hostName = this.hostName,
                fieldName = this.fieldName
            };
        }

        internal void ResetLanguesExcept(Str.Language kor)
        {
            if (kor != Str.Language.Kor)
            {
                kr = "";
            }

            if (kor != Str.Language.Eng)
            {
                en = "";
            }

            if (kor != Str.Language.Jap)
            {
                jp = "";
            }
            if (kor != Str.Language.Zht)
            {
                zht = "";
            }
            if (kor != Str.Language.Zhs)
            {
                zhs = "";
            }
        }

        public Str ToStr()
        {
            return new Str
            {
                code = this.code,
                kor = this.kr,
                eng = this.en,
                jp = this.jp,
                zht = this.zht,
                zhs = this.zhs
            };
        }
    }

    public struct StrUsageKey
    {
        public int code;
        public string hostName;
        public string fieldName;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + code.GetHashCode();
                hash = hash * 23 + hostName?.GetHashCode() ?? 0;
                hash = hash * 23 + fieldName?.GetHashCode() ?? 0;
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is StrUsageKey other)
            {
                return code == other.code && 
                       hostName == other.hostName && 
                       fieldName == other.fieldName;
            }
            return false;
        }
    }

    public class DataReaderException : Exception
    {
        public DataReaderException(string msg) : base(msg)
        {

        }
    }



    public static class TypeHelper
    {
        public static bool TryListOfWhat(Type type, out Type innerType)
        {
            //Contract.Requires(type != null);

            var interfaceTest = new Func<Type, Type>(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>) ? i.GetGenericArguments().Single() : null);

            innerType = interfaceTest(type);
            if (innerType != null)
            {
                return true;
            }

            foreach (var i in type.GetInterfaces())
            {
                innerType = interfaceTest(i);
                if (innerType != null)
                {
                    return true;
                }
            }

            return false;
        }
    }





}
