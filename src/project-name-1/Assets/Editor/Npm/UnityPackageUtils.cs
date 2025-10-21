using System.IO;
using UnityEditor;

/// <summary>
/// Utility class for generating Unity packages.
/// </summary>
public static class UnityPackageUtils
{
    private const string UnityPackageExtension = ".unitypackage";

    /// <summary>
    /// Generates separate Unity Packages for each sub folder in the specified root directory.
    /// </summary>
    /// <param name="rootDirPath">Root directory which sub folders will be exported for.</param>
    /// <param name="outputDirPath">The output directory where all packages will be saved.</param>
    public static void GenerateUnityPackagesForSubfolders(string rootDirPath, string outputDirPath)
    {
        string[] foldersForExport = Directory.GetDirectories(rootDirPath);

        foreach (string currentFolderForExport in foldersForExport)
        {
            string newPackageName = currentFolderForExport.Substring(currentFolderForExport.LastIndexOf('/') + 1) + UnityPackageExtension;
            AssetDatabase.ExportPackage(currentFolderForExport, outputDirPath + newPackageName,  ExportPackageOptions.Recurse);
        }

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Generates separate Unity packages for each selected asset in the Project Window.
    /// </summary>
    public static void GenerateUnityPackagesForSelectedFiles()
    {
        var assets = Selection.objects;

        var packagePathForExport = EditorUtility.OpenFolderPanel("Export packages to", "Packages/", "");
        foreach (var asset in assets)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            var newPackageName = assetPath.Substring(assetPath.LastIndexOf('/') + 1) + UnityPackageExtension;

            AssetDatabase.ExportPackage(assetPath, packagePathForExport + '/' + newPackageName, ExportPackageOptions.Recurse);
        }
    }
}