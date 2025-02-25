using UnityEditor;
using UnityEngine;

//Generates a context menu item which when clicked on creates a new order class
//New classes are created in the generic order folder and are named "NewOrder" - eventually a new window will popup to allow the user to rename the new order
//New classes are filled with the default order template to allow for easy creation of new orders
public class NewOrder : EditorWindow
{
    private static string orderName = "";
    private static string orderCategory = "";
    private static string orderDescription = "";

    [UnityEditor.MenuItem("LUTE/Utilities/Orders/Create New Order")]
    public static void NewOrderClass()
    {
        //rather than just create a new order, we create a new window with a text field to allow the user to categorise, name, and describe the new order
        //this window has a button which calls the CreateNewOrder method below
        NewOrder window = (NewOrder)EditorWindow.GetWindow(typeof(NewOrder));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Create New Order", EditorStyles.boldLabel);

        orderCategory = EditorGUILayout.TextField("Order Category:", orderCategory);
        orderName = EditorGUILayout.TextField("Order Name:", orderName);
        orderDescription = EditorGUILayout.TextField("Order Description:", orderDescription);

        if (GUILayout.Button("Create New Order"))
        {
            CreateNewOrder();
            Close();
        }
    }

    private static void CreateNewOrder()
    {
        string filePath = "Assets/LUTE/Scripts/Orders/UserCreated";
        //if the order name has spaces, we must convert this to camel case and use it as the file name and class name
        string camelCaseName = orderName.Replace(" ", "");
        string fileName = camelCaseName + ".cs";
        string fileText = "using UnityEngine;\n\n" +
                          "[OrderInfo(\"" + orderCategory + "\",\n" +
                          "              \"" + orderName + "\",\n" +
                          "              \"" + orderDescription + "\")]\n" +
                          "[AddComponentMenu(\"\")]\n" +
                          "public class " + camelCaseName + " : Order\n" +
                          "{\n" +
                          "    public override void OnEnter()\n" +
                          "    {\n" +
                          "      //this code gets executed as the order is called\n" +
                          "      //some orders may not lead to another node so you can call continue if you wish to move to the next order after this one   \n" +
                          "      //Continue();\n" +
                          "    }\n\n" +
                          "  public override string GetSummary()\n" +
                          "  {\n" +
                          " //you can use this to return a summary of the order which is displayed in the inspector of the order\n" +
                          "      return \"\";\n" +
                          "  }\n" +
                          "}";
        System.IO.File.WriteAllText(filePath + "/" + fileName, fileText);
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.ProjectWindowUtil.ShowCreatedAsset(UnityEditor.AssetDatabase.LoadAssetAtPath(filePath + "/" + fileName, typeof(UnityEngine.Object)));
    }
}


//class template:
//using UnityEngine;
//
// //This is a template for creating new orders - fill in the order name, description and category below to begin creating a new order
//[OrderInfo("Category",
//              "Order Name",
//              "Order Description")]
//[AddComponentMenu("")]
//public class NewOrder : Order
//{
//add your variables here - use the tooltip attribute to add a description to the variable (see example)
// //    [Tooltip("This is a tooltip")]
// //    [SerializeField] protected int myVariable = 0;
//
//    public override void OnEnter()
//    {
//      //this code gets executed as the order is called
//      //some orders may not lead to another node so you can call continue if you wish to move to the next order after this one   
//      //Continue();
//    }
//
//  public override string GetSummary()
//  {
// //you can use this to return a summary of the order which is displayed in the inspector of the order
//      return "";
//  }
//}