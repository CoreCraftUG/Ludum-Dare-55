using UnityEngine;

namespace CoreCraft.LudumDare55
{
    [CreateAssetMenu(fileName = "Grid Blocks", menuName = "Grid/Blocks", order = 1)]
    public class Block : ScriptableObject
    {
        [SerializeField] private BlockMaterial _material;
        [SerializeField] private BlockResources _resources;
        [SerializeField] private BlockingType _blockingType;
        [SerializeField] private bool _destructible;
        [SerializeField] private int _spawnMinDepth;
        [SerializeField] private int _spawnMaxDepth;
        [SerializeField] private GameObject _blockPrefab;

        public BlockMaterial Material { get { return _material; } }
        public BlockResources Resources { get { return _resources; } }
        public BlockingType Type { get { return _blockingType; } }
        public bool Destructible { get { return _destructible; } }
        public int SpawnMinDepth { get { return _spawnMinDepth; } }
        public int SpawnMaxDepth { get { return _spawnMaxDepth; } }
        public GameObject BlockPrefab { get { return _blockPrefab; } }
    }

    public enum BlockMaterial
    {
        None,
        Stone
    }

    public enum BlockResources
    {
        None,
        ManaCrystal
    }

    public enum BlockingType
    {
        None,
        Full
    }
}