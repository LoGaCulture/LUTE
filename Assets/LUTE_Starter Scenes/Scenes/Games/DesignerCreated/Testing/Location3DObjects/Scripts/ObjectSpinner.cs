using UnityEngine;
using UnityEngine.EventSystems;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Allows the user to spin an object around its local X and Y axes by dragging it using idraghandler interfaces.
    /// </summary>
    public class ObjectSpinner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        [SerializeField] protected float rotationSpeed = 5f;
        [SerializeField] protected float frictionCoefficient = 0.95f; // Controls how quickly the spin slows down

        [SerializeField] protected bool rotateX = true;
        [SerializeField] protected bool rotateY = false;
        [SerializeField] protected bool allowSpin = false;

        private Vector3 angularVelocity;

        protected virtual void Update()
        {
            if (!allowSpin)
                return;
            if (angularVelocity.magnitude > 0.01f)
            {
                // Apply momentum-based rotation
                if (rotateY)
                    transform.Rotate(Vector3.right, angularVelocity.x * Time.deltaTime, Space.World);
                if (rotateX)
                    transform.Rotate(Vector3.up, angularVelocity.y * Time.deltaTime, Space.World);
                // Apply friction
                angularVelocity *= frictionCoefficient;
            }
            else
            {
                angularVelocity = Vector3.zero;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            angularVelocity = Vector3.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float rotX = 0;
            float rotY = 0;
            if (rotateY)
            {
                rotX = -eventData.delta.y * rotationSpeed * Time.deltaTime;
                transform.Rotate(Vector3.right, rotX, Space.World);
            }

            if (rotateX)
            {
                rotY = eventData.delta.x * rotationSpeed * Time.deltaTime;
                transform.Rotate(Vector3.up, rotY, Space.World);
            }


            // Calculate angular velocity
            //angularVelocity = new Vector3(rotY, 0) / Time.deltaTime;
            angularVelocity = new Vector3(rotX, rotY, 0) / Time.deltaTime;

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // The angular velocity is already set, so we don't need to do anything here
        }
    }
}