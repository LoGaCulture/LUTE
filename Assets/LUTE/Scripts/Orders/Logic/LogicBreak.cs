using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Logic",
        "Break",
        "Force a loop to terminate immediately.")]
    public class LogicBreak : Order
    {
        public override void OnEnter()
        {
            Condition loopingCond = null;

            // Find index of previous looping Order
            for (int i = OrderIndex - 1; i >= 0; i--)
            {
                Condition cond = ParentNode.OrderList[i] as Condition;
                if (cond != null && cond.StatementLooping)
                {
                    loopingCond = cond;
                    break;
                }
            }

            if (loopingCond == null)
            {
                Debug.LogError("Break called but no enclosing looping construct found." + GetLocationIdentifier());
                Continue();
            }
            else
            {
                loopingCond.GoToEnd();
            }
        }

        public override string GetSummary()
        {
            return "Force loop to terminate immediately.";
        }

        public override Color GetButtonColour()
        {
            return new Color32(253, 253, 150, 255);
        }
    }
}
