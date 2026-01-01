using UnityEngine;

public class OverlapDetector : MonoBehaviour
{
    // The minimum overlap percentage required to consider the puzzle solved
    public float minimumOverlapPercentage = 0.75f;

    //event for callback when puzzle is solved
    public delegate void PuzzleSolvedEventHandler();
    public event PuzzleSolvedEventHandler PuzzleSolved;

    // This function is called every frame while another collider is within the trigger collider
    private void OnTriggerStay(Collider other)
    {

        //Debug.Log("Overlap Detected " + other.name);

        // Check if the other collider is the puzzle piece
        if (other.CompareTag("DragPiece"))
        {


            // Calculate the overlap area as a percentage of the puzzle piece's total area
            float overlapPercentage = CalculateOverlapPercentage(this.GetComponentInChildren<Collider>(), other);

            //times by 100
            overlapPercentage *= 100;


            //Debug.Log("Overlap Detected with percentage: " + overlapPercentage);

            // If the overlap is sufficient, consider the puzzle solved
            if (overlapPercentage >= minimumOverlapPercentage)
            {
                Debug.Log("Puzzle Solved!");
                PuzzleSolved?.Invoke();

                //destroy the puzzle piece and the transparent object
                //Destroy(other.gameObject);
                //Destroy(this.gameObject);
            }
        }
    }

    // Function to calculate the overlap percentage
    float CalculateOverlapPercentage(Collider targetArea, Collider puzzlePiece)
    {
        // Calculate the bounds of both colliders
        Bounds targetBounds = targetArea.bounds;
        Bounds pieceBounds = puzzlePiece.bounds;

        // Calculate the intersection volume
        float intersectionVolume = BoundsIntersectionVolume(targetBounds, pieceBounds);

        // Calculate the volume of the puzzle piece
        float pieceVolume = pieceBounds.size.x * pieceBounds.size.y * pieceBounds.size.z;

        // Return the overlap percentage
        return intersectionVolume / pieceVolume;
    }

    // Function to calculate the intersection volume of two bounds
    float BoundsIntersectionVolume(Bounds a, Bounds b)
    {
        // Find the min and max points of intersection
        float minX = Mathf.Max(a.min.x, b.min.x);
        float minY = Mathf.Max(a.min.y, b.min.y);
        float minZ = Mathf.Max(a.min.z, b.min.z);
        float maxX = Mathf.Min(a.max.x, b.max.x);
        float maxY = Mathf.Min(a.max.y, b.max.y);
        float maxZ = Mathf.Min(a.max.z, b.max.z);

        // Calculate the dimensions of the intersection area
        float width = Mathf.Max(0, maxX - minX);
        float height = Mathf.Max(0, maxY - minY);
        float depth = Mathf.Max(0, maxZ - minZ);

        // Return the volume of the intersection area
        return width * height * depth;
    }
}