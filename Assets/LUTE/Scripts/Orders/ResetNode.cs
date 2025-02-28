using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Stops executing the named Node.
    /// </summary>
    [OrderInfo("Flow",
                 "Stop Node",
                 "Stops executing the named Node")]
    public class ResetNode : Order
    {
        [Tooltip("The engine containing the node - parent is used if none specified.")]
        [SerializeField] protected BasicFlowEngine engine;
        [Tooltip("The node to be reset")]
        [SerializeField] protected string nodeName;
        [Tooltip("Whether or not to reset the node's location")]
        [SerializeField] protected bool resetLocation;
        public override void OnEnter()
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                Continue();
            }
            if (engine == null)
            {
                engine = (BasicFlowEngine)GetEngine();
            }

            var node = engine.FindNode(nodeName);
            if (node == null)
                Continue();

            //if (resetLocation)
            //    node.NodeLocation = null;
            node.Stop();
            node.ShouldCancel = true;

            Continue();
        }

        public override string GetSummary()
        {
            return nodeName;
        }

        public override Color GetButtonColour()
        {
            return new Color32(253, 253, 150, 255);
        }
    }
}
