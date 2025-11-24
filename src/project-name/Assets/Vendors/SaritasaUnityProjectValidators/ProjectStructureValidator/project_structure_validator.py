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

# Cache for compiled rules by project path
_compiled_rules_cache = {}

def extract_project_path(file_path):
    """
    Extract project path from file path (up to */src/project-name).

    Args:
        file_path: Path object of the file.

    Returns:
        Path: Project root path or None if pattern not found.
    """
    path_str = str(file_path.resolve())

    # Match path up to and including folder after /src/
    # Supports both forward and backward slashes
    pattern = r'^(.*[/\\]src[/\\][^/\\]+)'
    match = re.search(pattern, path_str, re.IGNORECASE)

    if match:
        return Path(match.group(1))

    return None

def compile_rules(rules):
    """
    Compile regex patterns for all rules.

    Args:
        rules: Dictionary of rules by file extension.

    Returns:
        dict: Compiled patterns with comments by extension.
    """
    return {
        ext: [
            {
                "pattern": re.compile(p["pattern"], re.IGNORECASE),
                "comment": p.get("comment", "No comment")
            }
            for p in rule["patterns"]
        ]
        for ext, rule in rules.items()
    }


def get_compiled_rules_for_project(project_path):
    """
    Get compiled rules for project with caching.

    Args:
        project_path: Path to the project root.

    Returns:
        tuple: (compiled_rules, ignore_dirs) or (None, None) if not found.
    """
    project_key = str(project_path)

    if project_key in _compiled_rules_cache:
        return _compiled_rules_cache[project_key]

    config_path = next(project_path.rglob("project_structure_config.json"), None)

    if config_path is None:
        print(f"[WARNING] Config not found for project: {project_path}", file=sys.stderr)
        return None, None

    try:
        with open(config_path, 'r', encoding='utf-8') as f:
            config = json.load(f)
    except Exception as e:
        print(f"[ERROR] Failed to read config: {e}", file=sys.stderr)
        return None, None

    compiled_rules = compile_rules(config["rules"])
    ignore_dirs = config["ignore_dirs"]

    _compiled_rules_cache[project_key] = (compiled_rules, ignore_dirs)

    return _compiled_rules_cache[project_key]


def should_ignore(path, ignore_dirs):
    """
    Return True if path should be ignored.

    Args:
        path: Path object to check.
        ignore_dirs: List of directory patterns to ignore.

    Returns:
        bool: True if path should be ignored.
    """
    path_str = path.as_posix().lower()
    return any(ignored.lower() in path_str for ignored in ignore_dirs)


def get_relative_path(path_obj, root_dir):
    """
    Get relative path from root directory.

    Args:
        path_obj: Path object to convert.
        root_dir: Root directory for relative path calculation.

    Returns:
        str: Relative path as POSIX string.
    """
    try:
        base = root_dir if root_dir else Path.cwd()
        return path_obj.relative_to(base).as_posix()
    except ValueError:
        return path_obj.as_posix()

def check_file(file_path, compiled_rules, ignore_dirs, root_dir):
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
    # Skip non-files and ignored directories.
    if not file_path.is_file() or should_ignore(file_path, ignore_dirs):
        return None

    ext = file_path.suffix.lower()

    # Skip extensions without rules.
    if ext not in compiled_rules:
        return None

    rel_path = get_relative_path(file_path, root_dir)

    # Check if path matches any pattern.
    if not any(p["pattern"].fullmatch(rel_path) for p in compiled_rules[ext]):
        return {
            "file": str(file_path).replace('\\', '/'),
            "comment": " OR ".join(p["comment"] for p in compiled_rules[ext])
        }

    return None


def main():
    """Main entry point for the validator."""
    parser = argparse.ArgumentParser(description="Validate project file structure")
    parser.add_argument("files", nargs='+', help="List of files to validate")
    args = parser.parse_args()

    all_errors = []

    for file_str in args.files:
        file_path = Path(file_str)

        if not file_path.exists():
            continue

        project_path = extract_project_path(file_path)

        if not project_path:
            continue

        compiled_rules, ignore_dirs = get_compiled_rules_for_project(project_path)

        if compiled_rules is None:
            continue

        error = check_file(
            file_path,
            compiled_rules,
            ignore_dirs,
            root_dir=project_path
        )

        if error:
            all_errors.append(error)

    # Always output JSON
    print(json.dumps(all_errors, indent=2, ensure_ascii=False))

    if all_errors:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()