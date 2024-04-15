using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreCraft.Core;
using CharacterController = CoreCraft.Core.CharacterController;

namespace CoreCraft.LudumDare55
{
    public class SummonManager : Singleton<SummonManager>
    {
        private List<IInGrid> _snailList = new List<IInGrid>();
        private CharacterController _player;

        public List<IInGrid> SnailList { get { return _snailList; } }
        public CharacterController Player { get { return _player; } }

        public void RegisterSnail(IInGrid snail)
        {
            if (snail != null && !_snailList.Contains(snail))
                _snailList.Add(snail);
        }

        public void UnregisterSnail(IInGrid snail)
        {
            if (snail != null && _snailList.Contains(snail))
                _snailList.Remove(snail);
        }

        public void RegisterPlayer(CharacterController player)
        {
            if (player != null && _player != player)
                _player = player;
        }
    }
}