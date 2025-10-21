using System.Collections.Generic;

/// <summary>
/// Data models for NPM pacjage json files.
/// </summary>
public class NPMJsonDataModels
{
    public class NPMPackageDataModel
    {
        public string displayName { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string unity { get; set; }
        public string description { get; set; }
        public string[] keywords { get; set; }
        public Sample[] samples { get; set; }
        public Dictionary<string, string> dependencies { get; set; }
    }

    public class Sample
    {
        public string displayName { get; set; }
        public string description { get; set; }
        public string path { get; set; }
    }
}