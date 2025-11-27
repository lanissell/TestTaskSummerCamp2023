#!/usr/bin/env python3
"""
Code Style Validator using Roslynator.
Compatible with validators_launcher: accepts file paths via argv, prints JSON list of {file, comment}.
"""
import sys
import json
import subprocess
import re
import tempfile
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import List, Dict, Any, Set

def check_roslynator_installed() -> bool:
    """Check if Roslynator is installed."""
    try:
        result = subprocess.run(["dotnet", "roslynator", "--version"],
                              capture_output=True, text=True, timeout=10,
                              encoding='utf-8', errors='replace')
        return result.returncode == 0
    except (subprocess.TimeoutExpired, FileNotFoundError, Exception):
        return False


def install_roslynator() -> bool:
    """Try to install Roslynator as a global tool."""
    try:
        result = subprocess.run(["dotnet", "tool", "install", "-g", "roslynator.dotnet.cli"],
                              capture_output=True, text=True, timeout=300,
                              encoding='utf-8', errors='replace')
        return result.returncode == 0
    except (subprocess.TimeoutExpired, FileNotFoundError, Exception):
        return False


def ensure_dependencies() -> bool:
    if not check_roslynator_installed():
        if not install_roslynator():
            return False

    return True


def find_projects(file_paths: List[str]) -> Set[Path]:
    """Find nearest .sln or .csproj for each .cs file."""
    projects: Set[Path] = set()
    for p in map(Path, file_paths):
        if p.suffix.lower() != ".cs":
            continue
        cur = p.parent
        while True:
            csprojs = list(cur.glob("*.csproj"))
            if csprojs:
                projects.update(csprojs)
                break
            if cur == cur.parent:
                break
            cur = cur.parent
    return projects


def parse_roslynator_xml(xml_content: str) -> List[Dict[str, Any]]:
    """Parse Roslynator XML output into list of {file, comment}."""
    res: List[Dict[str, Any]] = []
    
    try:
        root = ET.fromstring(xml_content)

        for project in root.findall('.//Project'):
            for diagnostic in project.findall('.//Diagnostic'):
                diagnostic_id = diagnostic.get('Id')
                severity_elem = diagnostic.find('Severity')
                message_elem = diagnostic.find('Message')
                file_path_elem = diagnostic.find('FilePath')
                location_elem = diagnostic.find('Location')

                if file_path_elem is not None and message_elem is not None:
                    file_path = file_path_elem.text
                    message = message_elem.text
                    severity = severity_elem.text if severity_elem is not None else 'info'
                    line_num = location_elem.get('Line') if location_elem is not None else None

                    message_clean = re.sub(r'\[.*?]', '', message).strip()
                    message_clean = message_clean.rstrip('.')
                    comment = f"[{severity.lower()}] {diagnostic_id or 'UNKNOWN'}: {message_clean}."

                    norm = str(Path(file_path).resolve())
                    result_item = {"file": norm, "comment": comment}
                    if line_num:
                        result_item["line"] = int(line_num)
                    res.append(result_item)
    except ET.ParseError:
        pass

    return res


def run_roslynator_analyze(project: Path) -> List[Dict[str, Any]]:
    """Run Roslynator analyze and return code style issues."""
    errors: List[Dict[str, Any]] = []

    with tempfile.NamedTemporaryFile(mode='w', suffix='.xml', delete=False) as temp_file:
        output_file = temp_file.name

    try:
        cmd = [
            "roslynator", "analyze", str(project),
            "--verbosity", "minimal",
            "--output", output_file,
            "--output-format", "xml"
        ]

        subprocess.run(cmd, capture_output=True, text=True, check=False, encoding='utf-8', errors='replace')

        # Try to read XML output file
        try:
            with open(output_file, 'r', encoding='utf-8') as f:
                xml_content = f.read()
            if xml_content.strip():
                errors.extend(parse_roslynator_xml(xml_content))
        except (FileNotFoundError, PermissionError):
            pass

    except subprocess.TimeoutExpired:
        errors.append({"file": str(project), "comment": "Roslynator analyze timed out"})
    except Exception as e:
        errors.append({"file": str(project), "comment": f"Error running Roslynator: {str(e)}"})
    finally:
        pass
        # Clean up temp file
        try:
            Path(output_file).unlink()
        except Exception:
            pass

    return errors


def main():
    # Ensure dependencies are installed
    if not ensure_dependencies():
        print(json.dumps([{"file": "", "comment": "Failed to install required dependencies (dotnet/Roslynator)"}],
                        ensure_ascii=True))
        sys.exit(0)

    # validators_launcher passes files as argv
    files = sys.argv[1:]
    if not files:
        print(json.dumps([], ensure_ascii=True))
        sys.exit(0)

    requested = {str(Path(f).resolve()) for f in files}
    projects = find_projects(files)
    if not projects:
        print(json.dumps([], ensure_ascii=True))
        sys.exit(0)

    all_errors: List[Dict[str, Any]] = []
    for proj in projects:
        all_errors.extend(run_roslynator_analyze(proj))

    # Filter only requested files
    filtered: List[Dict[str, Any]] = []
    for e in all_errors:
        fp = e.get("file")
        if not fp:
            continue
        try:
            if str(Path(fp).resolve()) in requested:
                filtered.append(e)
        except Exception:
            continue

    print(json.dumps(filtered, ensure_ascii=True, indent=2))
    sys.exit(0)

if __name__ == "__main__":
    main()
