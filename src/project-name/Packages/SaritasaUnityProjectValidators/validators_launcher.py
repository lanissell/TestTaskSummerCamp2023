#!/usr/bin/env python3
"""
Validators Launcher.
Runs all validators on provided files.
"""
import sys
import argparse
import subprocess
import json
from pathlib import Path


def run_validator(validator_path, files):
    """
    Run a single validator on files via subprocess.

    Args:
        validator_path: Path to validator script.
        files: List of file paths.

    Returns:
        tuple: (bool, list) - success status and list of errors.
    """
    validator_name = validator_path.stem

    try:
        # Run validator as subprocess
        cmd = [sys.executable, str(validator_path)] + files
        result = subprocess.run(cmd, capture_output=True, text=True)

        errors = []
        if result.stdout:
            try:
                errors = json.loads(result.stdout)
            except json.JSONDecodeError:
                pass

        if result.stderr:
            print(result.stderr, file=sys.stderr)

        return result.returncode == 0, errors

    except Exception as e:
        print(f"[ERROR] Failed to run validator {validator_name}: {e}", file=sys.stderr)
        return False, []


def print_pretty_console(all_results):
    """
    Print results in a pretty console format.

    Args:
        all_results: Dictionary of validator name -> list of errors.
    """
    has_errors = False

    for validator_name, errors in all_results.items():
        if errors:
            has_errors = True
            print(f"\n{'='*5}{validator_name}{'='*5}")

            for error in errors:
                print(f"\nFile: {error.get('file', 'Unknown')}")
                print(f"Comment: {error.get('comment', 'No comment')}")

    if not has_errors:
        print("All validations passe.")


def main():
    """Main entry point for the validators launcher."""
    parser = argparse.ArgumentParser(description="Run all validators")
    group = parser.add_mutually_exclusive_group(required=True)
    group.add_argument("--files", nargs='+', help="List of files to validate")
    group.add_argument("--dir", help="Directory to scan for files")
    parser.add_argument("--validators", nargs='+', help="List of validator paths to run", required=True)
    parser.add_argument("--json", action="store_true", help="Output results as JSON")
    args = parser.parse_args()

    # Collect files
    if args.files:
        files = args.files
    else:
        files = [str(f) for f in Path(args.dir).rglob("*") if f.is_file()]

    if not files:
        print("No files to validate")

    # Run all validators
    all_results = {}

    for validator_path in args.validators:
        validator_path = Path(validator_path)
        if not validator_path.exists():
            print(f"Validator not found: {validator_path}", file=sys.stderr)
            continue

        passed, errors = run_validator(validator_path, files)
        validator_name = validator_path.stem
        all_results[validator_name] = errors

    # Output results
    if args.json:
        print(json.dumps(all_results, indent=2, ensure_ascii=False))
    else:
        print_pretty_console(all_results)


if __name__ == "__main__":
    main()
