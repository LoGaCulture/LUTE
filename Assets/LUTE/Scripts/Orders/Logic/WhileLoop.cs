using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Logic",
        "While",
        "Continuously loop through a list of Orders while the condition is true. Use the 'Break' command to force the loop to terminate immediately.")]
    [AddComponentMenu("")]
    public class WhileLoop : If
    {
        public override bool StatementLooping { get { return true; } }
    }
}
