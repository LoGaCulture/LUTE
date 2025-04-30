using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Executes the given Node when the event is raised.
    /// </summary>
    [EventHandlerInfo("Events",
                  "Event Raised",
                  "Executes when the given event has been raised.")]
    [AddComponentMenu("")]
    public class GameEventHandler : EventHandler, IGameEventListener
    {
        [SerializeField] private InterfaceGameEvent gameEvent;
        [Tooltip("If true, the event will deregister itself after executing. This ensures it only fires once.")]
        [SerializeField] private bool deregisterOnExecute = true;

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            Deregister();
        }

        public void Register()
        {
            if (gameEvent == null)
            {
                throw new MissingReferenceException($"{name} tried to register to a GameEvent, but none was assigned.");
            }

            gameEvent.RegisterListener(this);
        }

        public void Deregister()
        {
            if (gameEvent == null)
            {
                throw new MissingReferenceException($"{name} tried to deregister from a GameEvent, but none was assigned.");
            }

            gameEvent.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
            if (deregisterOnExecute)
            {
                Deregister();
            }
            ExecuteNode();
        }

        public override string GetSummary()
        {
            return "This node will execute when the given event has been raised.";
        }
    }
}