# Unity Project Structure Validator

## Overview

The Unity Project Structure Validator is a comprehensive tool that enforces file organization rules in Unity projects. It helps maintain consistent project structure by validating file placements according to configurable regex patterns. The tool integrates seamlessly with Unity Editor, command-line workflows, and CI/CD pipelines.

## Key Features

- **Unity Editor Integration**: Validate project structure directly from Unity menu with colored console output
- **Python CLI Validator**: Standalone validation for automation and CI/CD pipelines
- **GitHub Actions Support**: Automated PR validation with inline review comments
- **Configurable Rules**: Define custom placement rules using flexible regex patterns

## Components

### 1. Unity Editor Integration

**File**: `Editor/ProjectStructureValidator.cs`

C# script providing Unity Editor menu commands:

- **Validate project structure** (`Project Validators/Project Structure Validator/Validate project structure`)
  - Runs validation on entire Assets folder
  - Displays results in Unity Console with color-coded output
  - Automatically installs Python if missing

- **Create\Validate Git Action file** (`Project Validators/Project Structure Validator/Create\Validate Workflow File`)
  - Creates or validates GitHub Actions workflow
  - Copies template to `.github/workflows/`
  - Checks for differences and offers to update

### 2. Python Validator

**File**: `project_structure_validator.py`

Command-line tool that performs the actual validation:

**Features**:
- Directory-based validation (recursive scan)
- File-specific validation (explicit file list)
- Regex pattern matching with case-insensitive support
- Configurable ignore directories

**Usage**:
```bash
# Validate entire directory
python project_structure_validator.py --config project_structure_config.json --dir /path/to/Assets

# Validate specific files
python project_structure_validator.py --config project_structure_config.json --files file1.cs file2.unity
```

### 3. Configuration File

**File**: `project_structure_config.json`

JSON configuration defining validation rules:

**Structure**:
```json
{
  "rules": {
    ".cs": {
      "patterns": [
        {
          "pattern": "(?i)^scripts/.*\\.cs$",
          "comment": ".cs files must be in 'Scripts/' directory"
        }
      ]
    }
  },
  "ignore_dirs": ["Vendors", "Plugins"]
}
```

**Key Concepts**:
- **rules**: Dictionary keyed by file extension (e.g., `.cs`, `.unity`, `.shader`)
- **patterns**: Array of pattern objects for each extension
  - **pattern**: Regex pattern
  - **comment**: Human-readable description shown in error messages
- **ignore_dirs**: Array of directory names to skip during validation

### 4. GitHub Actions Workflow

**File**: `Workflow\project-structure-validator-runner.yaml`

Template workflow for continuous integration:

**Features**:
- Triggers on PR open, synchronize, and reopen events
- Validates only changed files for efficiency
- Posts review comments on files with violations
- Removes outdated comments automatically
- Marks resolved issues with reactions

**GitHub App reminder**

Please ask your DevOps team to add/install the GitHub App named "saritasa-unity-project-validator": https://github.com/apps/saritasa-unity-project-validator

This App is required so the workflow can post review comments on pull requests and manage them appropriately.

### 5. JavaScript Helper Scripts

**Files**: `Workflow\JS\post_validation_comments.js`, `Workflow\JS\remove_outdated_comments.js`

Node.js scripts for GitHub PR comment management:

- **post_validation_comments.js**: 
  - Parses validator output
  - Creates review comments for files with violations
  - Includes expected location patterns in comments

- **remove_outdated_comments.js**: 
  - Deletes comments for files in current changeset
  - Marks other validation comments as resolved (👍 reaction)

### 6. Python Installer Components

**Files**: `Packages/SaritasaUnityProjectValidators/Editor/PythonInstaller/*.cs`

Cross-platform Python installation support:

- **PythonInstallerBase.cs**: Abstract base class for installers
- **WindowsPythonInstaller.cs**: Automatic Python installation on Windows
- **MacPythonInstaller.cs**: Guided installation on macOS

## Quick Start

### Using Unity Editor

1. Open your Unity project
2. Navigate to menu: **Project Validators → Project Structure Validator → Validate project structure**
3. Check Unity Console for validation results:
   - Green message: All files comply
   - Yellow messages: Files with violations grouped by expected location

### Setting Up GitHub Actions

1. **In Unity Editor**: 
   - Go to: **Project Validators → Project Structure Validator → Create\Validate Workflow File**
   - This copies the workflow template to `.github/workflows/project-structure-validator-runner.yaml`
2. **Commit and push** the workflow file to your repository
3. **Add GitHub App to project**:
   - Ask  DevOps team to add the GitHub App named "saritasa-unity-project-validator" to project repository: https://github.com/apps/saritasa-unity-project-validator

## File Locations

```
Assets/Vendors/SaritasaUnityProjectValidators/ProjectStructureValidator/
├── README.md                                   # This documentation
├── project_structure_validator.py              # Python validation script
├── project_structure_config.json               # Configuration file
├── Editor/
│   └── ProjectStructureValidator.cs            # Unity Editor integration
└── Workflow/
    ├── project-structure-validator-runner.yaml # GitHub Actions template
    └── JS/
        ├── post_validation_comments.js         # PR comment posting
        └── remove_outdated_comments.js         # Comment cleanup
```

Related files (Unity packages):
```
Assets/Vendors/SaritasaUnityProjectValidators/Editor/PythonInstaller/
├── PythonInstallerBase.cs                      # Base installer class
├── WindowsPythonInstaller.cs                   # Windows installer
└── MacPythonInstaller.cs                       # macOS installer
```