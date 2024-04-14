using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreCraft.Core;
using MoreMountains.Feel;

namespace CoreCraft.LudumDare55
{
    public class SummonManager : Singleton<SummonManager>
    {
        private List<IInGrid> _snailList = new List<IInGrid>();

        public List<IInGrid> SnailList { get { return _snailList; } }

        public void RegisterSnail(IInGrid snail)
        {
            if (snail != null && _snailList.Contains(snail))
                _snailList.Add(snail);
        }

        public void UnregisterSnail(IInGrid snail)
        {
            if (snail != null && _snailList.Contains(snail))
                _snailList.Remove(snail);
        }
    }
}
