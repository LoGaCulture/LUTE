using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Logic",
        "For Each Loop",
        "Loop over each element in a given collection, similar to a foreach but internally uses indicies.")]
    public class ForEach : Condition, ICollectionCompatible
    {
        [Tooltip("The collection to loop over.")]
        [SerializeField] protected CollectionData collection;

        [SerializeField]
        [VariableProperty(compatibleVariableName = "collection")]
        protected Variable item;

        [Tooltip("Optional variable to store the current index in.")]
        [SerializeField]
        protected IntegerData currentIndex;

        public override bool StatementLooping { get { return true; } }

        protected override void PreEvaluate()
        {
            // If we come from the end then are already are looping, otherwise this is first loop so prepare
            if (ParentNode.PrevActiveOrderIndex != end.OrderIndex)
            {
                currentIndex.Value = -1;
            }
        }

        public override bool EvaluateConditions()
        {
            var col = collection.Value;
            currentIndex.Value++;
            if (currentIndex < col.Count)
            {
                col.Get(currentIndex, ref item);
                return true;
            }

            return false;
        }

        protected override void OnFalse()
        {
            GoToEnd();
        }

        protected override bool HasRequiredProperties()
        {
            return collection.Value != null && item != null;
        }

        public override bool HasReference(Variable variable)
        {
            return collection.collectionRef == variable || item == variable || base.HasReference(variable);
        }

        bool ICollectionCompatible.IsVarCompatibleWithCollection(Variable v, string compatWith)
        {
            if (compatWith == "collection")
            {
                return collection.Value == null ? false : collection.Value.IsElementCompatible(v);
            }
            else
                return true;
        }

        public override string GetSummary()
        {
            if (item == null)
                return "Error: No item variable selected.";
            if (collection.Value == null)
                return "Error: No collection selected.";

            return item.Key + " in " + collection.Value.name;
        }
    }
}
