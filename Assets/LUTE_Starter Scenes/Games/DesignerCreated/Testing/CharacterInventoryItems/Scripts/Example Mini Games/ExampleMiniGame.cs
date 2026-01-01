using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// An example of how one can interact with objects using the inventory and custom item system.
    /// In a lot of cases, one would have a method that sets this object to active or inactive and thus begins the logic for this object.
    /// </summary>
    public class ExampleMiniGame : MonoBehaviour
    {
        public virtual void StartMiniGame()
        {
            Debug.Log($"Starting Mini Game on {gameObject.name}");
        }
    }
}
