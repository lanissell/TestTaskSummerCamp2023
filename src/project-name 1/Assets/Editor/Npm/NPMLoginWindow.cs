using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor window to prepare log in for NPM nexus repository.
/// </summary>
public class NPMLoginWindow : EditorWindow
{
    private string username = string.Empty;
    private string password = string.Empty;
    private string email = "unity@saritasa.com"; // Used by default but can be overridden by user.
    
    public static void ShowWindow()
    {
        var window = GetWindow<NPMLoginWindow>(true, "Login to Npm registry");
        window.minSize = window.maxSize = new Vector2(350, 130);
        window.Show();
    }
    
    private void OnGUI()
    {
        username = EditorGUILayout.TextField("Login", username);
        password = EditorGUILayout.PasswordField("Password", password);
        email = EditorGUILayout.TextField("Email", email);
        EditorGUILayout.HelpBox("This will create .npmrc file in the user folder. This file will be used while uploading package to NPM server.", MessageType.Info);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Create local credentials"))
        {
            NPMUtils.Login(username, password, email);
            Close();
        }
    }
}