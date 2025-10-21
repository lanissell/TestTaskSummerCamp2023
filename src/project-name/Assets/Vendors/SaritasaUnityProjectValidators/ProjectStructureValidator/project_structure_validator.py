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


def load_config(config_path):
    """
    Load simplified JSON config with 'patterns' format everywhere.

    Args:
        config_path: Path to the JSON configuration file.

    Returns:
        dict: Loaded configuration with rules and ignore directories.
    """
    try:
        with open(config_path, 'r', encoding='utf-8') as f:
            config = json.load(f)
    except Exception as e:
        print(f"[ERROR] Failed to read config: {e}")
        sys.exit(1)

    return config

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

def matches_any_pattern(rel_path, patterns):
    """
    Check if path matches any of the patterns.

    Args:
        rel_path: Relative path string.
        patterns: List of compiled pattern dictionaries.

    Returns:
        bool: True if path matches any pattern.
    """
    return any(p["pattern"].fullmatch(rel_path) for p in patterns)

def check_structure(files, rules, ignore_dirs, root_dir=None):
    """
    Validate file structure according to rules.

    Args:
        files: List of file paths to validate.
        rules: Dictionary of rules by file extension.
        ignore_dirs: List of directories to ignore.
        root_dir: Root directory for relative path calculation.

    Returns:
        dict: Dictionary of errors grouped by file extension.
    """
    errors = {}
    compiled = compile_rules(rules)

    for file_path in files:
        path_obj = Path(file_path)

        # Skip non-files and ignored directories.
        if not path_obj.is_file() or should_ignore(path_obj, ignore_dirs):
            continue

        ext = path_obj.suffix.lower()

        # Skip extensions without rules.
        if ext not in compiled:
            continue

        rel_path = get_relative_path(path_obj, root_dir)

        # Check if path matches any pattern.
        if not matches_any_pattern(rel_path, compiled[ext]):
            errors.setdefault(ext, []).append({
                "path": rel_path,
                "expected": " OR ".join(p["comment"] for p in compiled[ext])
            })

    return errors

def print_pretty(errors):
    """
    Print formatted output.

    Args:
        errors: Dictionary of errors grouped by file extension.
    """
    if not errors:
        print("All files comply with the rules")
        return

    # Print errors grouped by expected location.
    for files in errors.values():
        print(f"{files[0]['expected']}\n")
        for f in files:
            print(f"    {f['path']}")

def main():
    """Main entry point for the validator."""
    parser = argparse.ArgumentParser(description="Validate project file structure")

    # Mutually exclusive: validate directory or specific files.
    group = parser.add_mutually_exclusive_group(required=True)
    group.add_argument("--dir", help="Root directory to validate")
    group.add_argument("--files", nargs='+', help="List of files to validate")
    parser.add_argument("--config", required=True, help="Path to JSON config")
    args = parser.parse_args()

    config = load_config(args.config)

    # Determine files to check based on input.
    if args.dir:
        files_to_check = list(Path(args.dir).rglob("*"))
        root_dir = Path(args.dir)
    else:
        files_to_check = [Path(f) for f in args.files]
        root_dir = None

    # Run validation.
    errors = check_structure(files_to_check, config["rules"], config["ignore_dirs"], root_dir=root_dir)
    print_pretty(errors)

    # Exit with error code if validation failed.
    sys.exit(0 if not errors else 1)


if __name__ == "__main__":
    main()