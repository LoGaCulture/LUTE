using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Utilities;


[RequireComponent(typeof(ObjectSpawner))]
public class ObjectSpawnerWrapper : MonoBehaviour
{
    [Tooltip("Path to the 'parent' prefab in Resources that will wrap the newly spawned object.")]
    [SerializeField] private string spawnableResourcePath = "Prefabs/Spawnable";

    private ObjectSpawner spawner;

    public event Action<GameObject> wrapperSpawned;

    private void Awake()
    {
        // Grab the spawner on the same GameObject
        spawner = GetComponent<ObjectSpawner>();
        if (spawner == null)
        {
            Debug.LogError("ObjectSpawnerWrapper requires an ObjectSpawner on the same GameObject.", this);
            enabled = false;
            return;
        }

        // Subscribe to the spawner's event
        spawner.objectSpawned += OnObjectSpawned;
    }

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.objectSpawned -= OnObjectSpawned;
    }

    /// <summary>
    /// Called after the spawner instantiates its object. We do our "extra" logic here,
    /// like instantiating a wrapper prefab and re-parenting the newly spawned object.
    /// </summary>
    private void OnObjectSpawned(GameObject spawnedObject)
    {
        // Load the "Spawnable" prefab (or whichever path you set in the Inspector).
        var parentPrefab = Resources.Load<GameObject>(spawnableResourcePath);
        if (parentPrefab == null)
        {
            Debug.LogWarning($"ObjectSpawnerWrapper: No prefab found at '{spawnableResourcePath}'. Check your path or Resources folder.");
            return;
        }

      
        var wrapper = Instantiate(parentPrefab, spawnedObject.transform.position, spawnedObject.transform.rotation);

        spawnedObject.transform.SetParent(wrapper.transform, true);

      
        var grabInteractable = wrapper.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = wrapper.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        
        var meshCollider = spawnedObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = spawnedObject.AddComponent<MeshCollider>();


        grabInteractable.colliders.Add(meshCollider);

        
        var arTransformer = wrapper.GetComponent<UnityEngine.XR.Interaction.Toolkit.Transformers.ARTransformer>();
        if (arTransformer == null)
            arTransformer = wrapper.AddComponent<UnityEngine.XR.Interaction.Toolkit.Transformers.ARTransformer>();

       
       // arTransformer.minScale = spawnedObject.transform.localScale.x;

       
        wrapperSpawned?.Invoke(wrapper);

        Debug.Log("ObjectSpawnerWrapper: Successfully decorated spawned object!");
    }
}
