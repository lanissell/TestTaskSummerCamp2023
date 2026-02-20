#!/usr/bin/env python3
"""
Project Structure Validator.
Validates file structure according to configured rules and patterns.
"""
import re
import json
import argparse
import sys
from pathlib import Path
from typing import Dict, List, Optional, Tuple

# Cache for compiled rules by project path
_compiled_rules_cache = {}

PROJECT_PATH_PATTERN = r'^(.*[/\\]src[/\\][^/\\]+)'
CONFIG_FILENAME = "project_structure_config.json"


def extract_project_path(file_path: Path) -> Optional[Path]:
    """Extract project path from file path (up to */src/project-name)."""
    path_str = str(file_path.resolve())
    match = re.search(PROJECT_PATH_PATTERN, path_str, re.IGNORECASE)
    return Path(match.group(1)) if match else None


def load_config(config_path: Path) -> Optional[dict]:
    """Load and parse JSON config file."""
    try:
        with open(config_path, 'r', encoding='utf-8-sig') as f:
            return json.load(f)
    except Exception as e:
        print(f"[ERROR] Failed to read {config_path.name}: {e}", file=sys.stderr)
        return None


def compile_rules(rules: dict) -> Dict[str, List[dict]]:
    """
    Compile regex patterns for all rules.

    Args:
        rules: Dictionary of rules by file extension.

    Returns:
        dict: Compiled patterns with comments by extension.
    """
    compiled = {}
    for ext, rule in rules.items():
        compiled[ext] = [
            {
                "pattern": re.compile(p["pattern"], re.IGNORECASE),
                "comment": p.get("comment", "No comment")
            }
            for p in rule["patterns"]
        ]
    return compiled


def get_compiled_rules_for_project(
    project_path: Path,
    cache: dict
) -> Tuple[Optional[dict], Optional[list]]:
    """Get compiled rules for project with caching."""
    project_key = str(project_path)

    if project_key in cache:
        return cache[project_key]

    config_path = next(project_path.rglob(CONFIG_FILENAME), None)
    if not config_path:
        print(f"[WARNING] {CONFIG_FILENAME} not found for project: {project_path}", file=sys.stderr)
        return None, None

    config = load_config(config_path)
    if not config:
        return None, None

    compiled_rules = compile_rules(config["rules"])
    ignore_dirs = config["ignore_dirs"]

    cache[project_key] = (compiled_rules, ignore_dirs)
    return cache[project_key]


def should_ignore(path: Path, ignore_dirs: List[str]) -> bool:
    """Return True if path should be ignored."""
    path_str = path.as_posix().lower()
    return any(ignored.lower() in path_str for ignored in ignore_dirs)


def get_relative_path(path_obj: Path, root_dir: Optional[Path]) -> str:
    """Get relative path from root directory."""
    try:
        base = root_dir or Path.cwd()
        return path_obj.relative_to(base).as_posix()
    except ValueError:
        return path_obj.as_posix()


def collect_files_from_paths(paths: List[str]) -> List[Path]:
    """
    Collect all files from list of paths (files or directories).

    Args:
        paths: List of file or directory paths.

    Returns:
        list: List of Path objects for all files.
    """
    all_files = []

    for path_str in paths:
        path = Path(path_str)

        if not path.exists():
            continue

        if path.is_file():
            all_files.append(path)
        elif path.is_dir():
            all_files.extend(f for f in path.rglob("*") if f.is_file())

    return all_files


def check_file(
    file_path: Path,
    compiled_rules: dict,
    ignore_dirs: List[str],
    root_dir: Optional[Path]
) -> Optional[dict]:
    """
    Validate single file according to rules.

    Args:
        file_path: Path object of the file to validate.
        compiled_rules: Dictionary of compiled rules by file extension.
        ignore_dirs: List of directories to ignore.
        root_dir: Root directory for relative path calculation.

    Returns:
        dict: Dictionary with error information or None if valid.
    """
    if should_ignore(file_path, ignore_dirs):
        return None

    ext = file_path.suffix.lower()
    if ext not in compiled_rules:
        return None

    rel_path = get_relative_path(file_path, root_dir)
    rules = compiled_rules[ext]

    # Check if path matches any pattern
    if any(rule["pattern"].fullmatch(rel_path) for rule in rules):
        return None

    return {
        "file": str(file_path).replace('\\', '/'),
        "comment": " OR ".join(rule["comment"] for rule in rules)
    }


def validate_files(files: List[Path], cache: dict) -> List[dict]:
    """
    Validate multiple files and return errors.

    Args:
        files: List of file paths to validate.
        cache: Cache dictionary for compiled rules.

    Returns:
        list: List of validation errors.
    """
    errors = []
    files_by_project = {}

    # Group files by project
    for file_path in files:
        project_path = extract_project_path(file_path)
        if not project_path:
            continue

        if project_path not in files_by_project:
            files_by_project[project_path] = []
        files_by_project[project_path].append(file_path)

    # Validate each project's files
    for project_path, project_files in files_by_project.items():
        compiled_rules, ignore_dirs = get_compiled_rules_for_project(project_path, cache)

        if not compiled_rules:
            continue

        for file_path in project_files:
            error = check_file(file_path, compiled_rules, ignore_dirs, project_path)
            if error:
                errors.append(error)

    return errors


def main():
    """Main entry point for the validator."""
    parser = argparse.ArgumentParser(description="Validate project file structure")
    parser.add_argument("paths", nargs='+', help="List of files or directories to validate")
    args = parser.parse_args()

    all_files = collect_files_from_paths(args.paths)

    if not all_files:
        print(json.dumps([], indent=2, ensure_ascii=False))
        sys.exit(0)

    errors = validate_files(all_files, _compiled_rules_cache)

    print(json.dumps(errors, indent=2, ensure_ascii=False))
    sys.exit(1 if errors else 0)


if __name__ == "__main__":
    main()