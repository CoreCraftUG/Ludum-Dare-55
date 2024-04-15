using UnityEngine.Events;
using UnityEngine;

namespace CoreCraft.Core
{ 
    public class EventManager : Singleton<EventManager>
    {
        public UnityEvent GameOverEvent = new UnityEvent();
        public UnityEvent<int, float> PlayAudio = new UnityEvent<int, float>();
        public UnityEvent<int> LevelUpEvent = new UnityEvent<int>();
        public UnityEvent OnGameOptionsUIInitialized = new UnityEvent();
        /// <summary>
        /// Vector3 = displacement value;
        /// float = move time;
        /// int = displacement grid cells
        /// </summary>
        public UnityEvent<Vector3, float, int> GridMoveUp = new UnityEvent<Vector3,float, int>();
        public UnityEvent Summon = new UnityEvent();
        public UnityEvent SummonComplete = new UnityEvent();
        public UnityEvent DiggingEvent = new UnityEvent();
    }
}