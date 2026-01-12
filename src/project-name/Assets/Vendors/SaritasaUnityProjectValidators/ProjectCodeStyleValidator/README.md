# Unity Project Code Style Validator

A Unity Editor extension that validates C# code style using `dotnet format` and EditorConfig rules.

## Features

- **Automated Code Style Validation** - Check C# files against EditorConfig rules
- **Auto-fix Support** - Automatically fix code style issues
- **Caching** - Smart caching system to speed up repeated validations
- **Ignore Patterns** - Exclude specific files/folders from validation

## Usage

### Menu Items

Access the validator through Unity's menu: **Tools/Saritasa Validators/Code Style/**

#### Available Commands:

1. **Validate Project Code Style**
   - Analyzes C# files for code style violations
   - Shows results in the Validation Results window
   - Uses caching for faster subsequent checks

2. **Fix code style issues from last check**
   - Automatically fixes issues found in the last validation
   - Modifies files in place
   - Clears validation results after fixing

3. **Open code style settings (.editorconfig)**
   - Opens the `.editorconfig` file in your default code editor
   - Located at `Assets/.editorconfig`

4. **Copy code style settings from template**
   - Copies the template `.editorconfig` to your project
   - Overwrites existing file if present

### EditorConfig Setup

The validator uses `.editorconfig` for code style rules. A template is provided in:
```
Assets/Vendors/SaritasaUnityProjectValidators/ProjectCodeStyleValidator/editorconfig_template.txt
```

**To set up:**
1. Use menu: **Tools/Saritasa Validators/Code Style/Copy code style settings from template**
2. Or manually copy the template to `Assets/.editorconfig`
3. Customize rules as needed

### Ignore Patterns

Configure which files to skip validation by editing:
```
Assets/Vendors/SaritasaUnityProjectValidators/ProjectCodeStyleValidator/code_style_ignore_files.txt
```

**Example patterns:**
```
**/Editor/Npm/**
**/Vendors/**
**/ThirdParty/**
```

- Lines starting with `#` are comments
- Supports wildcards (`*`, `**`)
- Case-insensitive matching

## How It Works

1. **File Collection**: Gathers C# files from selected paths
2. **Filtering**: Applies ignore patterns from `code_style_ignore_files.txt`
3. **Project Grouping**: Groups files by Unity project root
4. **Cache Check**: Checks if files have changed since last validation
5. **Validation**: Runs `dotnet format` on changed files only
6. **Results**: Displays issues in the Validation Results window

### Caching System

- Calculates SHA256 hash of each file
- Stores validation results per file
- Only re-validates changed files
- Cache location: `Assets/Vendors/SaritasaUnityProjectValidators/ProjectCodeStyleValidator/.cache/`

## Code Style Rules

The template EditorConfig includes:

- **Formatting**: Consistent indentation, bracing, spacing
- **Naming Conventions**: PascalCase for constants, proper modifiers order
- **Modern C# Features**: File-scoped namespaces, pattern matching
- **Code Quality**: Enabled .NET analyzers with customized severity levels
- **Unity-specific**: Disabled rules that conflict with Unity conventions

### Components:

1. **ProjectCodeStyleValidator.cs** (C#)
   - Unity Editor integration
   - Menu items and UI
   - Validation orchestration

2. **code_style_validator.py** (Python)
   - Core validation logic
   - `dotnet format` execution
   - Caching implementation
   - Error parsing

3. **editorconfig_template.txt**
   - Default code style rules
   - Pre-configured for Unity projects

4. **code_style_ignore_files.txt**
   - File/folder exclusion patterns

