using UnityEditor;

/// <summary>
/// This class contains all commands available in the Editor main menu.
/// </summary>
public static class SaritasaPackageTools
{

    [MenuItem("Package Tools/Generate Unity packages")]
    public static void GenerateForBuildTools()
    {
        string ProjectValidatorsPath = "Assets/Vendors/SaritasaUnityProjectValidators/";
        string SaritasaPackagePath = "Packages/SaritasaUnityProjectValidators/";
        UnityPackageUtils.GenerateUnityPackagesForSubfolders(ProjectValidatorsPath, SaritasaPackagePath);
    }

    [MenuItem("Package Tools/Publish NPM package")]
    public static void PublishBuildTools()
    {
        if (NPMUtils.IsLoggedIn())
        {
            GenerateForBuildTools();
            NPMUtils.PublishNPMPackage("Packages/SaritasaUnityProjectValidators");
        }
        else
        {
            NPMUtils.ShowLoginWindow();
        }
    }

    [MenuItem("Package Tools/Log in to Nexus NPM")]
    public static void LogInToNexusNPM()
    {
        NPMUtils.ShowLoginWindow();
    }
}