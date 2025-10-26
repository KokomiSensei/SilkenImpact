BEPINEX_FOLDER = "D:\\Apps\\Steam\\steamapps\\common\\Hollow Knight Silksong\\BepInEx\\plugins\\SilkenImpact"
OUTPUT_FOLDER = "C:\\Users\\Steven\\Desktop\\silksong"
THUNDERSTORE_ASSET_FOLDER = ".\\thunder store assets"
CHANGE_LOG_PATH = ".\\CHANGELOG.md"
import json
import os
import zipfile
import shutil
import tempfile


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
        log_latest_version = get_version_from_changlog()
        if log_latest_version != version:
            print(f"Warning: The version in CHANGELOG.md ({log_latest_version}) does not match the specified version ({version}).")
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


if __name__ == "__main__":
    make_nexus_output()
    version = input("Enter the version number for Thunderstore (e.g., 1.0.1): ")
    make_thunderstore_output(version)
    print("Export complete.")
    print("See the output at: " + OUTPUT_FOLDER)
