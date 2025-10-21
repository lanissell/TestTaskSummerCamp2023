using Saritasa.UPV.PythonInstaller;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Saritasa.UPV.ProjectStructureValidator
{
    /// <summary>
    /// Provides tools for validating the Unity project structure using an external Python script,
    /// and for generating or validating a GitHub Action workflow file for continuous validation.
    /// </summary>
    public static class ProjectStructureValidator
    {
        private const string MenuItemPath = "Project Validators/Project Structure Validator";

        private static readonly string PathToWorkspace = Application.dataPath.CombineAsPath("Vendors")
            .CombineAsPath("SaritasaUnityProjectValidators")
            .CombineAsPath("ProjectStructureValidator");

        private static readonly string PathToExecutableFile = PathToWorkspace.CombineAsPath("project_structure_validator.py");
        private static readonly string PathToConfig = PathToWorkspace.CombineAsPath("project_structure_config.json");
        private static readonly string PathToCheckedFolder = Application.dataPath;

        private static readonly string GitActionFileName = "project-structure-validator-runner.yaml";
        private static readonly string PathToGitActionTemplate = PathToWorkspace.CombineAsPath("Workflow").CombineAsPath(GitActionFileName);

        private static readonly string PythonVersion = $"3.12.10";

        private static CancellationTokenSource cts;
        private static PythonInstallerBase pythonInstaller;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            //Create the appropriate installer based on platform.
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                pythonInstaller = new MacPythonInstaller(PythonVersion);
            }
            else
            {
                pythonInstaller = new WindowsPythonInstaller(PythonVersion);
            }
        }

        /// <summary>
        /// Validates the project structure by running a Python script that checks the files
        /// according to the predefined configuration.  
        /// </summary>
        [MenuItem(MenuItemPath + "/Validate project structure")]
        public static void CheckProjectStructure()
        {
            if (!File.Exists(PathToExecutableFile))
            {
                Debug.LogError($"File not found: {PathToExecutableFile}.");
                return;
            }

            if (pythonInstaller.InstallStarted)
            {
                Debug.LogWarning("Wait for python install to finish.");
                return;
            }

            cts?.Cancel();
            cts = new CancellationTokenSource();

            if (!pythonInstaller.IsPythonInstalled())
            {
                InstallPythonAndCheckProjectStructureAsync(cts.Token);
                return;
            }

            CheckProjectStructureAsync(cts.Token);
        }

        /// <summary>
        /// Creates or validates the GitHub Actions workflow file for automated project structure checks.  
        /// If the workflow file doesn't exist, it will be created from a template.
        /// If it exists, the method compares it with the template and offers to replace it if differences are found.
        /// Also copies README file with workflow documentation to the workflow folder.
        /// </summary>
        [MenuItem(MenuItemPath + "/Create\\Validate Workflow File")]
        public static void CreateValidateGitActionFile()
        {
            // 1. Get path.
            var pathToRootFolder = Application.dataPath.CombineAsPath("..").CombineAsPath("..").CombineAsPath("..");
            var pathToGitHubFolder = pathToRootFolder.CombineAsPath(".github");
            var pathToWorkFlows = pathToGitHubFolder.CombineAsPath("workflows");

            // 2. Check that .github, workflows and project-structure-validator folders exist.
            if (!Directory.Exists(pathToGitHubFolder))
            {
                Directory.CreateDirectory(pathToGitHubFolder);
            }

            if (!Directory.Exists(pathToWorkFlows))
            {
                Directory.CreateDirectory(pathToWorkFlows);
            }

            var gitActionFilePath = pathToWorkFlows.CombineAsPath(GitActionFileName);

            // 3. If workflow file does not exist - create both workflow and README.
            if (!File.Exists(gitActionFilePath))
            {
                CreateGitActionFile(gitActionFilePath);
            }
            else
            {
                // 3.1 Validate existing Git Action file with template using CompareFiles.
                bool yamlIdentical = CompareFiles(PathToGitActionTemplate, gitActionFilePath);

                if (yamlIdentical)
                {
                    Debug.Log("Project structure checker YAML files are identical.");
                }
                else
                {
                    bool fixYaml = EditorUtility.DisplayDialog(
                        "Project structure checker YAML files are different",
                        "Change the yaml file according to the template?",
                        "Yes",
                        "No"
                    );

                    if (fixYaml)
                    {
                        CreateGitActionFile(gitActionFilePath);
                    }
                }
            }
        }

        /// <summary>
        /// Compare two text files.
        /// </summary>
        /// <returns>
        /// <c>True</c> if both files exist and contents are exactly equal.
        /// </returns>
        private static bool CompareFiles(string sourcePath, string destPath)
        {
            try
            {
                if (!File.Exists(destPath))
                {
                    return false;
                }

                string srcText = File.ReadAllText(sourcePath);
                string dstText = File.ReadAllText(destPath);
                return string.Equals(srcText, dstText, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to compare files: {ex.Message}\nSource: {sourcePath}\nDest: {destPath}");
                return false;
            }
        }

        private static void CreateGitActionFile(string gitActionFilePath)
        {
            try
            {
                if (File.Exists(gitActionFilePath))
                {
                    File.Delete(gitActionFilePath);
                }

                File.Copy(PathToGitActionTemplate, gitActionFilePath);
                Debug.Log($"Copied Git Action yaml at: {gitActionFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"Failed to copy GitHub Actions workflow: {ex.Message}\n" +
                    $"Template workflow: {PathToGitActionTemplate}\n" +
                    $"Existing workflow: {gitActionFilePath}");
            }
        }

        private static async void InstallPythonAndCheckProjectStructureAsync(CancellationToken ct)
        {
            await pythonInstaller.InstallPythonAsync(ct);

            CheckProjectStructureAsync(ct);
        }

        private static async void CheckProjectStructureAsync(CancellationToken ct)
        {
            using var process = new Process();

            try
            {
                process.StartInfo.FileName = pythonInstaller.PythonExecutableName;
                process.StartInfo.Arguments =
                    $"\"{PathToExecutableFile}\" --config \"{PathToConfig}\" --dir \"{PathToCheckedFolder}\"";

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string errorOutput = await process.StandardError.ReadToEndAsync();

                await Task.Run(() => process.WaitForExit(), ct);

                if (!string.IsNullOrWhiteSpace(errorOutput))
                {
                    Debug.LogError($"<b><color=red>[Project Structure Checker] Python error:</color></b>\n{errorOutput}");
                    return;
                }

                if (string.IsNullOrWhiteSpace(output))
                {
                    Debug.LogWarning("[Project Structure Checker] No output received from Python script.");
                    return;
                }

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 1 && lines[ 0 ].Contains("All files comply", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"<b><color=green>{lines[ 0 ]}</color></b>");
                    return;
                }

                Debug.LogWarning("<b>[Project Structure Checker] Found files that violate the rules:</b>");

                string currentHint = string.Empty;

                foreach( string raw in lines )
                {
                    string line = raw.Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (!line.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning($"<color=#cc9a05>└─ <i>{line}</i></color>");
                        continue;
                    }

                    if (currentHint != line)
                    {
                        currentHint = line;
                        Debug.LogWarning($"<color=#ffc107>{line}</color>");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Project structure check cancelled.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Project Structure Checker] Error: {e.Message}");
            }
        }

        private static string CombineAsPath(this string path, string otherPath)
        {
            return Path.Combine(path, otherPath);
        }
    }
}
