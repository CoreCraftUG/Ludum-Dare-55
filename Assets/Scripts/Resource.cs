using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class Resource : MonoBehaviour
    {
        [SerializeField] private BlockResources _resource;
        public Vector2Int PosCell;

        public BlockResources Resources { get { return _resource; } }
        
    }
}
