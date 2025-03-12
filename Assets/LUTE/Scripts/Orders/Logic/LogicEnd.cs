using UnityEngine;

/// <summary>
/// Marks the end of a conditional block.
/// </summary>
[OrderInfo("Logic",
             "End",
             "Marks the end of a condition")]
[AddComponentMenu("")]
public class LogicEnd : Order
{
    public virtual bool Loop { get; set; }

    public virtual int LoopBackIndex { get; set; }

    public override void OnEnter()
    {
        if (Loop)
        {
            Continue(LoopBackIndex);
            return;
        }
        Continue();
    }

    public override bool CloseNode()
    {
        return true;
    }

    public override Color GetButtonColour()
    {
        return new Color32(253, 150, 150, 255);
    }
}
