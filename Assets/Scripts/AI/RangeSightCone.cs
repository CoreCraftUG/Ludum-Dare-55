using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class RangeSightCone : BaseSightCone
    {
        [ShowInInspector, ReadOnly] protected IPeripheryTrigger peripheryTrigger => GetComponentInParent<IPeripheryTrigger>();

    protected override void OnTriggerExit(Collider other)
    {
            peripheryTrigger.TriggerExit(other);
    }
}
}
