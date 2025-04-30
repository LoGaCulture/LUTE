using UnityEngine;
using UnityEngine.Events;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// This class is used to listen to GameEvents and execute a response when the event is raised.
    /// The response is a UnityEvent that can be set in the inspector.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        public GameEvent gameEvent;
        public UnityEvent response;

        private void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }

        private void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }

        public virtual void OnEventRaised()
        {
            if (response != null)
            {
                response.Invoke();
            }
        }
    }
}
