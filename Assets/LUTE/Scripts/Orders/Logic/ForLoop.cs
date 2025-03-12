using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Logic",
        "For Loop",
        "Loops over a sent range, just like a normal for loop would.")]
    public class ForLoop : Condition
    {
        [Tooltip("Starting value of the loop.")]
        [SerializeField] protected IntegerData startingValue;
        [Tooltip("Ending value of the loop.")]
        [SerializeField] protected IntegerData endingValue;
        [Tooltip("Optional integer variable to hold the current loop counter.")]
        [SerializeField] protected IntegerData loopCounter;
        [Tooltip("Step size for the counter, how much it should increment by each iteration. Default is 1.")]
        [SerializeField] protected IntegerData stepSize = new IntegerData(1);

        public override bool StatementLooping { get { return true; } }

        protected override void PreEvaluate()
        {
            // If we came from the end of the loop, we are already looping otherwise we are starting the loop
            if (ParentNode.PrevActiveOrderIndex != end.OrderIndex)
            {
                loopCounter.Value = startingValue.Value;
            }
            else
            {
                loopCounter.Value += (startingValue.Value <= endingValue.Value ? stepSize.Value : -stepSize.Value);
            }
        }

        public override bool EvaluateConditions()
        {
            // If the starting value is less than or equal to the ending value, we are incrementing the loop counter
            return (startingValue.Value <= endingValue.Value ?
                loopCounter.Value < endingValue.Value :
                loopCounter.Value > endingValue.Value);
        }

        protected override void OnFalse()
        {
            GoToEnd();
        }

        public override void OnValidate()
        {
            // No infinite loops allowed
            if (stepSize.Value == 0)
                stepSize.Value = 1;

            // No negative steps allowed
            stepSize.Value = Mathf.Abs(stepSize.Value);
        }

        public override bool HasReference(Variable variable)
        {
            return startingValue.integerRef == variable || endingValue.integerRef == variable || loopCounter.integerRef == variable || stepSize.integerRef == variable || base.HasReference(variable);
        }
    }
}
