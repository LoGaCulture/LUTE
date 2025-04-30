using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// This class is used to create events that can utilise the IGameEventListener interface.
    /// </summary>
    [CreateAssetMenu(fileName = "InterfaceGameEvent", menuName = "LUTE/Scriptable Objects/InterfaceGameEvent")]
    public class InterfaceGameEvent : GameEvent
    {
        private List<IGameEventListener> eventListeners = new List<IGameEventListener>();

        public override void Raise()
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(IGameEventListener listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void UnregisterListener(IGameEventListener listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }

    }
}