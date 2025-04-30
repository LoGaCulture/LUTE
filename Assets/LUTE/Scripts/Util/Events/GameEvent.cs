using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// GameEvent is a ScriptableObject that can be used to create events in Unity.
    /// This base class uses game event listener but can be extended to use other types of listeners.
    /// </summary>
    [CreateAssetMenu(fileName = "GameEvent", menuName = "LUTE/Scriptable Objects/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        private List<GameEventListener> eventListeners = new List<GameEventListener>();

        public virtual void Raise()
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised();
            }
        }

        public virtual void RegisterListener(GameEventListener listener)
        {
            if (!eventListeners.Contains(listener))
            {
                eventListeners.Add(listener);
            }
        }

        public virtual void UnregisterListener(GameEventListener listener)
        {
            if (eventListeners.Contains(listener))
            {
                eventListeners.Remove(listener);
            }
        }
    }
}
