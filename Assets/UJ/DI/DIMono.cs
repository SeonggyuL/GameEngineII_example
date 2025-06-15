using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace UJ.DI
{
    public class DIMono : MonoBehaviour
    {

        protected InjectObj InjectObj = new InjectObj();

        public bool IsInitialized => InjectObj.IsInjected;


        private void Start()
        {
            CheckInjectAndInit();
        }

        public void CheckInjectAndInit()
        {
            if (InjectObj.IsInjected)
            {
                return;
            }
            InjectObj.CheckInject(this);

            Init();
        }

        

  

        public virtual void Init()
        {
        }

       

    }
}