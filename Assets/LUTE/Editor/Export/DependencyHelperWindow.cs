using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    public class DependencyHelperWindow : EditorWindow
    {
        // List of required packages with display names and file paths
        private List<DependencyPackage> requiredPackages = new List<DependencyPackage>();

        // Status tracking
        private Dictionary<string, bool> packageStatus = new Dictionary<string, bool>();
        private Dictionary<string, bool> sampleStatus = new Dictionary<string, bool>();
        private ListRequest listRequest;
        private bool isCheckingPackages = false;
        private bool isSamplesCheckInitiated = false;
        private string xrInteractionToolkitId = "com.unity.xr.interaction.toolkit";

        // GUI styling
        private GUIStyle headerStyle;
        private GUIStyle packageNameStyle;
        private GUIStyle statusStyle;
        private GUIStyle sampleHeaderStyle;

        [MenuItem("LUTE/Utilities/Dependency Helper")]
        public static void ShowWindow()
        {
            var window = GetWindow<DependencyHelperWindow>("Dependency Helper");
            window.minSize = new Vector2(400, 350);
            window.Show();
        }

        private void OnEnable()
        {
            // Initialize required packages
            InitializePackageList();

            // Initialize styles
            InitializeStyles();

            // Start checking package status
            CheckPackageStatus();
        }

        private void InitializePackageList()
        {
            // Add XR packages based on the requirements
            requiredPackages.Add(new DependencyPackage(
                "AR Foundation",
                "com.unity.xr.arfoundation",
                "Packages/DependencyHelper/PackagesToImport/ARFoundation.unitypackage",
                "6.0.5"
            ));

            requiredPackages.Add(new DependencyPackage(
                "Google ARCore XR Plugin",
                "com.unity.xr.arcore",
                "Packages/DependencyHelper/PackagesToImport/GoogleARCoreXRPlugin.unitypackage",
                "6.0.5"
            ));

            requiredPackages.Add(new DependencyPackage(
                "XR Core Utilities",
                "com.unity.xr.core-utils",
                "Packages/DependencyHelper/PackagesToImport/XRCoreUtilities.unitypackage",
                "2.5.1"
            ));

            requiredPackages.Add(new DependencyPackage(
                "XR Interaction Toolkit",
                xrInteractionToolkitId,
                "Packages/DependencyHelper/PackagesToImport/XRInteractionToolkit.unitypackage",
                "3.0.7",
                new Dictionary<string, string> {
                    { "Starter Assets", "Assets to streamline setup of behaviors, including a default set of input actions and presets for use with XR Interaction Toolkit behaviors that use the Input System." }
                }
            ));

            requiredPackages.Add(new DependencyPackage(
                "XR Legacy Input Helpers",
                "com.unity.xr.legacyinputhelpers",
                "Packages/DependencyHelper/PackagesToImport/XRLegacyInputHelpers.unitypackage",
                "2.1.12"
            ));

            requiredPackages.Add(new DependencyPackage(
                "XR Plugin Management",
                "com.unity.xr.management",
                "Packages/DependencyHelper/PackagesToImport/XRPluginManagement.unitypackage",
                "4.5.0"
            ));

            // Initialize status dictionary
            foreach (var package in requiredPackages)
            {
                packageStatus[package.Id] = false;

                // Initialize sample status if the package has samples
                if (package.Samples != null && package.Samples.Count > 0)
                {
                    foreach (var sample in package.Samples)
                    {
                        sampleStatus[$"{package.Id}:{sample.Key}"] = false;
                    }
                }
            }
        }

        private void InitializeStyles()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 20)
            };

            packageNameStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            statusStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight
            };

            sampleHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 15, 5)
            };
        }

        private void CheckPackageStatus()
        {
            if (isCheckingPackages) return;

            isCheckingPackages = true;
            listRequest = Client.List();
            EditorApplication.update += CheckPackageListProgress;
        }

        private void CheckPackageListProgress()
        {
            if (listRequest == null || !listRequest.IsCompleted) return;

            EditorApplication.update -= CheckPackageListProgress;

            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var package in requiredPackages)
                {
                    packageStatus[package.Id] = false;

                    foreach (var installedPackage in listRequest.Result)
                    {
                        if (installedPackage.name == package.Id)
                        {
                            // Check if installed version matches or is newer than the required version
                            if (string.IsNullOrEmpty(package.Version) ||
                                CompareVersions(installedPackage.version, package.Version) >= 0)
                            {
                                packageStatus[package.Id] = true;

                                // If the package has samples, check them
                                if (package.Samples != null && package.Samples.Count > 0)
                                {
                                    CheckSampleStatus(package, installedPackage);
                                }
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to check package status: {listRequest.Error.message}");
            }

            isCheckingPackages = false;
            isSamplesCheckInitiated = true;
            Repaint();
        }

        private void CheckSampleStatus(DependencyPackage package, UnityEditor.PackageManager.PackageInfo installedPackage)
        {
            if (package.Id == xrInteractionToolkitId)
            {
                // Check if Starter Assets sample is imported
                string sampleId = $"{package.Id}:Starter Assets";

                // Check if the sample is imported in the project
                string samplePath = $"Assets/Samples/{package.DisplayName}/{installedPackage.version}/Starter Assets";
                bool isSampleImported = Directory.Exists(samplePath);

                sampleStatus[sampleId] = isSampleImported;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Dependency Helper", headerStyle);

            EditorGUILayout.HelpBox("Check and import required packages for this project.", MessageType.Info);
            EditorGUILayout.Space(10);

            if (isCheckingPackages)
            {
                EditorGUILayout.LabelField("Checking package status...");
                return;
            }

            DrawPackageList();

            EditorGUILayout.Space(10);

            // Draw samples list if we have packages with samples
            if (isSamplesCheckInitiated)
            {
                DrawSamplesList();
            }

            EditorGUILayout.Space(20);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Refresh Status", GUILayout.Width(120)))
                {
                    CheckPackageStatus();
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Import All Missing", GUILayout.Width(150)))
                {
                    ImportAllMissing();
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawPackageList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var package in requiredPackages)
            {
                bool isInstalled = packageStatus[package.Id];

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(package.DisplayName, packageNameStyle);
                EditorGUILayout.LabelField($"{package.Id} ({package.Version})", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if (isInstalled)
                {
                    GUI.enabled = false;
                    EditorGUILayout.LabelField("✓ Installed", statusStyle, GUILayout.Width(90));
                    GUI.enabled = true;
                }
                else
                {
                    if (GUILayout.Button("Import", GUILayout.Width(90)))
                    {
                        ImportPackage(package);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSamplesList()
        {
            bool hasSamples = false;

            // First check if any of our packages have samples
            foreach (var package in requiredPackages)
            {
                if (package.Samples != null && package.Samples.Count > 0 && packageStatus[package.Id])
                {
                    hasSamples = true;
                    break;
                }
            }

            if (!hasSamples)
                return;

            EditorGUILayout.LabelField("Package Samples", sampleHeaderStyle);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var package in requiredPackages.Where(p => p.Samples != null && p.Samples.Count > 0 && packageStatus[p.Id]))
            {
                foreach (var sample in package.Samples)
                {
                    string sampleId = $"{package.Id}:{sample.Key}";
                    bool isInstalled = sampleStatus[sampleId];

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField($"{sample.Key} ({package.DisplayName})", packageNameStyle);
                    EditorGUILayout.LabelField(sample.Value, EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    if (isInstalled)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.LabelField("✓ Imported", statusStyle, GUILayout.Width(90));
                        GUI.enabled = true;
                    }
                    else
                    {
                        if (GUILayout.Button("Import", GUILayout.Width(90)))
                        {
                            ImportSample(package, sample.Key);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void ImportAllMissing()
        {
            // First import all missing packages
            foreach (var package in requiredPackages)
            {
                if (!packageStatus[package.Id])
                {
                    ImportPackage(package);
                }
            }

            // Schedule import of samples after packages are imported
            EditorApplication.delayCall += () =>
            {
                // Import missing samples for installed packages
                foreach (var package in requiredPackages.Where(p => packageStatus[p.Id] && p.Samples != null && p.Samples.Count > 0))
                {
                    foreach (var sample in package.Samples)
                    {
                        string sampleId = $"{package.Id}:{sample.Key}";
                        if (!sampleStatus[sampleId])
                        {
                            ImportSample(package, sample.Key);
                        }
                    }
                }
            };
        }

        private void ImportPackage(DependencyPackage package)
        {
            string packagePath = package.PackagePath;

            if (!File.Exists(packagePath))
            {
                // If package file is not found, try using Package Manager API
                Debug.Log($"Package file not found at {packagePath}. Attempting to install via Package Manager...");
                InstallViaPackageManager(package);
                return;
            }

            AssetDatabase.ImportPackage(packagePath, true);

            // After importing, we schedule a status refresh
            EditorApplication.delayCall += () =>
            {
                CheckPackageStatus();
            };
        }

        private void InstallViaPackageManager(DependencyPackage package)
        {
            string packageId = package.Id;
            string version = !string.IsNullOrEmpty(package.Version) ? $"@{package.Version}" : "";

            EditorUtility.DisplayProgressBar("Installing Package",
                $"Installing {package.DisplayName} {package.Version}...", 0.5f);

            Client.Add($"{packageId}{version}");

            // Schedule status refresh
            EditorApplication.delayCall += () =>
            {
                EditorUtility.ClearProgressBar();
                CheckPackageStatus();
            };
        }

        private void ImportSample(DependencyPackage package, string sampleName)
        {
            if (package.Id != xrInteractionToolkitId)
                return;

            // Find the package in Package Manager
            foreach (var installedPackage in listRequest.Result)
            {
                if (installedPackage.name == package.Id)
                {
                    // Start sample import
                    ImportXRInteractionSample(installedPackage, sampleName);
                    break;
                }
            }
        }

        private void ImportXRInteractionSample(UnityEditor.PackageManager.PackageInfo package, string sampleName)
        {
            // We need to access the Package Manager window via reflection
            System.Reflection.Assembly editorAssembly = typeof(Editor).Assembly;
            System.Type packageManagerWindowType = editorAssembly.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");

            if (packageManagerWindowType != null)
            {
                // Get the window
                var getWindowMethod = packageManagerWindowType.GetMethod("GetWindow",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (getWindowMethod != null)
                {
                    var packageManagerWindow = getWindowMethod.Invoke(null, null);

                    // Try to show the sample for our package
                    Debug.Log($"Opening Package Manager to import {sampleName} from {package.displayName}");
                    EditorUtility.DisplayDialog("Import XR Interaction Toolkit Sample",
                        $"To complete setup, you need to import the '{sampleName}' sample from the XR Interaction Toolkit package.\n\n" +
                        "The Package Manager will now open. Please:\n" +
                        "1. Click on 'XR Interaction Toolkit' in the package list\n" +
                        "2. Scroll to find the 'Samples' section\n" +
                        "3. Click the 'Import' button next to 'Starter Assets'\n\n" +
                        "After importing, return to the Dependency Helper window and click 'Refresh Status'.",
                        "OK");

                    // Open Package Manager window and show the package
                    var menuItem = "Window/Package Manager";
                    EditorApplication.ExecuteMenuItem(menuItem);

                    // Schedule a check to see if the samples were imported
                    EditorApplication.delayCall += () =>
                    {
                        CheckPackageStatus();
                    };
                }
            }
        }

        // Utility function to compare version strings
        private int CompareVersions(string version1, string version2)
        {
            // Parse version strings into numbers
            var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var v2Parts = version2.Split('.').Select(int.Parse).ToArray();

            // Compare each part
            for (int i = 0; i < Mathf.Min(v1Parts.Length, v2Parts.Length); i++)
            {
                if (v1Parts[i] > v2Parts[i]) return 1;
                if (v1Parts[i] < v2Parts[i]) return -1;
            }

            // If one version has more parts than the other
            return v1Parts.Length.CompareTo(v2Parts.Length);
        }
    }

    // Class to store package information
    public class DependencyPackage
    {
        public string DisplayName { get; private set; }
        public string Id { get; private set; }
        public string PackagePath { get; private set; }
        public string Version { get; private set; }
        public Dictionary<string, string> Samples { get; private set; }

        public DependencyPackage(string displayName, string id, string packagePath, string version = "", Dictionary<string, string> samples = null)
        {
            DisplayName = displayName;
            Id = id;
            PackagePath = packagePath;
            Version = version;
            Samples = samples;
        }
    }
}