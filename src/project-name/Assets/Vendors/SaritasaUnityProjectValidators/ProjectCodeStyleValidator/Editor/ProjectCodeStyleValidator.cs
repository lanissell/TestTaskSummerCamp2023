using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SaritasaUnityProjectValidators.Editor
{
    /// <summary>
    /// Validator for project code style using .editorconfig and a Python script.
    /// </summary>
    public class ProjectCodeStyleValidator : MonoBehaviour
    {
        private const string MenuItemValidateCodeStyle = "/Code Style/";
        private const string ValidatorName = "code_style_validator";
        private const string ValidatorFolder = "ProjectCodeStyleValidator";

        private static readonly string PathToTemplate = Path.Combine(
            ProjectValidatorsRunner.PathToWorkspace,
            ValidatorFolder,
            "editorconfig_template.txt");
        private static readonly string PathToEditorConfig = Path.Combine(
            Application.dataPath,
            ".editorconfig");

        private static CancellationTokenSource validationCts;
        private static CancellationTokenSource fixCts;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            ProjectValidatorsRunner.RegisterValidatorByName(ValidatorName, ValidatorFolder);
        }

        [MenuItem(ProjectValidatorsRunner.MenuItemStartPath + MenuItemValidateCodeStyle + "Validate Project Code Style", false, 20)]
        private static void CheckProjectCodeStyle()
        {
            if (validationCts != null)
            {
                EditorUtility.DisplayDialog(
                    "Project Code Style Validator",
                    "Please, wait until the current validation is finished.",
                    "OK"
                );
                return;
            }

            validationCts?.Cancel();
            validationCts = new CancellationTokenSource();

            CheckProjectCodeStyleAsync(validationCts.Token);
        }

        private static async void CheckProjectCodeStyleAsync(CancellationToken ct)
        {
            try
            {
                await ProjectValidatorsRunner.CheckProject(ValidatorName, ct);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Project code style check cancelled.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Project Code Style Validator] Error: {e.Message}");
            }
            finally
            {
                validationCts?.Cancel();
                validationCts?.Dispose();
                validationCts = null;
            }
        }

        [MenuItem(ProjectValidatorsRunner.MenuItemStartPath + MenuItemValidateCodeStyle + "Fix code style issues from last check", false, 21)]
        private static void FixCodeStyleIssuesFromLastCheck()
        {
            if (!ValidationResultsSession.AllResults.TryGetValue(ValidatorName, out var codeStyleIssues))
            {
                EditorUtility.DisplayDialog(
                    "Fix code style issues from last check",
                    "No code style issues found from the last check.",
                    "OK"
                );
                return;
            }

            var result = EditorUtility.DisplayDialog(
                "Fix code style issues from last check",
                $"Files for fix: {codeStyleIssues.Count}",
                "Start",
                "Cancel"
            );

            if (!result)
            {
                return;
            }

            fixCts?.Cancel();
            fixCts = new CancellationTokenSource();

            var files = codeStyleIssues.Values.SelectMany(f => f).ToHashSet();
            _ = FixCodeStyleIssuesFromLastCheckAsync(files, fixCts.Token);
        }

        private static async Task FixCodeStyleIssuesFromLastCheckAsync(HashSet<FileWithValidationError> paths, CancellationToken ct)
        {
            var args = $"{string.Join(" ", paths.Select(f => f.Path))} --fix";

            var allResults = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<FileWithValidationError>>>(ValidationResultsSession.AllResults)
            {
                [ValidatorName] = new Dictionary<string, IReadOnlyCollection<FileWithValidationError>>()
            };
            ValidationResultsSession.SetResults(allResults);

            ValidationResultsWindow.StartLoading();

            _ = await ProjectValidatorsRunner.RunPythonValidatorManually(ValidatorName, args, false, ct);

            ValidationResultsWindow.StopLoading();
            ValidationResultsWindow.ShowResults();
        }

        [MenuItem(ProjectValidatorsRunner.MenuItemStartPath + MenuItemValidateCodeStyle + "Open code style settings (.editorconfig)", false, 100)]
        private static void OpenEditorConfig()
        {
            if (!File.Exists(PathToEditorConfig))
            {
                EditorUtility.DisplayDialog(
                    "Open EditorConfig",
                    $"File not found:\n{PathToEditorConfig}",
                    "OK"
                );
                return;
            }

            try
            {
                if (!InternalEditorUtility.OpenFileAtLineExternal(PathToEditorConfig, 1))
                {
                    EditorUtility.DisplayDialog(
                        "Open EditorConfig",
                        "Failed to open file in the current code editor.",
                        "OK"
                    );
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Project Code Style Validator] Failed to open .editorconfig: {e.Message}");
                EditorUtility.DisplayDialog("Open EditorConfig", "Unexpected error occurred.", "OK");
            }
        }

        [MenuItem(ProjectValidatorsRunner.MenuItemStartPath + MenuItemValidateCodeStyle + "Copy code style settings from template", false, 101)]
        private static void CopyEditorConfigFromTemplate()
        {
            try
            {
                File.Copy(PathToTemplate, PathToEditorConfig, overwrite: true);
                Debug.Log($".editorconfig copied from template to:\n{PathToEditorConfig}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Project Code Style Validator] Failed to copy .editorconfig from {PathToTemplate}: {e.Message}");
                EditorUtility.DisplayDialog(
                    "Copy EditorConfig",
                    "Failed to copy .editorconfig from template.",
                    "OK"
                );
            }
        }
    }
}
