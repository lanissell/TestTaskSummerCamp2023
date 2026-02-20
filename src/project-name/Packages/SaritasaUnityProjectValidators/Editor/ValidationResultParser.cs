using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Parser for validation results from validators launcher.
/// </summary>
public static class ValidationResultParser
{
    [Serializable]
    private class ValidationError
    {
        [SerializeField]
        private string file;

        /// <summary>
        /// Path to the file with validation error.
        /// </summary>
        public string File => file;

        [SerializeField]
        private string comment;

        /// <summary>
        /// Comment describing the validation error.
        /// </summary>
        public string Comment => comment;

        [SerializeField]
        private string line;

        /// <summary>
        /// Line number where the validation error occurred.
        /// </summary>
        public string Line => line;
    }

    [Serializable]
    private class ValidationResult
    {
        /// <summary>
        /// Array of validation errors.
        /// </summary>
        public ValidationError[] errors;
    }

    /// <summary>
    /// Parse validation errors for specific validator from launcher JSON output.
    /// </summary>
    /// <param name="jsonOutput">JSON output from validators_launcher.py</param>
    /// <param name="validatorName">Name of the validator to extract results for</param>
    /// <returns>Dictionary where key is comment and value is list of file paths</returns>
    public static Dictionary<string, List<string>> ParseValidatorErrors(string jsonOutput, string validatorName)
    {
        var result = new Dictionary<string, List<string>>();

        try
        {
            // Extract validator-specific array using regex
            string pattern = $"\"{validatorName}\"\\s*:\\s*(\\[[^\\]]*(?:\\{{[^}}]*\\}}[^\\]]*)*\\])";
            var match = Regex.Match(jsonOutput, pattern, RegexOptions.Singleline);

            if (!match.Success || match.Groups.Count < 2)
            {
                return result;
            }

            // Wrap array in an object for JsonUtility
            string validatorJson = $"{{\"errors\":{match.Groups[1].Value}}}";
            var validationResult = JsonUtility.FromJson<ValidationResult>(validatorJson);

            if (validationResult?.errors != null)
            {
                foreach(var error in validationResult.errors)
                {
                    if (string.IsNullOrEmpty(error.File) || string.IsNullOrEmpty(error.Comment))
                    {
                        continue;
                    }

                    if (!result.ContainsKey(error.Comment))
                    {
                        result[error.Comment] = new List<string>();
                    }

                    int assetsIndex = error.File.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                    string trimmedPath = assetsIndex >= 0 ? error.File.Substring(assetsIndex) : error.File;

                    var pathMessage = string.IsNullOrWhiteSpace(error.Line) ? trimmedPath : $"Line {error.Line} in {trimmedPath}";
                    result[error.Comment].Add(pathMessage);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse validation JSON: {ex.Message}");
            return result;
        }
    }
}