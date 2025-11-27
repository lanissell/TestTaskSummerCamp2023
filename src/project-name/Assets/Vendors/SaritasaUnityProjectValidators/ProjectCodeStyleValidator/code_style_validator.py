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

def extract_project_path(file_path: Path) -> Path | None:
    """Extract project path from file path (up to */src/project-name)."""
    path_str = str(file_path.resolve())
    pattern = r'^(.*[/\\]src[/\\][^/\\]+)'
    match = re.search(pattern, path_str, re.IGNORECASE)
    if match:
        return Path(match.group(1))
    return None


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


def create_temporary_csproj(cs_files: List[Path], project_root: Path) -> Path | None:
    """Create a temporary .csproj for a single project including given C# files and its analyzers."""
    # Resolve analyzers folder: <project_root>/Assets/Vendors/SaritasaUnityProjectValidators/ProjectCodeStyleValidator/Analyzers
    analyzers_dir = project_root / "Assets" / "Vendors" / "SaritasaUnityProjectValidators" / "ProjectCodeStyleValidator" / "Analyzers"
    if not analyzers_dir.exists() or not analyzers_dir.is_dir():
        return None

    analyzer_files: List[Path] = []
    # Collect all .dll recursively inside Analyzers (including subfolders like en-GB)
    for p in analyzers_dir.rglob("*.dll"):
        analyzer_files.append(p)

    if not analyzer_files:
        return None

    temp_dir = Path(tempfile.mkdtemp())
    csproj_path = temp_dir / "temp_project.csproj"
    content = [
        '<Project Sdk="Microsoft.NET.Sdk">',
        '  <PropertyGroup>',
        '    <TargetFramework>net6.0</TargetFramework>',
        '    <Nullable>enable</Nullable>',
        '    <ImplicitUsings>enable</ImplicitUsings>',
        '  </PropertyGroup>',
        '  <ItemGroup>',
    ]
    for cs in cs_files:
        abs_path = str(cs.resolve()).replace('\\', '/')
        content.append(f'    <Compile Include="{abs_path}" />')
    content += [
        '  </ItemGroup>',
        '  <ItemGroup>',
    ]
    for analyzer in analyzer_files:
        abs_path = str(analyzer.resolve()).replace('\\', '/')
        content.append(f'    <Analyzer Include="{abs_path}" />')
    content += [
        '  </ItemGroup>',
        '</Project>',
    ]
    csproj_path.write_text("\n".join(content), encoding="utf-8")
    return csproj_path


def parse_roslynator_xml(xml_content: str) -> List[Dict[str, Any]]:
    """Parse Roslynator XML output into list of {file, comment}."""
    res: List[Dict[str, Any]] = []
    if not xml_content or not xml_content.strip():
        return res
    try:
        root = ET.fromstring(xml_content)
        for project in root.findall('.//Project'):
            for diagnostic in project.findall('.//Diagnostic'):
                diagnostic_id = diagnostic.get('Id')
                severity_elem = diagnostic.find('Severity')
                message_elem = diagnostic.find('Message')
                file_path_elem = diagnostic.find('FilePath')
                location_elem = diagnostic.find('Location')

                file_text = file_path_elem.text if file_path_elem is not None else None
                msg_text = message_elem.text if message_elem is not None else None
                if not file_text or not msg_text:
                    continue

                severity = (severity_elem.text if (severity_elem is not None and severity_elem.text) else 'info').lower()
                line_num = location_elem.get('Line') if (location_elem is not None and location_elem.get('Line')) else None

                message_clean = re.sub(r'\[.*?]', '', msg_text).strip()
                message_clean = message_clean.rstrip('.')
                comment = f"[{severity}] {diagnostic_id or 'UNKNOWN'}: {message_clean}."

                try:
                    norm = str(Path(file_text).resolve())
                except Exception:
                    norm = file_text
                item: Dict[str, Any] = {"file": norm, "comment": comment}
                if line_num and str(line_num).isdigit():
                    item["line"] = int(line_num)
                res.append(item)
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
            "--output-format", "xml",
            "--culture", "en-US",
            "--ignore-compiler-diagnostics"
        ]
        subprocess.run(cmd, capture_output=True, text=True, check=False, encoding='utf-8', errors='replace', timeout=120)
        try:
            xml_content = Path(output_file).read_text(encoding='utf-8')
            if xml_content and xml_content.strip():
                errors.extend(parse_roslynator_xml(xml_content))
        except Exception:
            pass
    except subprocess.TimeoutExpired:
        errors.append({"file": str(project), "comment": "Roslynator analyze timed out"})
    except Exception as e:
        errors.append({"file": str(project), "comment": f"Error running Roslynator: {str(e)}"})
    finally:
        try:
            Path(output_file).unlink(missing_ok=True)
        except Exception:
            pass
    return errors


def main():
    # Ensure dependencies are installed
    if not ensure_dependencies():
        print(json.dumps([{"file": "", "comment": "Failed to install required dependencies (dotnet/Roslynator)"}],
                        ensure_ascii=False))
        sys.exit(0)

    files = sys.argv[1:]
    if not files:
        print(json.dumps([], ensure_ascii=False))
        sys.exit(0)

    cs_files = [Path(f) for f in files if Path(f).suffix.lower() == ".cs"]
    if not cs_files:
        print(json.dumps([], ensure_ascii=False))
        sys.exit(0)

    requested = {str(Path(f).resolve()) for f in files}

    # Group files by project root and create separate temp projects
    project_groups: Dict[Path, List[Path]] = {}
    for cs in cs_files:
        proj = extract_project_path(cs)
        if proj is None:
            # Skip files not matching expected project structure
            continue
        project_groups.setdefault(proj, []).append(cs)

    all_errors: List[Dict[str, Any]] = []

    for project_root, files_in_project in project_groups.items():
        temp_csproj = create_temporary_csproj(files_in_project, project_root)
        if temp_csproj is None:
            # No analyzers for this project -> skip its files
            continue
        try:
            all_errors.extend(run_roslynator_analyze(temp_csproj))
        finally:
            # cleanup temp project and directory
            try:
                temp_dir = temp_csproj.parent
                temp_csproj.unlink(missing_ok=True)
                try:
                    next(temp_dir.iterdir())
                except StopIteration:
                    temp_dir.rmdir()
            except Exception:
                pass

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

    print(json.dumps(filtered, ensure_ascii=False, indent=2))
    sys.exit(0)

if __name__ == "__main__":
    main()
