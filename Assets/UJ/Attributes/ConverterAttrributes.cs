using System;
using System.ComponentModel;
using UnityEngine;

namespace UJ.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class FromXlsxAttribute : Attribute
    {
    }

    public class InjectAttribute : Attribute
    {
        public readonly string key;
        public bool canNull = false;
        public InjectAttribute(string key = "", bool canNull = false)
        {
            this.key = key;
            this.canNull = canNull;
        }
    }
}