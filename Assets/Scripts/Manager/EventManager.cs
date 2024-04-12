using CoreCraft.Core;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager>
{
    public UnityEvent GameOverEvent = new UnityEvent();
    public UnityEvent<int, float> PlayAudio = new UnityEvent<int, float>();
    public UnityEvent<int> LevelUpEvent = new UnityEvent<int>();
    public UnityEvent OnGameOptionsUIInitialized = new UnityEvent();
}