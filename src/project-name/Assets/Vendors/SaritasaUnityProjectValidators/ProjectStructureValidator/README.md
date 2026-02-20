# Project Structure Validator

This validator checks the file and folder structure of your Unity project according to customizable rules.

## How It Works

- The validator reads rules from `project_structure_config.json`.
- Each rule defines allowed file patterns and directory placement for specific file extensions.
- The validator can be run from the Unity Editor or via command line.

## Configuration

- Edit `project_structure_config.json` to define your own rules and ignored directories.

**Example `project_structure_config.json`:**
```json
{
  "rules": {
    ".cs": {
      "patterns": [
        {
          "pattern": "(?i)^(?:.*/)?scripts/.*\\.cs$",
          "comment": ".cs files must be in 'Scripts/' directory"
        },
        {
          "pattern": "(?i)^(?:.*/)?editor/.*\\.cs$",
          "comment": "in 'Editor/' directory"
        }
      ]
    },
    ".shader": {
      "patterns": [
        {
          "pattern": "(?i)^(?:.*/)?shaders/.*\\.shader$",
          "comment": ".shader files must be in 'Shaders/' directory"
        }
      ]
    }
  },
  "ignore_dirs": [
    "Vendors",
    "UserWorkspace",
    "Plugins",
    "TextMesh Pro",
    "Packages"
  ]
}
```
