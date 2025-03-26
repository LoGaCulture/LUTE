using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CreateAssetMenu(menuName = "Mapbox/Modifiers/Prefab Path Modifier")]
    public class PrefabPathModifier : MeshModifier
    {
        [Header("Prefab Configuration")]
        public GameObject PrefabToSpawn;       // Prefab to instantiate along the path
        public float SpawnInterval = 0.5f;     // Distance between prefab instances
        public float VerticalOffset = 0.05f;   // Vertical lift above the map
        public bool RandomRotation = false;    // Randomize rotation of spawned prefabs
        public Vector3 FixedRotation = Vector3.zero; // Optional fixed rotation

        [Header("Spawn Options")]
        public bool AlignToPath = true;        // Rotate prefabs to follow path direction

        public override ModifierType Type { get { return ModifierType.Preprocess; } }

        public override void Run(VectorFeatureUnity feature, MeshData md, UnityTile tile = null)
        {
            // Check if a prefab is assigned
            if (PrefabToSpawn == null) return;

            // Create a parent object to organize spawned prefabs
            GameObject pathParent = new GameObject("PrefabPath_" + feature.GetHashCode());

            foreach (var roadSegment in feature.Points)
            {
                if (roadSegment.Count <= 1) continue;

                List<Vector3> spawnPoints = GenerateSpawnPoints(roadSegment);

                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    Vector3 spawnPosition = spawnPoints[i];

                    // Add vertical offset
                    spawnPosition.y += VerticalOffset;

                    // Instantiate prefab
                    GameObject spawnedPrefab = Instantiate(PrefabToSpawn, spawnPosition, Quaternion.identity);

                    // Set parent for organization
                    spawnedPrefab.transform.SetParent(pathParent.transform, true);

                    // Handle rotation
                    if (AlignToPath && spawnPoints.Count > 1)
                    {
                        // Calculate direction for path alignment
                        Vector3 direction = (i < spawnPoints.Count - 1)
                            ? (spawnPoints[i + 1] - spawnPoints[i]).normalized
                            : (spawnPoints[i] - spawnPoints[i - 1]).normalized;

                        // Create rotation based on path direction
                        Quaternion pathRotation = Quaternion.LookRotation(direction, Vector3.up);

                        // Apply fixed or random rotation
                        if (RandomRotation)
                        {
                            // Add random rotation around Y axis
                            pathRotation *= Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                        }
                        else if (FixedRotation != Vector3.zero)
                        {
                            // Apply additional fixed rotation
                            pathRotation *= Quaternion.Euler(FixedRotation);
                        }

                        spawnedPrefab.transform.rotation = pathRotation;
                    }
                }
            }
        }

        private List<Vector3> GenerateSpawnPoints(List<Vector3> originalPath)
        {
            List<Vector3> spawnPoints = new List<Vector3>();

            for (int i = 0; i < originalPath.Count - 1; i++)
            {
                Vector3 start = originalPath[i];
                Vector3 end = originalPath[i + 1];
                Vector3 direction = (end - start).normalized;
                float segmentLength = Vector3.Distance(start, end);

                float currentDistance = 0;
                while (currentDistance < segmentLength)
                {
                    Vector3 spawnPoint = start + direction * currentDistance;
                    spawnPoints.Add(spawnPoint);

                    // Move to next spawn point
                    currentDistance += SpawnInterval;
                }
            }

            return spawnPoints;
        }
    }
}
