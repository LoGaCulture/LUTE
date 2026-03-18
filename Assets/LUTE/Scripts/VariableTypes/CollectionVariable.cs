using UnityEngine;

[VariableInfo("Other", "Collection")]
[AddComponentMenu("")]
[System.Serializable]
public class CollectionVariable : BaseVariable<Collection>
{
}

/// <summary>
/// Container for a Collection variable reference or constant value.
/// </summary>
[System.Serializable]
public struct CollectionData
{
    [SerializeField]
    [VariableProperty("<Value>", typeof(CollectionVariable))]
    public CollectionVariable collectionRef;

    [SerializeField]
    public Collection collectionVal;
    public CollectionData(Collection v)
    {
        collectionVal = v;
        collectionRef = null;
    }

    public static implicit operator Collection(CollectionData CollectionData)
    {
        return CollectionData.Value;
    }

    public Collection Value
    {
        get { return (collectionRef == null) ? collectionVal : collectionRef.Value; }
        set { if (collectionRef == null) { collectionVal = value; } else { collectionRef.Value = value; } }
    }

    public string GetDescription()
    {
        if (collectionRef == null)
        {
            return collectionVal != null ? collectionVal.ToString() : "Null";
        }
        else
        {
            return collectionRef.Key;
        }
    }
}
