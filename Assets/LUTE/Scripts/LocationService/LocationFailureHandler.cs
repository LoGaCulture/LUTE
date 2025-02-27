using Mapbox.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// A class that handles the failure of the location service (i.e. the user cannot accses a location)
    /// Provides a series of methods that can be called to handle inaccessibility of the location
    /// When player reaches a location that cannot be accessed, the location searches for this object and matching location
    /// It then acts according to the priority of the methods defined in this class (this list can be modified by designers in the inspector)
    /// </summary>
    [RequireComponent(typeof(BasicFlowEngine))]
    public class LocationFailureHandler : MonoBehaviour
    {
        [SerializeField] protected List<FailureMethod> failureMethods = new List<FailureMethod>();

        public List<FailureMethod> FailureMethods { get => failureMethods; set => failureMethods = value; }

        public enum FailureHandlingOutcome
        {
            Continue, // Continue to the next method in the list
            Stop, // Stop the execution of the methods, consider the failure as handled
            Abort // Abort the execution of the methods, consider the failure unhandled
        }

        private static Dictionary<string, MethodInfo> availableMethods = new Dictionary<string, MethodInfo>();

        private void Awake()
        {
            RegisterFailureHandlingMethods();
        }

        private void OnEnable()
        {
            LocationServiceSignals.OnLocationComplete += OnLocationSuccess;
        }

        private void OnDisable()
        {
            LocationServiceSignals.OnLocationComplete -= OnLocationSuccess;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoadRuntimeMethod()
        {
            RegisterFailureHandlingMethods();
        }

        private static void RegisterFailureHandlingMethods()
        {
            if (availableMethods.Count > 0)
            {
                return;
            }

            var methods = typeof(LocationFailureHandler).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var method in methods)
            {
                var attribute = Attribute.GetCustomAttribute(method, typeof(FailureHandlingMethodAttribute)) as FailureHandlingMethodAttribute;
                if (attribute != null)
                {
                    availableMethods[method.Name] = method;
                }
            }
        }

        protected virtual void OnLocationSuccess(LocationVariable location)
        {
            // Find the relevant failure method via location and set it to handled
            var failureMethod = failureMethods.Find(fm =>
                           fm.QueriedLocation != null && Equals(fm.QueriedLocation.Value, location.Value));

            if (failureMethod != null)
            {
                failureMethod.IsHandled = true;
            }
        }

        public static string[] GetAvailableMethods()
        {
            RegisterFailureHandlingMethods();
            return new List<string>(availableMethods.Keys).ToArray();
        }

        public void SetupLocations()
        {
            var engine = BasicFlowEngine.CachedEngines.FirstOrDefault();
            if (engine == null)
                engine = FindObjectOfType<BasicFlowEngine>();
            if (engine != null)
            {
                var nodes = engine.GetComponents<Node>();
                if (nodes != null && nodes.Count() > 0)
                {
                    foreach (var node in nodes)
                    {
                        if (node != null && node.NodeLocation != null)
                        {
                            // Found a node location so we can set this up
                            AddNodeLocation(node.NodeLocation);
                        }
                        else
                        {
                            // Node is not null but has no location - check to see if any orders on this node use a location
                            var orders = node.OrderList;
                            foreach (var order in orders)
                            {
                                if (order.GetOrderLocation() != null)
                                {
                                    var location = order.GetOrderLocation();
                                    AddOrderLocation(location, order);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddNodeLocation(LocationVariable location)
        {
            // Check if the location already exists in our failure handler list
            bool locationExists = failureMethods.Any(fm =>
                fm.QueriedLocation != null &&
                Equals(fm.QueriedLocation.Value, location.Value));

            if (!locationExists)
            {
                var newFailureMethod = new FailureMethod(location);
                // Available methods applicable
                newFailureMethod.PriorityMethods.Add("Execute_Anyway");
                newFailureMethod.PriorityMethods.Add("Increase_Radius");
                newFailureMethod.PriorityMethods.Add("Use_Nearest_Location");
                newFailureMethod.PriorityMethods.Add("Disbale_Location_Behaviour");
                failureMethods.Add(newFailureMethod);
            }
        }

        private void AddOrderLocation(LocationVariable location, Order order)
        {
            // Check if the location already exists in our failure handler list
            bool locationExists = failureMethods.Any(fm =>
                fm.QueriedLocation != null &&
                Equals(fm.QueriedLocation.Value, location.Value));
            // If not then add a location failure handler for it and base the method and setup on the type of node provided
            if (!locationExists && order != null)
            {
                var newFailureMethod = new FailureMethod(location);
                // Available methods applicable are based on order type - add more here
                switch (order)
                {
                    //case If ifT:
                    //    newFailureMethod.PriorityMethods.Add("Execute_Anyway");
                    //    newFailureMethod.PriorityMethods.Add("Increase_Radius");
                    //    newFailureMethod.PriorityMethods.Add("Use_Nearest_Location");
                    //    newFailureMethod.PriorityMethods.Add("Disbale_Location_Behaviour");
                    //    break;
                    //case LocationPickups locationPickupsT:
                    //    newFailureMethod.PriorityMethods.Add("Execute_Anyway");
                    //    newFailureMethod.PriorityMethods.Add("Increase_Radius");
                    //    newFailureMethod.PriorityMethods.Add("Use_Nearest_Location");
                    //    newFailureMethod.PriorityMethods.Add("Disbale_Location_Behaviour");
                    //    break;
                    default:
                        newFailureMethod.PriorityMethods.Add("Execute_Anyway");
                        newFailureMethod.PriorityMethods.Add("Increase_Radius");
                        newFailureMethod.PriorityMethods.Add("Use_Nearest_Location");
                        newFailureMethod.PriorityMethods.Add("Disbale_Location_Behaviour");
                        break;
                }
                failureMethods.Add(newFailureMethod);
            }
        }

        public bool HandleFailure(Vector2d location)
        {
            if (location == null)
            {
                // There are no locations to handle
                return false;
            }
            foreach (FailureMethod method in failureMethods)
            {
                var location2d = method.QueriedLocation.Value.Position;
                if (Vector2d.Equals(location2d, location))
                {
                    foreach (string methodName in method.PriorityMethods)
                    {
                        if (availableMethods.TryGetValue(methodName, out var methodInfo))
                        {
                            FailureHandlingOutcome outcome = InvokeMethod(methodInfo, method);
                            switch (outcome)
                            {
                                case FailureHandlingOutcome.Stop:
                                    return true;
                                case FailureHandlingOutcome.Abort:
                                    return false;
                                case FailureHandlingOutcome.Continue:
                                    continue;
                            }
                        }
                    }
                    return false; // If we've gone through all methods without a Stop or Abort
                }
            }
            return false; // If no matching FailureMethod was found
        }

        private FailureHandlingOutcome InvokeMethod(MethodInfo methodInfo, FailureMethod failureMethod)
        {
            var parameters = methodInfo.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(FailureMethod))
                {
                    args[i] = failureMethod;
                }
                else if (parameters[i].ParameterType == typeof(LocationVariable))
                {
                    args[i] = failureMethod.QueriedLocation;
                }
                else if (parameters[i].ParameterType == typeof(List<LocationVariable>))
                {
                    args[i] = failureMethod.BackupLocations;
                }
                // Add more parameter types as needed
            }

            object result = methodInfo.Invoke(this, args);

            // Interpret the result
            if (result is FailureHandlingOutcome outcome)
            {
                return outcome;
            }
            else if (result is bool boolResult)
            {
                return boolResult ? FailureHandlingOutcome.Stop : FailureHandlingOutcome.Continue;
            }
            else if (methodInfo.ReturnType == typeof(void))
            {
                return FailureHandlingOutcome.Continue;
            }
            else
            {
                // For any other return type, consider it a success if non-null
                return result != null ? FailureHandlingOutcome.Stop : FailureHandlingOutcome.Continue;
            }
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome Use_Backup_Location(FailureMethod failureMethod)
        {
            // Check if there are any backup locations
            if (failureMethod.BackupLocations == null || failureMethod.BackupLocations.Count == 0)
            {
                return FailureHandlingOutcome.Continue;
            }

            // Iterate through all backup locations
            foreach (var backupLocation in failureMethod.BackupLocations)
            {
                // Check if the backup location is the same as the queried location
                if (backupLocation.Equals(failureMethod.QueriedLocation))
                {
                    Debug.Log($"Skipping backup location {backupLocation} as it's the same as the queried location.");
                    continue;
                }

                // Check if the backup location is accessible
                if (IsLocationAccessible(backupLocation))
                {
                    var engine = failureMethod.GetEngine();
                    if (engine != null)
                    {
                        var mapManager = engine.GetMapManager();
                        if (mapManager != null)
                        {
                            mapManager.HideLocationMarker(failureMethod.QueriedLocation);
                            bool updateText = failureMethod.UpdateLocationText;
                            mapManager.ShowLocationMarker(backupLocation);
                        }
                    }

                    failureMethod.QueriedLocation.Apply(SetOperator.Assign, backupLocation);
                    failureMethod.IsHandled = true;
                    string message = $"Location {failureMethod.QueriedLocation.Key} is inaccessible. The backup location {backupLocation.Key} has been selected instead.";
                    DisplayMessage(message);
                    return FailureHandlingOutcome.Stop;
                }
                else
                {
                    Debug.Log($"Backup location {backupLocation} is inaccessible. Trying next backup location.");
                }
            }

            // If we've gone through all backup locations and none were suitable
            return FailureHandlingOutcome.Continue;
        }

        private bool IsLocationAccessible(LocationVariable location)
        {
            // Implement your logic to check if a location is accessible using some server side or client side logic
            // This could involve checking GPS coordinates, network connectivity, or any other relevant factors
            // Return true if the location is accessible, false otherwise

            // For now, we'll just return true for demonstration purposes
            return true;
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome Increase_Radius(FailureMethod failureMethod)
        {
            if (failureMethod.HasIncreased && !failureMethod.AllowContinuousIncrease)
            {
                // We have already increased but we don't allow multiple increases
                return FailureHandlingOutcome.Continue;
            }
            // Increase the location radius check size
            failureMethod.QueriedLocation.Value.RadiusIncrease += failureMethod.RadiusIncreaseSize;
            failureMethod.HasIncreased = true;

            string message = $"Location {failureMethod.QueriedLocation.Key} is inaccessible. The radius has been increased by {failureMethod.RadiusIncreaseSize} meters.";
            DisplayMessage(message);

            return FailureHandlingOutcome.Stop;
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome Execute_Anyway(FailureMethod failureMethod)
        {
            // If location cannot be accessed then we create a menu of failed nodes for the player to execute
            failureMethod.QueriedLocation.Value.LocationDisabled = true;
            var engine = failureMethod.GetEngine();
            if (engine != null)
            {
                var mapManager = engine.GetMapManager();
                if (mapManager != null)
                {
                    mapManager.HideLocationMarker(failureMethod.QueriedLocation);
                }

                var nodes = engine.GetComponents<Node>();
                var affectedNodes = new List<Node>();

                foreach (var node in nodes)
                {
                    bool nodeAffected = false;

                    if (node.NodeLocation != null && Equals(node.NodeLocation.Value, failureMethod.QueriedLocation.Value))
                    {
                        nodeAffected = true;
                    }

                    if (!nodeAffected)
                    {
                        foreach (var order in node.OrderList)
                        {
                            if (order.GetOrderLocation() != null && Equals(order.GetOrderLocation().Value, failureMethod.QueriedLocation.Value))
                            {
                                nodeAffected = true;
                            }
                        }
                    }

                    if (nodeAffected)
                    {
                        affectedNodes.Add(node);
                    }
                }
                string failedNodes = string.Empty;
                foreach (var affectedNode in affectedNodes)
                {
                    affectedNode.NodeLocation = null;
                    affectedNode.Stop();
                    affectedNode.ShouldCancel = true;
                    LocationServiceSignals.DoLocationFailed(failureMethod, affectedNode);
                    failedNodes += affectedNode._NodeName + ", ";
                }

                // If we have affected nodes then we stop the failure method and the player can execute the node via menu
                if (affectedNodes.Any())
                {
                    failureMethod.IsHandled = true;
                    string message = $"Location {failureMethod.QueriedLocation.Key} is inaccessible. Please use the content menu to play the content for these given nodes: {failedNodes}";
                    DisplayMessage(message);
                    return FailureHandlingOutcome.Stop;
                }
            }

            return FailureHandlingOutcome.Continue;
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome Use_Nearest_Location(FailureMethod failureMethod)
        {
            var engine = failureMethod.GetEngine();
            if (engine != null)
            {
                var mapManager = engine.GetMapManager();
                if (mapManager != null)
                {
                    LocationVariable nearestLocation = null;
                    var allLocations = engine.GetComponents<LocationVariable>();
                    foreach (var location in allLocations)
                    {
                        if (location != null && !location.Equals(failureMethod.QueriedLocation))
                        {
                            if (nearestLocation == null)
                            {
                                nearestLocation = location;
                            }
                            else
                            {
                                var currentDistance = Vector2d.Distance(nearestLocation.Value.LatLongString(), failureMethod.QueriedLocation.Value.LatLongString());
                                var newDistance = Vector2d.Distance(location.Value.LatLongString(), failureMethod.QueriedLocation.Value.LatLongString());
                                if (newDistance < currentDistance)
                                {
                                    nearestLocation = location;
                                }
                            }
                        }
                    }
                    if (nearestLocation != null)
                    {
                        mapManager.HideLocationMarker(failureMethod.QueriedLocation);
                        bool updateText = failureMethod.UpdateLocationText;
                        mapManager.ShowLocationMarker(nearestLocation);
                        failureMethod.QueriedLocation.Apply(SetOperator.Assign, nearestLocation);
                        failureMethod.IsHandled = true;
                        string message = $"Location {failureMethod.QueriedLocation.Key} is inaccessible. The nearest location has been selected instead. Please head to: {nearestLocation.Key}";
                        DisplayMessage(message);
                        return FailureHandlingOutcome.Stop;
                    }
                }
            }
            return FailureHandlingOutcome.Continue;
        }

        [FailureHandlingMethod]
        private FailureHandlingOutcome Use_Backup_Node(FailureMethod failureMethod)
        {
            var engine = failureMethod.GetEngine();
            // If we find a node and the engine is available then we can jump to the node
            if (engine != null && failureMethod.BackupNode != null)
            {
                int index = 0;
                if (failureMethod.StartIndex >= 0 && failureMethod.StartIndex <= failureMethod.BackupNode.OrderList.Count)
                {
                    index = failureMethod.StartIndex;
                }
                engine.ExecuteNode(failureMethod.BackupNode, index);
                // Do we hide the location also?
                failureMethod.IsHandled = true;
                return FailureHandlingOutcome.Stop;
            }

            // Otherwise, we continue to the next method
            return FailureHandlingOutcome.Continue;
        }

        [FailureHandlingMethod]
        private void Disbale_Location_Behaviour(FailureMethod failureMethod)
        {
            // Find any reference to location - if on node then set node to cannot execute
            // If on order then do the same to the parent node
            var engine = failureMethod.GetEngine();
            if (engine != null)
            {
                var mapManager = engine.GetMapManager();
                if (mapManager != null)
                {
                    mapManager.HideLocationMarker(failureMethod.QueriedLocation);
                }

                var nodes = engine.GetComponents<Node>();
                foreach (var node in nodes)
                {
                    // If the node uses the same location as the failure method then it cannot execute
                    if (node.NodeLocation != null && Equals(node.NodeLocation.Value, failureMethod.QueriedLocation.Value))
                    {
                        node.NodeComplete = true;
                        node.CanExecuteAgain = false;
                        failureMethod.IsHandled = true;
                    }
                    foreach (var order in node.OrderList)
                    {
                        // If the order uses the same location as the failure method then the parent node cannot execute
                        if (order.GetOrderLocation() != null && Equals(order.GetOrderLocation(), failureMethod.QueriedLocation.Value))
                        {
                            node.NodeComplete = true;
                            node.CanExecuteAgain = false;
                            failureMethod.IsHandled = true;
                        }
                    }
                }
            }
        }

        private string DisplayMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return null;
            }
            var dialogueBox = DialogueBox.GetDialogueBox();
            if (dialogueBox == null)
            {
                return null;
            }

            //dialogueBox.SetStoryText(message);
            //dialogueBox.SetActive(true);
            //dialogueBox.StartDialogue(0.04f, 1f, true, true, false, delegate
            //{
            //});
            return message;
        }
    }

    //[InitializeOnLoad]
    public class LocationFailureHandlerInitializer
    {
        static LocationFailureHandlerInitializer()
        {
            LocationFailureHandler.GetAvailableMethods(); // This will trigger method registration
        }
    }

    [Serializable]
    public class FailureMethod
    {
        public bool foldout = true;

        //[Header("Failure Method and Location")]
        [Tooltip("The location that this method is associated with")]
        [SerializeField] protected LocationVariable queriedLocation;
        [Tooltip("A list of methods that can be executed to handle the failure")]
        [SerializeField] protected List<string> priorityMethods = new List<string>();
        //[Header("Backup Locations")]
        [Tooltip("A list of locations that can be used as alternatives")]
        [SerializeField] protected List<LocationVariable> backupLocations = new List<LocationVariable>();
        [Tooltip("Whether the failure has been handled")]
        [SerializeField] protected bool isHandled = false;
        [Tooltip("Whether the location text should be updated when the location is changed")]
        [SerializeField] protected bool updateLocationText = false;
        //[Header("Node Jump Settings")]
        [Tooltip("The node to jump to if the location is inaccessible")]
        [SerializeField] protected Node backupNode;
        [Tooltip("The index of the order list to start from on the backup node")]
        [SerializeField] protected int startIndex = 0;
        //[Header("Radius Increase Settings")]
        [Tooltip("Whether the radius of the location can be increased more than once")]
        [SerializeField] protected bool allowContinuousIncrease = false;
        [Tooltip("The size of the radius increase in meters")]
        [SerializeField] protected float radiusIncreaseSize = 50.0f;

        // This is a special case where we need to ensure that the radius has been increased
        private bool hasIncreased = false;

        public LocationVariable QueriedLocation { get => queriedLocation; }
        public List<LocationVariable> BackupLocations { get => backupLocations; }
        public List<string> PriorityMethods { get => priorityMethods; set => priorityMethods = value; }
        public bool IsHandled { get => isHandled; set => isHandled = value; }
        public bool AllowContinuousIncrease { get => allowContinuousIncrease; }
        public bool UpdateLocationText { get => updateLocationText; }
        public bool HasIncreased { get => hasIncreased; set => hasIncreased = value; }
        public Node BackupNode { get => backupNode; }
        public int StartIndex { get => startIndex; }
        public float RadiusIncreaseSize { get => radiusIncreaseSize; }

        public FailureMethod(LocationVariable queriedLocation, List<string> priorityMethods = null, List<LocationVariable> backupLocations = null)
        {
            this.queriedLocation = queriedLocation;
            this.priorityMethods = priorityMethods ?? new List<string>();
            this.backupLocations = backupLocations ?? new List<LocationVariable>();

            // Set default values for other fields
            this.foldout = true;
            this.isHandled = false;
            this.updateLocationText = false;
            this.backupNode = null;
            this.startIndex = 0;
            this.allowContinuousIncrease = false;
            this.radiusIncreaseSize = 50.0f;
            this.hasIncreased = false;
        }

        public BasicFlowEngine GetEngine()
        {
            var engine = BasicFlowEngine.CachedEngines.Find(e => e != null);
            if (engine == null)
            {
                engine = UnityEngine.Object.FindObjectOfType<BasicFlowEngine>();
            }
            return engine;
        }
    }
}