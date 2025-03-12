using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Logic",
        "Generic Node Button",
        "Creates a button which will execute a Node.")]
    public class GenericNodeButton : GenericButton
    {
        [Tooltip("The Node to execute")]
        [HideInInspector]
        [SerializeField] protected Node node;

        public override void OnEnter()
        {
            if (node == null)
            {
                Debug.LogError("No node set in the GenericNodeButton order");
                Continue();
                return;
            }

            var popupIcon = SetupButton();

            UnityEngine.Events.UnityAction action = () =>
            {
                node.StartExecution();
            };

            SetAction(popupIcon, action);

            Continue();
        }

        public override string GetSummary()
        {
            return node != null ? $"    Creates a button which will execute the Node: {node._NodeName}." : "    Error: No node set";
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 255, 184, 255);
        }

        public override void GetConnectedNodes(ref List<Node> connectedNodes)
        {
            if (node != null)
            {
                connectedNodes.Add(node);
            }
        }
    }
}