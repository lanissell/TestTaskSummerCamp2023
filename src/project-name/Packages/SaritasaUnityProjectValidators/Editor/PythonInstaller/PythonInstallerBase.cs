using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;

/// <summary>
/// Base Python installer.
/// </summary>
public abstract class PythonInstallerBase
{
    /// <summary>
    /// Returns python execution name.
    /// </summary>
    public abstract string PythonExecutableName { get; }

    /// <summary>
    /// Returns python version.
    /// </summary>
    protected string PythonVersion { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="version">Python version</param>
    protected PythonInstallerBase(string version)
    {
        PythonVersion = version;
    }

    /// <summary>
    /// Returns true if python installed.
    /// </summary>
    /// <returns></returns>
    public bool IsPythonInstalled()
    {
        try
        {
            using var process = new Process();
            process.StartInfo.FileName = PythonExecutableName;
            process.StartInfo.Arguments = "--version";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Install python async.
    /// </summary>
    public async Task InstallPythonAsync(CancellationToken ct)
    {
        try
        {
            EditorUtility.DisplayProgressBar("Installing Python", "Downloading installer...", 0.3f);

            bool success = await InstallPythonAsyncInternal(ct, (progress, message) => { EditorUtility.DisplayProgressBar("Installing Python", message, progress); });

            EditorUtility.ClearProgressBar();

            if (!success)
            {
                return;
            }

            EditorUtility.DisplayDialog(
                "Installation Complete",
                "Python has been installed successfully.",
                "OK"
            );
        }
        catch (OperationCanceledException)
        {
            EditorUtility.ClearProgressBar();
            Debug.Log("Python installation cancelled");
        }
        catch (Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(
                "Installation Error",
                $"Failed to install Python: {e.Message}. Please install it manually from python.org",
                "OK"
            );
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    /// <summary>
    /// Internal python installation logic for specific platform.
    /// </summary>
    protected abstract Task<bool> InstallPythonAsyncInternal(CancellationToken ct, Action<float, string> progressCallback = null);
}