BEPINEX_FOLDER = "D:\\Apps\\Steam\\steamapps\\common\\Hollow Knight Silksong\\BepInEx\\plugins\\SilkenImpact"
OUTPUT_FOLDER = "C:\\Users\\Steven\\Desktop\\silksong"
THUNDERSTORE_ASSET_FOLDER = ".\\thunder store assets"
CHANGE_LOG_PATH = ".\\CHANGELOG.md"
CSPROJ_PATH = ".\\SilkenImpact.csproj"
import json
import os
import zipfile
import shutil
import tempfile
import re


def zip_folder(folder_path, output_path):

    with zipfile.ZipFile(output_path, "w", zipfile.ZIP_DEFLATED) as zipf:
        for root, dirs, files in os.walk(folder_path):
            for file in files:
                file_path = os.path.join(root, file)
                arcname = os.path.relpath(file_path, folder_path)
                zipf.write(file_path, arcname)


def make_nexus_output():
    zip_folder(BEPINEX_FOLDER, OUTPUT_FOLDER + "\\SilkenImpact_Nexus.zip")


def get_version_from_changlog():
    # open CHANGELOG, to see the latest version.
    # The version number is in heading-3 (### x.x.x)
    try:
        with open(CHANGE_LOG_PATH, "r", encoding="utf-8") as f:
            lines = f.readlines()

        latest_version = None
        # Find the first heading-3 line (### x.x.x)
        for line in lines:
            line = line.strip()
            if line.startswith("### "):
                # Extract version number after "### "
                latest_version = line[4:].strip()

        if latest_version is not None:
            return latest_version
        # If no version found, return None or raise exception
        raise ValueError("No version number found in CHANGELOG.md")
    except FileNotFoundError:
        raise FileNotFoundError(f"CHANGELOG.md not found at: {CHANGE_LOG_PATH}")
    except Exception as e:
        raise Exception(f"Error reading CHANGELOG.md: {e}")


def get_version_from_csproj():
    # open SilkenImpact.csproj, to see the version number.
    csproj_path = CSPROJ_PATH
    try:
        with open(csproj_path, "r", encoding="utf-8") as f:
            lines = f.readlines()

        version = None
        for line in lines:
            line = line.strip()
            if line.startswith("<Version>") and line.endswith("</Version>"):
                version = line[len("<Version>") : -len("</Version>")].strip()
                break

        if version is not None:
            return version
        raise ValueError("No version number found in SilkenImpact.csproj")
    except FileNotFoundError:
        raise FileNotFoundError(f"SilkenImpact.csproj not found at: {csproj_path}")
    except Exception as e:
        raise Exception(f"Error reading SilkenImpact.csproj: {e}")


def set_version_in_csproj(version):
    csproj_path = CSPROJ_PATH
    try:
        with open(csproj_path, "r", encoding="utf-8") as f:
            content = f.read()

        pattern = r"<Version>.*?</Version>"
        replacement = f"<Version>{version}</Version>"

        new_content = re.sub(pattern, replacement, content)

        with open(csproj_path, "w", encoding="utf-8") as f:
            f.write(new_content)

    except FileNotFoundError:
        raise FileNotFoundError(f"SilkenImpact.csproj not found at: {csproj_path}")
    except Exception as e:
        raise Exception(f"Error updating SilkenImpact.csproj: {e}")


def make_thunderstore_output(version: str):
    # modify the version in thunder store manifest.json
    manifest_path = os.path.join(THUNDERSTORE_ASSET_FOLDER, "manifest.json")
    with open(manifest_path, "r", encoding="utf-8") as f:
        manifest = json.load(f)

    # modify version_number and write back to manifest.json
    manifest["version_number"] = version
    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, ensure_ascii=False, indent=2)

    out_dir = os.path.abspath(OUTPUT_FOLDER)
    assets_src = os.path.abspath(THUNDERSTORE_ASSET_FOLDER)

    if not os.path.isdir(assets_src):
        raise FileNotFoundError(f"Thunderstore asset folder not found: {assets_src}")

    os.makedirs(out_dir, exist_ok=True)

    temp_dir = tempfile.mkdtemp(prefix="thunderstore_", dir=out_dir)
    try:
        # copy contents of THUNDERSTORE_ASSET_FOLDER into the temp folder
        for entry in os.listdir(assets_src):
            src = os.path.join(assets_src, entry)
            dst = os.path.join(temp_dir, entry)
            if os.path.isdir(src):
                shutil.copytree(src, dst)
            else:
                shutil.copy2(src, dst)

        # copy CHANGELOG.md into the temp folder
        shutil.copy2(CHANGE_LOG_PATH, os.path.join(temp_dir, "CHANGELOG.md"))

        # copy the content of BEPINEX_FOLDER into the temp folder
        for entry in os.listdir(BEPINEX_FOLDER):
            src = os.path.join(BEPINEX_FOLDER, entry)
            dst = os.path.join(temp_dir, entry)
            if os.path.isdir(src):
                shutil.copytree(src, dst)
            else:
                shutil.copy2(src, dst)

        # create zip in the output folder
        zip_path = os.path.join(out_dir, "SilkenImpact_Thunderstore.zip")
        zip_folder(temp_dir, zip_path)
    finally:
        # cleanup temp folder
        shutil.rmtree(temp_dir, ignore_errors=True)


def check_versions(release_version) -> bool:
    passed = True
    log_latest_version = get_version_from_changlog()
    if log_latest_version != release_version:
        passed = False
        print(f"Warning: The version in CHANGELOG.md ({log_latest_version}) does not match the specified version ({release_version}).")
    project_version = get_version_from_csproj()
    if project_version != release_version:
        passed = False
        print(f"Warning: The version in SilkenImpact.csproj ({project_version}) does not match the specified version ({release_version}).")
        set_version_in_csproj(release_version)
        print(f"Updated version to {release_version}. Make sure to build again")
    return passed


if __name__ == "__main__":
    make_nexus_output()
    version = input("Enter the version number for Thunderstore (e.g., 1.0.1): ")
    if not check_versions(version):
        print("Version check failed. Please ensure versions are consistent.")
        exit(0)
    make_thunderstore_output(version)
    print("Export complete.")
    print("See the output at: " + OUTPUT_FOLDER)
