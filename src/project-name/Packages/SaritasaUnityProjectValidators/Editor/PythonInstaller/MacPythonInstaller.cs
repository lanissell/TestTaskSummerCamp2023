using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MacOS-specific Python installer
/// </summary>
public class MacPythonInstaller : PythonInstallerBase
{
    /// <inheritdoc/>
    public MacPythonInstaller(string version) : base(version) { }

    /// <inheritdoc/>
    public override string PythonExecutableName => "python3";

    /// <inheritdoc/>
    protected override Task<bool> InstallPythonAsyncInternal(CancellationToken ct, Action<float, string> progressCallback = null)
    {
        bool installPython = EditorUtility.DisplayDialog(
            "Python Required",
            "Python is required for project structure checking. Would you like to install it?",
            "Yes",
            "No"
        );

        if (installPython)
        {
            Application.OpenURL("https://www.python.org/");
        }

        return Task.FromResult(false);
    }
}