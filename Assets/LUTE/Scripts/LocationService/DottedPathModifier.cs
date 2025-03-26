using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CreateAssetMenu(menuName = "Mapbox/Modifiers/Dotted Path Modifier")]
    public class DottedPathModifier : MeshModifier
    {
        [Header("Dot Configuration")]
        public float DotRadius = 0.1f;         // Radius of each dot
        public float DotSpacing = 0.5f;        // Space between dots
        public float VerticalOffset = 0.05f;   // Vertical lift above the map

        public override ModifierType Type { get { return ModifierType.Preprocess; } }

        public override void Run(VectorFeatureUnity feature, MeshData md, UnityTile tile = null)
        {
            if (feature.Points.Count < 1) return;

            foreach (var roadSegment in feature.Points)
            {
                if (roadSegment.Count <= 1) continue;

                List<Vector3> dottedPathPoints = GenerateDottedPath(roadSegment);

                for (int i = 0; i < dottedPathPoints.Count; i++)
                {
                    Vector3 dotPosition = dottedPathPoints[i];

                    // Add vertical offset
                    dotPosition.y += VerticalOffset;

                    // Create a circle (disk) facing upwards
                    int vertexCount = 12; // Number of vertices for the circle
                    Vector3 center = dotPosition;

                    // Center vertex
                    int centerVertexIndex = md.Vertices.Count;
                    md.Vertices.Add(center);
                    md.UV[0].Add(new Vector2(0.5f, 0.5f));

                    // Create circle vertices (flipped to face up)
                    for (int j = 0; j <= vertexCount; j++)
                    {
                        float angle = j * (2f * Mathf.PI / vertexCount);
                        Vector3 offset = new Vector3(
                            Mathf.Cos(angle) * DotRadius,
                            0,
                            Mathf.Sin(angle) * DotRadius
                        );

                        md.Vertices.Add(center + new Vector3(offset.x, 0, offset.z));
                        md.UV[0].Add(new Vector2(
                            0.5f + offset.x / (2f * DotRadius),
                            0.5f + offset.z / (2f * DotRadius)
                        ));
                    }

                    // Create triangles for the circle (reversed order to face up)
                    if (md.Triangles.Count == 0)
                        md.Triangles.Add(new List<int>());

                    for (int j = 0; j < vertexCount; j++)
                    {
                        md.Triangles[0].Add(centerVertexIndex);
                        md.Triangles[0].Add(centerVertexIndex + j + 2);
                        md.Triangles[0].Add(centerVertexIndex + j + 1);
                    }
                }
            }
        }

        private List<Vector3> GenerateDottedPath(List<Vector3> originalPath)
        {
            List<Vector3> dottedPathPoints = new List<Vector3>();

            for (int i = 0; i < originalPath.Count - 1; i++)
            {
                Vector3 start = originalPath[i];
                Vector3 end = originalPath[i + 1];
                Vector3 direction = (end - start).normalized;
                float segmentLength = Vector3.Distance(start, end);

                float currentDistance = 0;
                while (currentDistance < segmentLength)
                {
                    Vector3 dotPosition = start + direction * currentDistance;
                    dottedPathPoints.Add(dotPosition);

                    // Move to next dot
                    currentDistance += DotSpacing;
                }
            }

            return dottedPathPoints;
        }
    }
}
