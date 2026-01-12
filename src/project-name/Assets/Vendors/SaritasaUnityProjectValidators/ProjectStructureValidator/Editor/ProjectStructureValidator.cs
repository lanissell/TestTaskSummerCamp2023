using System;
using System.Threading;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace SaritasaUnityProjectValidators.Editor
{
    /// <summary>
    /// Provides tools for validating the Unity project structure using an external Python script,
    /// and for generating or validating a GitHub Action workflow file for continuous validation.
    /// </summary>
    public static class ProjectStructureValidator
    {
        private const string ValidatorName = "project_structure_validator";
        private const string ValidatorFolder = "ProjectStructureValidator";

        private static CancellationTokenSource cts;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            ProjectValidatorsRunner.RegisterValidatorByName(ValidatorName, ValidatorFolder);
        }

        [MenuItem(ProjectValidatorsRunner.MenuItemStartPath + "Validate Project Structure", priority = 22)]
        private static void CheckProjectStructure()
        {
            if (cts != null)
            {
                EditorUtility.DisplayDialog(
                    "Project Structure Validator",
                    "Please, wait until the current validation is finished.",
                    "OK");
                return;
            }

            cts?.Cancel();
            cts = new CancellationTokenSource();

            CheckProjectStructureAsync(cts.Token);
        }

        private static async void CheckProjectStructureAsync(CancellationToken ct)
        {
            try
            {
                await ProjectValidatorsRunner.CheckProject(ValidatorName, ct);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Project structure check cancelled.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Project Structure Checker] Error: {e.Message}");
            }
            finally
            {
                cts?.Cancel();
                cts?.Dispose();
                cts = null;
            }
        }
    }
}
