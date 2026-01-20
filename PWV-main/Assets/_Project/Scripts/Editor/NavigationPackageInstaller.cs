using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Instala automáticamente el paquete de Navigation para Unity
    /// </summary>
    public class NavigationPackageInstaller : EditorWindow
    {
        private static AddRequest _addRequest;
        
        [MenuItem("EtherDomes/Install Navigation Package")]
        public static void InstallNavigationPackage()
        {
            Debug.Log("[NavigationPackageInstaller] Installing AI Navigation package...");
            
            // Instalar el paquete de Navigation
            _addRequest = Client.Add("com.unity.ai.navigation");
            EditorApplication.update += CheckInstallProgress;
        }
        
        private static void CheckInstallProgress()
        {
            if (_addRequest.IsCompleted)
            {
                EditorApplication.update -= CheckInstallProgress;
                
                if (_addRequest.Status == StatusCode.Success)
                {
                    Debug.Log("[NavigationPackageInstaller] AI Navigation package installed successfully!");
                    Debug.Log("[NavigationPackageInstaller] You can now access Window > AI > Navigation");
                    
                    // Mostrar mensaje de éxito
                    EditorUtility.DisplayDialog(
                        "Navigation Package Installed", 
                        "AI Navigation package has been installed successfully!\n\n" +
                        "You can now access:\n" +
                        "• Window > AI > Navigation\n" +
                        "• Navigation Static checkbox in Inspector\n" +
                        "• NavMesh baking tools", 
                        "OK"
                    );
                }
                else
                {
                    Debug.LogError($"[NavigationPackageInstaller] Failed to install AI Navigation package: {_addRequest.Error.message}");
                    
                    // Mostrar mensaje de error
                    EditorUtility.DisplayDialog(
                        "Installation Failed", 
                        $"Failed to install AI Navigation package:\n{_addRequest.Error.message}\n\n" +
                        "Please try installing manually:\n" +
                        "1. Open Window > Package Manager\n" +
                        "2. Click '+' > Add package by name\n" +
                        "3. Enter: com.unity.ai.navigation", 
                        "OK"
                    );
                }
                
                _addRequest = null;
            }
        }
        
        [MenuItem("EtherDomes/Check Navigation Package")]
        public static void CheckNavigationPackage()
        {
            var listRequest = Client.List();
            EditorApplication.update += () => CheckListProgress(listRequest);
        }
        
        private static void CheckListProgress(ListRequest listRequest)
        {
            if (listRequest.IsCompleted)
            {
                EditorApplication.update -= () => CheckListProgress(listRequest);
                
                if (listRequest.Status == StatusCode.Success)
                {
                    bool navigationFound = false;
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name == "com.unity.ai.navigation")
                        {
                            navigationFound = true;
                            Debug.Log($"[NavigationPackageInstaller] AI Navigation package found: {package.version}");
                            break;
                        }
                    }
                    
                    if (!navigationFound)
                    {
                        Debug.LogWarning("[NavigationPackageInstaller] AI Navigation package not found. Use 'EtherDomes > Install Navigation Package' to install it.");
                    }
                    else
                    {
                        Debug.Log("[NavigationPackageInstaller] AI Navigation package is installed and ready to use!");
                    }
                }
            }
        }
    }
}