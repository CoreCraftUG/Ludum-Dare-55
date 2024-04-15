using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class BaseTriggerBox : MonoBehaviour
    {
        IPeripheryTrigger peripheryTrigger => GetComponentInParent<IPeripheryTrigger>();

        private void OnTriggerEnter(Collider other)
        {
            peripheryTrigger.TriggerEnter(other);    
        }
        private void OnTriggerExit(Collider other)
        {
            peripheryTrigger.TriggerExit(other);
        }
    }
}
