using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UJ.Attributes;

namespace UJ.DI
{


    public class DIContainer
    {

        static List<DIContainer> diContainers = new List<DIContainer>();



        public static void AddContainer(DIContainer container)
        {
            diContainers.Add(container);
        }

        public static void RemoveContainer(DIContainer container)
        {
            diContainers.Remove(container);
        }


        Dictionary<string, object> Container = new Dictionary<string, object>();

        public static string GetKey(Type type, string key = "")
        {
            return type + "_" + key;
        }


        public void Regist<T>(T o, string key = "")
        {
            Container.Add(GetKey(typeof(T), key), o);
        }

        public void InjectAndRegist<T>(T o, string key = "")
        {
            Inject(o);
            Container.Add(GetKey(typeof(T), key), o);

        }


        public T GetValue<T>(string key = "")
        {
            return (T)GetValue(typeof(T), key);
        }



        public static void Inject(object o)
        {
            foreach (var f in o.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
            {

                var injectAttr = Attribute.GetCustomAttribute(f, typeof(InjectAttribute)) as InjectAttribute;

                if (injectAttr != null)
                {
                    var t = f.FieldType;
                    try
                    {
                        object v = GetValueFromAll(t, injectAttr.key, injectAttr.canNull);
                        f.SetValue(o, v);
                    }
                    catch(Exception e)
                    {
                        UJ.Log.ColorLog(UnityEngine.Color.red, "DIContainer.Inject : " + o.GetType() + " "+  o + " " + f.Name + " " + e.Message);
                        throw e;
                    }
        
                }
            }
        }

        public static void OverwriteValue<T>(T o, string key = "")
        {
            var diKey = GetKey(typeof(T), key);
            foreach (var c in diContainers)
            {
                if (c.Container.ContainsKey(diKey))
                {
                    c.Container[diKey] = o;
                    return;
                }
            }

            throw new Exception("DIContainer.OverwriteValue : " + typeof(T) + " is not found");
        }


        public static T GetValueFromAllT<T>(string key = "", bool canNull = false)
        {
            return (T)GetValueFromAll(typeof(T), key);
        }

        public static object GetValueFromAll(Type t, string key = "",bool canNull=false)
        {
            object v = null;

            int idx = diContainers.Count - 1;
            for (; idx >= 0; idx--)
            {
                var c = diContainers[idx];
                var diKey = GetKey(t, key);
                if (c.Container.ContainsKey(diKey))
                {
                    v = c.Container[diKey];
                    break;
                }
            }



            if (v == null && canNull==false)
            {
                throw new Exception("DIContainer.GetValueFromAll : " + t + " is not found");
            }

            return v;
        }



        public object GetValue(Type t, string key = "")
        {
            object v = null;
            var diKey = GetKey(t, key);
            if (Container.ContainsKey(diKey))
            {
                v = Container[diKey];
            }

            return v;
        }
    }
}