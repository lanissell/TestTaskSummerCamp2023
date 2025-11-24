using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Runs project validators using Python scripts.
/// </summary>
public static class ProjectValidatorsRunner
{
    /// <summary>
    /// The base path for validator menu items.
    /// </summary>
    public const string MenuItemStartPath = "Project Validatotrs/";

    private static readonly string pathToWorkspace = Path.Combine(
        Application.dataPath,
        "Vendors",
        "SaritasaUnityProjectValidators");

    private static readonly string projectFolder = Directory.GetParent(Application.dataPath).FullName;

    private static readonly string pathToLauncherScript = Path.Combine(
        projectFolder,
        "Packages",
        "SaritasaUnityProjectValidators",
        "validators_launcher.py"
    );

    private static string pythonScriptsPath = Path.Combine(
        Directory.GetParent(projectFolder).FullName,
        "saritasa-unity-project-validator-api",
        "Validators"
    );

    private static readonly Dictionary<string, string> validators = new Dictionary<string, string>();

    private static readonly string pythonVersion = $"3.12.10";
    private static PythonInstallerBase pythonInstaller;

    private static CancellationTokenSource internalCts;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        //Create the appropriate installer based on platform.
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            pythonInstaller = new MacPythonInstaller(pythonVersion);
        }
        else
        {
            pythonInstaller = new WindowsPythonInstaller(pythonVersion);
        }
    }

    /// <summary>
    /// Registers a validator by name and folder location.
    /// </summary>
    /// <param name="name">The name of the validator.</param>
    /// <param name="folderName">The folder name where the validator is located.</param>
    public static void RegisterValidatorByName(string name, string folderName)
    {
        var folderPath = Path.Combine(pathToWorkspace, folderName);

        if (validators.TryAdd(name, folderPath))
        {
            return;
        }

        validators[name] = folderPath ;
    }

    /// <summary>
    /// Checks the project asynchronously using the specified validator.
    /// </summary>
    /// <param name="validatorName">The name of the validator to run.</param>
    /// <param name="ct">Cancellation token for cancelling the operation.</param>
    public static async Task CheckProjectAsync(string validatorName, CancellationToken ct)
    {
        if (internalCts != null)
        {
            EditorUtility.DisplayDialog(
                "Project Structure Validator",
                "Please, wait until the current validation is finished.",
                "OK"
            );
            return;
        }

        internalCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        await CheckProjectInternalAsync(validatorName, internalCts.Token);

        internalCts?.Cancel();
        internalCts?.Dispose();
        internalCts = null;
    }

    /// <summary>
    /// Copies validator scripts from the API project to the destination.
    /// </summary>
    public static void CopyValidatorsScripts()
    {
        var validatorsInternal = new Dictionary<string, string>(validators)
        {
            { Path.GetFileNameWithoutExtension(pathToLauncherScript), Path.GetDirectoryName(pathToLauncherScript) }
        };

        foreach(var validator in validatorsInternal)
        {
            var validatorName = $"{validator.Key}.py";

            var sourcePath = Path.Combine(
                pythonScriptsPath,
                validatorName
            );

            var destinationPath = Path.Combine(
                validator.Value,
                validatorName
            );

            // Ensure source file exists
            if (!File.Exists(sourcePath))
            {
                Debug.LogError($"Source file not found: {sourcePath}");
                return;
            }

            // Ensure destination directory exists
            if (!Directory.Exists(validator.Value))
            {
                Directory.CreateDirectory(validator.Value);
            }

            // Copy file
            File.Copy(sourcePath, destinationPath, overwrite: true);
            Debug.Log($"Copied {validator.Key} to {destinationPath}");
        }

        AssetDatabase.Refresh();
    }

    [MenuItem(MenuItemStartPath + "Run all validators", priority = 2)]
    private static void RunAllValidators()
    {
        if (internalCts != null)
        {
            EditorUtility.DisplayDialog(
                "Project Structure Validator",
                "Please, wait until the current validation is finished.",
                "OK"
            );
            return;
        }

        internalCts = new CancellationTokenSource();
        CheckProjectWithAllValidatorsAsync(internalCts.Token);
    }

    private static async Task CheckProjectInternalAsync(string validatorName, CancellationToken ct)
    {
        try
        {
            if (!pythonInstaller.IsPythonInstalled())
            {
                await pythonInstaller.InstallPythonAsync(ct);
            }

            if (!validators.TryGetValue(validatorName, out var folderName))
            {
                Debug.LogError($"Validator '{validatorName}' is not registered.");
                return;
            }

            var pathToExecutableFile = Path.Combine(pathToWorkspace, folderName, $"{validatorName}.py");

            if (!File.Exists(pathToExecutableFile))
            {
                Debug.LogError($"File not found: {pathToExecutableFile}.");
                return;
            }

            var arguments = $"\"{pathToLauncherScript}\" --dir \"{Application.dataPath}\" --validators \"{pathToExecutableFile}\" --json";
            var processResult = await RunProcessAsync(pythonInstaller.PythonExecutableName, arguments, ct);

            if (!string.IsNullOrWhiteSpace(processResult.Item2))
            {
                Debug.LogError($"<b><color=red>[{validatorName}] Python error:</color></b>\n{processResult.Item2}");
                return;
            }

            if (string.IsNullOrWhiteSpace(processResult.Item1))
            {
                Debug.LogWarning($"[{validatorName}] No output received from Python script.");
                return;
            }

            var errorsByComment = ValidationResultParser.ParseValidatorErrors(processResult.Item1, validatorName);
            PrintResult(errorsByComment, validatorName);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Project validation cancelled.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Project Structure Validator] Error: {e.Message}");
        }
    }

    private static async void CheckProjectWithAllValidatorsAsync(CancellationToken ct)
    {
        try
        {
            var separator = $"<b><color=grey>{new string('=', 50)}</b>";

            foreach(var validatorName in validators.Keys)
            {
                Debug.LogWarning(separator);
                await CheckProjectInternalAsync(validatorName, ct);
                Debug.LogWarning(separator);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Project validation cancelled.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[All Validators] Error: {e.Message}");
        }
        finally
        {
            internalCts?.Cancel();
            internalCts?.Dispose();
            internalCts = null;
        }
    }

    private static void PrintResult(Dictionary<string, List<string>> results, string validatorName)
    {
        if (results == null || results.Count == 0)
        {
            Debug.Log($"<b><color=green>[{validatorName}] All files comply with the rules!</color></b>");
            return;
        }

        Debug.LogWarning($"<b>[{validatorName}] Found file(s) that violate the rules:</b>");

        foreach(var kvp in results)
        {
            Debug.LogWarning($"<color=#ffc107>{kvp.Key}</color>");
            foreach(var filePath in kvp.Value)
            {
                Debug.LogWarning($"<color=#cc9a05>└─ <i>{filePath}</i></color>");
            }
        }
    }

    private static async Task<(string, string)> RunProcessAsync(string pathToExecutableFile, string arguments, CancellationToken ct)
    {
        using var process = new Process();

        process.StartInfo.FileName = pathToExecutableFile;
        process.StartInfo.Arguments = arguments;

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string errorOutput = await process.StandardError.ReadToEndAsync();

        await Task.Run(() => process.WaitForExit(), ct);

        return (output, errorOutput);
    }
}