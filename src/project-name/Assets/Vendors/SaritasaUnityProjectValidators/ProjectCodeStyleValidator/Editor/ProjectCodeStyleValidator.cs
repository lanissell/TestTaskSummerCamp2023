using System;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class ProjectCodeStyleValidator : MonoBehaviour
{
    private const string ValidatorName = "code_style_validator";
    private const string ValidatorFolder = "ProjectCodeStyleValidator";

    private static CancellationTokenSource cts;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        ProjectValidatorsRunner.RegisterValidatorByName(ValidatorName, ValidatorFolder);
    }

    [MenuItem(ProjectValidatorsRunner.MenuItemStartPath + "Validate Project Code Style")]
    private static void CheckProjectCodeStyl()
    {
        if (cts != null)
        {
            EditorUtility.DisplayDialog(
                "Project Code Style Validator",
                "Please, wait until the current validation is finished.",
                "OK"
            );
            return;
        }

        cts?.Cancel();
        cts = new CancellationTokenSource();

        CheckProjectCodeStyleAsync(cts.Token);
    }

    private static async void CheckProjectCodeStyleAsync(CancellationToken ct)
    {
        try
        {
            await ProjectValidatorsRunner.CheckProjectAsync(ValidatorName, ct);
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
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}
