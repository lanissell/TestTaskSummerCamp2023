#!/usr/bin/env python3
"""
Code Style Validator using dotnet format.
"""
import hashlib
import pickle
import sys
import json
import subprocess
import re
import time
from pathlib import Path
from typing import List, Dict, Any

# Constants
VALIDATOR_FOLDER = "Assets/Vendors/SaritasaUnityProjectValidators/ProjectCodeStyleValidator"
IGNORE_FILE_NAME = "code_style_ignore_files.txt"
CACHE_FILE_NAME = "format_cache.pkl"
TARGET_FRAMEWORK = "net4.7.1"

# Regex patterns
PATTERN_PROJECT_PATH = re.compile(r'^(.*[/\\]src[/\\][^/\\]+)', re.IGNORECASE)
PATTERN_FORMAT_OUTPUT = re.compile(
    r'^(.+?)\((\d+),\d+\):\s*(warning|error|info)\s+(\w+):\s*(.+)$',
    re.IGNORECASE
)
PATTERN_CSPROJ_BRACKETS = re.compile(r'\s*\[.+?\.csproj\]\s*')

def load_ignore_patterns(project_root: Path) -> List[re.Pattern]:
    """Load and compile ignore patterns from file."""
    ignore_file = project_root / VALIDATOR_FOLDER / IGNORE_FILE_NAME

    if not ignore_file.exists():
        return []

    patterns: List[re.Pattern] = []
    try:
        with open(ignore_file, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#'):
                    # Convert glob pattern to regex once
                    regex_pattern = (line.replace('\\', '/')
                                    .replace('.', r'\.')
                                    .replace('*', '.*'))
                    patterns.append(re.compile(regex_pattern, re.IGNORECASE))
    except Exception as e:
        print(f"Warning: Could not read ignore file: {e}", file=sys.stderr)

    return patterns

def should_ignore_file(file_path: Path, ignore_patterns: List[re.Pattern]) -> bool:
    """Check if file should be ignored based on compiled patterns."""
    if not ignore_patterns:
        return False

    file_str = str(file_path.resolve()).replace('\\', '/')
    return any(pattern.search(file_str) for pattern in ignore_patterns)

def extract_project_path(file_path: Path) -> Path | None:
    """Extract project path from file path (up to */src/project-name)."""
    match = PATTERN_PROJECT_PATH.search(str(file_path.resolve()))
    return Path(match.group(1)) if match else None

def check_dotnet_installed() -> bool:
    """Check if dotnet CLI is installed."""
    try:
        result = subprocess.run(
            ["dotnet", "--version"],
            capture_output=True,
            text=True,
            encoding='utf-8',
            errors='replace'
        )
        return result.returncode == 0
    except (subprocess.TimeoutExpired, FileNotFoundError, Exception):
        return False

def create_temporary_csproj(cs_files: List[Path], project_root: Path) -> Path | None:
    """Create a temporary .csproj for a single project including given C# files."""
    csproj_path = project_root / "temp_project.csproj"

    # Build content parts
    header = f'''<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>{TARGET_FRAMEWORK}</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>'''

    compile_items = '\n'.join(
        f'    <Compile Include="{cs.resolve().as_posix()}" />'
        for cs in cs_files
    )

    # Add analyzers if present
    analyzers_path = project_root / VALIDATOR_FOLDER / "Analyzers"
    analyzer_items = ''
    if analyzers_path.exists() and analyzers_path.is_dir():
        analyzer_files = list(analyzers_path.glob("*.dll"))
        if analyzer_files:
            analyzer_items = '\n  </ItemGroup>\n  <ItemGroup>\n' + '\n'.join(
                f'    <Analyzer Include="{analyzer.resolve().as_posix()}" />'
                for analyzer in analyzer_files
            )

    footer = '\n  </ItemGroup>\n</Project>'

    try:
        csproj_path.write_text(
            header + '\n' + compile_items + analyzer_items + footer,
            encoding="utf-8"
        )
        return csproj_path
    except Exception as e:
        print(f"Error creating csproj: {e}", file=sys.stderr)
        return None

def parse_dotnet_format_output(output: str) -> List[Dict[str, Any]]:
    """Parse dotnet format output into list of error dictionaries."""
    if not output or not output.strip():
        return []

    errors: List[Dict[str, Any]] = []
    for line in output.splitlines():
        line = line.strip()
        if not line:
            continue

        match = PATTERN_FORMAT_OUTPUT.match(line)
        if match:
            file_path, line_num, severity, code, message = match.groups()
            message = PATTERN_CSPROJ_BRACKETS.sub('', message).strip()

            errors.append({
                "file": file_path.strip().replace('\\', '/'),
                "comment": f"[{severity.lower()}] {code}: {message}",
                "line": int(line_num)
            })

    return errors

def get_cache_path(project_root: Path) -> Path:
    """Get cache file path for the project."""
    cache_dir = project_root / VALIDATOR_FOLDER / ".cache"
    cache_dir.mkdir(parents=True, exist_ok=True)
    return cache_dir / CACHE_FILE_NAME

def calculate_file_hash(file_path: Path) -> str:
    """Calculate SHA256 hash of file content."""
    try:
        with open(file_path, 'rb') as f:
            return hashlib.sha256(f.read()).hexdigest()
    except Exception:
        return ""

def load_cache(project_root: Path) -> Dict[str, Dict[str, Any]]:
    """Load cache from disk."""
    cache_path = get_cache_path(project_root)
    if not cache_path.exists():
        return {}
    try:
        with open(cache_path, 'rb') as f:
            return pickle.load(f)
    except Exception:
        return {}

def save_cache(project_root: Path, cache: Dict[str, Dict[str, Any]]) -> None:
    """Save cache to disk."""
    try:
        with open(get_cache_path(project_root), 'wb') as f:
            pickle.dump(cache, f)
    except Exception:
        pass

def run_dotnet_format_analyze(
    csproj_path: Path,
    cs_files: List[Path],
    verify_no_changes: bool,
    project_root: Path
) -> List[Dict[str, Any]]:
    """Run dotnet format and return code style issues with caching."""
    cache = load_cache(project_root)
    all_errors: List[Dict[str, Any]] = []
    files_to_check: List[Path] = []

    # Check cache for each file
    for cs_file in cs_files:
        file_key = str(cs_file.resolve())
        file_hash = calculate_file_hash(cs_file)

        cached = cache.get(file_key, {})
        if cached.get('hash') == file_hash:
            # Use cached errors
            all_errors.extend(cached.get('errors', []))
        else:
            # File changed or not in cache
            files_to_check.append(cs_file)

    # If all files were cached, return cached results
    if not files_to_check:
        return all_errors

    # Run format for changed files
    start_time = time.perf_counter()
    cmd = [
        "dotnet", "format", str(csproj_path),
        "--verbosity", "q",
        "--severity", "info"
    ]
    if verify_no_changes:
        cmd.append("--verify-no-changes")

    try:
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            check=False,
            encoding='utf-8',
            creationflags=subprocess.CREATE_NO_WINDOW if sys.platform == 'win32' else 0
        )

        combined_output = f"{result.stdout or ''}\n{result.stderr or ''}"
        elapsed_time = time.perf_counter() - start_time

        new_errors = parse_dotnet_format_output(combined_output)

        # Update cache for checked files
        for cs_file in files_to_check:
            file_key = str(cs_file.resolve())
            file_errors = [
                e for e in new_errors
                if Path(e["file"]).resolve() == cs_file.resolve()
            ]
            cache[file_key] = {
                'hash': calculate_file_hash(cs_file),
                'errors': file_errors
            }

        save_cache(project_root, cache)

        all_errors.extend(new_errors)
        all_errors.insert(0, {"file": "", "comment": f"Run validator {elapsed_time:.2f} seconds", "line": 0})

        return all_errors

    except Exception as e:
        all_errors.append({"file": "", "comment": f"dotnet format error: {str(e)}", "line": 0})
        return all_errors

def collect_and_group_files_by_project(paths: List[str]) -> Dict[Path, List[Path]]:
    """Collect .cs files from paths and group by project root, filtering ignored files."""
    project_groups: Dict[Path, List[Path]] = {}
    ignore_cache: Dict[Path, List[re.Pattern]] = {}

    for path_str in paths:
        path = Path(path_str)
        if not path.exists():
            continue

        # Get all CS files
        cs_files = [path] if path.is_file() and path.suffix.lower() == ".cs" else list(path.rglob("*.cs"))

        for file_path in cs_files:
            proj = extract_project_path(file_path)
            if proj is None:
                continue

            # Load ignore patterns once per project
            if proj not in ignore_cache:
                ignore_cache[proj] = load_ignore_patterns(proj)

            if not should_ignore_file(file_path, ignore_cache[proj]):
                project_groups.setdefault(proj, []).append(file_path)


    return project_groups

def process_project_groups(
    project_groups: Dict[Path, List[Path]],
    verify_no_changes: bool
) -> List[Dict[str, Any]]:
    """Process each project group and collect all errors."""
    all_errors: List[Dict[str, Any]] = []

    for project_root, filtered_files in project_groups.items():
        temp_csproj = create_temporary_csproj(filtered_files, project_root)
        if temp_csproj is None:
            continue

        try:
            errors = run_dotnet_format_analyze(
                temp_csproj,
                filtered_files,
                verify_no_changes,
                project_root
            )
            all_errors.extend(errors)
        finally:
            temp_csproj.unlink(missing_ok=True)

    return all_errors

def main():
    # Ensure dependencies are installed
    if not check_dotnet_installed():
        print(json.dumps(
            [{"file": "", "comment": "Failed to find dotnet CLI. Please install .NET SDK.", "line": 0}],
            ensure_ascii=False
        ))
        sys.exit(0)

    # Parse arguments
    verify_no_changes = "--fix" not in [a.lower() for a in sys.argv[1:]]
    paths = [a for a in sys.argv[1:] if a.lower() != "--fix"]

    if not paths:
        print(json.dumps([], ensure_ascii=False))
        sys.exit(0)

    # Collect and group files by project root
    project_groups = collect_and_group_files_by_project(paths)

    if not project_groups:
        print(json.dumps([], ensure_ascii=False))
        sys.exit(0)

    # Process each project group
    all_errors = process_project_groups(project_groups, verify_no_changes)

    print(json.dumps(all_errors, ensure_ascii=False, indent=2))
    sys.exit(0)

if __name__ == "__main__":
    main()
