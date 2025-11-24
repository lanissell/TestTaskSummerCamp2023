using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

/// <summary>
/// Windows-specific Python installer.
/// </summary>
public class WindowsPythonInstaller : PythonInstallerBase
{
    /// <inheritdoc/>
    public WindowsPythonInstaller(string version) : base(version) { }

    /// <inheritdoc/>
    public override string PythonExecutableName => "py";

    /// <inheritdoc/>
    protected override async Task<bool> InstallPythonAsyncInternal(CancellationToken ct, Action<float, string> progressCallback = null)
    {
        bool installPython = EditorUtility.DisplayDialog(
            "Python Required",
            "Python is required for project structure checking. Would you like to install it?",
            "Yes",
            "No"
        );

        if (!installPython)
        {
            return false;
        }

        progressCallback?.Invoke(0.3f, "Downloading installer...");

        string installerPath = Path.Combine(Path.GetTempPath(), "python_installer.exe");

        using (var client = new System.Net.WebClient())
        {
            await client.DownloadFileTaskAsync($"https://www.python.org/ftp/python/{PythonVersion}/python-{PythonVersion}-amd64.exe", installerPath);
        }

        progressCallback?.Invoke(0.6f, "Installing Python...");

        using (var process = new Process())
        {
            process.StartInfo.FileName = installerPath;
            process.StartInfo.Arguments = "/quiet InstallAllUsers=1 PrependPath=1";
            process.StartInfo.Verb = "runas"; // Request admin privileges.
            process.StartInfo.UseShellExecute = true;

            try
            {
                process.Start();
                await Task.Run(() => process.WaitForExit(), ct);

                if (File.Exists(installerPath))
                {
                    File.Delete(installerPath);
                }

                if (process.ExitCode != 0)
                {
                    EditorUtility.DisplayDialog(
                        "Installation Failed",
                        "Python installation failed. Please install it manually from python.org",
                        "OK"
                    );
                    return false;
                }

                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                throw new Exception("Administrator privileges are required to install Python.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to install Python: {ex.Message}");
            }
        }
    }
}