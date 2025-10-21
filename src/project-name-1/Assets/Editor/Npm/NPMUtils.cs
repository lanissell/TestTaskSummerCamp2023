using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Saritasa.UBT;
using Debug = UnityEngine.Debug;
using Saritasa.UBT.GitVersion;

public static class NPMUtils
{
    //Registry field name in yaml file.
    private const string RegistryUrl = "https://nexus.saritasa.io/repository/saritasa-unity-npm/";

    //Path to global .npmrc file.
    private static readonly string globalNpmrcPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        .CombineAsPath(".npmrc");

    //Path to package.
    private static readonly string packagesFolderPath = PathUtils.GetSolutionPath()
        .CombineAsPath("Packages");

    /// <summary>
    /// True if .npmrc file exists in the user's folder.
    /// </summary>
    /// <returns></returns>
    public static bool IsLoggedIn()
    {
        return File.Exists(packagesFolderPath.CombineAsPath(".npmrc"));
    }

    /// <summary>
    /// Show log in window to allow log in in to NPM nexus server.
    /// </summary>
    public static void ShowLoginWindow()
    {
        NPMLoginWindow.ShowWindow();
    }
    
    /// <summary>
    /// Login into npm registry. It calls npm-cli-login command that creates .npmrc file and
    /// copies that file in to the package folder. This file will be used during publishing to
    /// NPM nexus server.
    /// </summary>
    public static void Login(string username, string password, string email)
    {
        string registry = RegistryUrl;

        //Check for npm-cli-login.
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/C npm install -g npm-cli-login",
            Verb = "runas",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal
        };
        var process = Process.Start(processStartInfo);
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            //Start login process.
            process = Process.Start("cmd.exe", string.Format(
                "/C npm-cli-login -u {1} -p {2} -e {3} -r {0} --config-path {4}",
                registry.Remove(registry.Length - 1, 1), username, password, email, packagesFolderPath.CombineAsPath(".npmrc")));
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Debug.LogError("Login failed");
            }
        }
        else
        {
            Debug.LogError($"Npm-cli-login was not setup. ExitCode {process.ExitCode}");
        }
    }

    private static void SetPackageVersion(string packageFolderPath)
    {
        try
        {
            var packageVersion = GitVersionCLI.GetVersionVariables().SemVer;

            if (string.IsNullOrWhiteSpace(packageVersion))
            {
                throw new InvalidOperationException("GitVersion returned empty version.");
            }

            var packageJsonFileName = "package.json";
            var packageJsonFilePath = packageFolderPath.CombineAsPath(packageJsonFileName);

            if (!File.Exists(packageJsonFilePath))
            {
                throw new FileNotFoundException($"{packageJsonFilePath} not found.");
            }

            var packageJsonFileContent = File.ReadAllText(packageJsonFilePath);
            var config = JsonConvert.DeserializeObject<NPMJsonDataModels.NPMPackageDataModel>(packageJsonFileContent);
            config.version = packageVersion;

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            var updatedJson = JsonConvert.SerializeObject(config, settings);

            File.WriteAllText(packageJsonFilePath, updatedJson);

            Debug.Log($"Updated {packageJsonFileName} version to: {packageVersion}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error updating package.json: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Publish npm package into NPM registry.
    /// </summary>
    /// <param name="packageFolderPath">Folder that will be packed in to package.</param>
    public static void PublishNPMPackage(string packageFolderPath)
    {
        File.Copy(packagesFolderPath.CombineAsPath(".npmrc"), packageFolderPath.CombineAsPath(".npmrc"), true);

        SetPackageVersion(packageFolderPath);

        // Determine if this is a prerelease version
        var packageJsonFilePath = packageFolderPath.CombineAsPath("package.json");
        var packageJsonFileContent = File.ReadAllText(packageJsonFilePath);
        var config = JsonConvert.DeserializeObject<NPMJsonDataModels.NPMPackageDataModel>(packageJsonFileContent);
        
        // Check if version contains prerelease identifiers (e.g., alpha, beta, rc)
        string tag = "latest";
        if (!string.IsNullOrEmpty(config.version) && (config.version.Contains("-alpha") || config.version.Contains("-beta")))
        {
            tag = "prerelease";
        }

        //Publish package.
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = string.Format("/C cd {0} && npm publish --registry={1} --tag={2}", packageFolderPath, RegistryUrl, tag),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        var process = Process.Start(processStartInfo);
        
        // Read output streams to prevent buffer overflow and hanging
        var standardOutput = process.StandardOutput.ReadToEnd();
        var errorOutput = process.StandardError.ReadToEnd();
        
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            Debug.Log($"Publish success (tag: {tag})");
            if (!string.IsNullOrWhiteSpace(standardOutput))
            {
                Debug.Log($"Output: {standardOutput}");
            }
        }
        else
        {
            Debug.LogError($"Publish failed {process.ExitCode} {errorOutput}");
        }
    }
}