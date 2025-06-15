using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UJ.DI
{

    public class InjectObj
    {


        bool isInjected = false;

        public bool IsInjected => isInjected;

        public void CheckInject<T>(T obj, string sceneContextName = "SceneContext")
        {

            if (isInjected == false)
            {
                ForceInject(obj, sceneContextName);
            }
        }

        public void ForceInject<T>(T obj, string sceneContextName = "SceneContext")
        {
            DIContainer.Inject(obj);
            isInjected = true;
        }
    }
}