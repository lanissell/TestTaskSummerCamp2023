#!/usr/bin/env python3
"""
Code Style Validator.
Validates code style according to configured regex rules and patterns.
"""
import re
import json
import argparse
import sys
from pathlib import Path

# Cache for compiled rules by project path
_compiled_rules_cache = {}

def extract_project_path(file_path):
    """Extract project path from file path (up to */src/project-name)."""
    path_str = str(file_path.resolve())
    pattern = r'^(.*[/\\]src[/\\][^/\\]+)'
    match = re.search(pattern, path_str, re.IGNORECASE)
    if match:
        return Path(match.group(1))
    return None

def get_compiled_rules_for_project(project_path, config_filename, compile_rules_func, cache):
    """Get compiled rules for project with caching."""
    project_key = str(project_path)
    if project_key in cache:
        return cache[project_key]

    config_path = next(project_path.rglob(config_filename), None)
    if config_path is None:
        print(f"[WARNING] {config_filename} not found for project: {project_path}", file=sys.stderr)
        return None, None

    try:
        with open(config_path, 'r', encoding='utf-8-sig') as f:
            config = json.load(f)
    except Exception as e:
        print(f"[ERROR] Failed to read {config_filename}: {e}", file=sys.stderr)
        return None, None

    compiled_rules = compile_rules_func(config["rules"])
    ignore_dirs = config["ignore_dirs"]
    cache[project_key] = (compiled_rules, ignore_dirs)
    return cache[project_key]

def should_ignore(path, ignore_dirs):
    """Return True if path should be ignored."""
    path_str = path.as_posix().lower()
    return any(ignored.lower() in path_str for ignored in ignore_dirs)

def process_files(files, config_filename, compile_rules_func, check_file_func, cache):
    """Process multiple files and return validation results."""
    all_errors = []
    for file_str in files:
        file_path = Path(file_str)
        if not file_path.exists():
            continue

        project_path = extract_project_path(file_path)
        if not project_path:
            continue

        compiled_rules, ignore_dirs = get_compiled_rules_for_project(
            project_path, config_filename, compile_rules_func, cache
        )
        if compiled_rules is None:
            continue

        result = check_file_func(file_path, compiled_rules, ignore_dirs, root_dir=project_path)
        if result:
            if isinstance(result, list):
                all_errors.extend(result)
            else:
                all_errors.append(result)
    return all_errors

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
                "pattern": re.compile(p["pattern"]),
                "comment": p.get("comment", "No comment")
            }
            for p in rule["patterns"]
        ]
        for ext, rule in rules.items()
    }

def check_file(file_path, compiled_rules, ignore_dirs, root_dir):
    """
    Validate single file according to code style rules.

    Args:
        file_path: Path object of the file to validate.
        compiled_rules: Dictionary of compiled rules by file extension.
        ignore_dirs: List of directories to ignore.
        root_dir: Root directory for relative path calculation.

    Returns:
        list: List of error dictionaries or None if valid.
    """
    # Skip non-files and ignored directories.
    if not file_path.is_file() or should_ignore(file_path, ignore_dirs):
        return None

    ext = file_path.suffix.lower()

    # Skip extensions without rules.
    if ext not in compiled_rules:
        return None

    errors = []

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
    except Exception as e:
        return [{
            "file": str(file_path).replace('\\', '/'),
            "line": 0,
            "comment": f"Failed to read file: {e}"
        }]

    for line_num, line in enumerate(lines, 1):
        line_content = line.rstrip('\n\r')

        for rule in compiled_rules[ext]:
            # Check if pattern matches (indicates violation)
            if rule["pattern"].search(line_content):
                errors.append({
                    "file": str(file_path).replace('\\', '/'),
                    "line": line_num,
                    "comment": rule["comment"]
                })

    return errors if errors else None

def main():
    """Main entry point for the validator."""
    parser = argparse.ArgumentParser(description="Validate code style")
    parser.add_argument("files", nargs='+', help="List of files to validate")
    args = parser.parse_args()

    all_errors = process_files(
        args.files,
        "code_style_config.json",
        compile_rules,
        check_file,
        _compiled_rules_cache
    )

    # Always output JSON
    print(json.dumps(all_errors, indent=2, ensure_ascii=False))

    if all_errors:
        sys.exit(1)
    else:
        sys.exit(0)

if __name__ == "__main__":
    main()
