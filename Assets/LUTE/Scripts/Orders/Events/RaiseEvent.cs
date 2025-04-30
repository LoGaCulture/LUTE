using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Events",
                  "Raise Event",
                  "Raises the given event and informs invokes all listeners,")]
    [AddComponentMenu("")]
    public class RaiseEvent : Order
    {
        [SerializeField]
        private GameEvent gameEvent;

        public override void OnEnter()
        {
            if (gameEvent != null)
            {
                gameEvent.Raise();
            }
            Continue();
        }

        public override string GetSummary()
        {
            return "Raises the given event and informs invokes all listeners,";
        }

        public override Color GetButtonColour()
        {
            return new Color(0.5f, 0.5f, 1f);
        }
    }
}
