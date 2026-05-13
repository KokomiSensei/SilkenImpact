import csv
import re
from pathlib import Path

# Update this path when needed.
LOG_PATH = Path(r"D:\Apps\Steam\steamapps\common\Hollow Knight Silksong\BepInEx\LogOutput.log")
CSV_OUTPUT_PATH = Path("dist/name_resolve_journal_matches.csv")

# Capturing groups:
# 1) strong|weak
# 2) game object name
# 3) record name
# 4) match count
PATTERN = re.compile(r"NameResolveJournal(strong|weak).*=(.+)\s.+=(.+).*\s.+=(.+)", re.IGNORECASE)


def normalize(s: str) -> str:
    return s.strip()


total_match_count = 0


def extract_rows(log_path: Path):
    seen = set()
    rows_in_log_order = []

    with log_path.open("r", encoding="utf-8", errors="ignore") as f:
        for line in f:
            m = PATTERN.search(line)
            if not m:
                continue

            global total_match_count
            total_match_count += 1

            strength = normalize(m.group(1)).lower()
            game_object_name = normalize(m.group(2))
            record_name = normalize(m.group(3))
            match = normalize(m.group(4))

            row = (strength, game_object_name, record_name, match)
            if row in seen:
                continue
            seen.add(row)
            rows_in_log_order.append(row)

    rows_sorted = sorted(rows_in_log_order, key=lambda x: x[1].lower())
    return rows_in_log_order, rows_sorted


def write_csv(rows, output_path: Path):
    with output_path.open("w", newline="", encoding="utf-8-sig") as f:
        writer = csv.writer(f)
        writer.writerow(["strength", "game_object_name", "record_name", "match"])
        writer.writerows(rows)


def main():
    if not LOG_PATH.exists():
        raise FileNotFoundError(f"Log file not found: {LOG_PATH}")

    rows_in_log_order, rows_sorted = extract_rows(LOG_PATH)
    write_csv(rows_in_log_order, CSV_OUTPUT_PATH.with_stem(CSV_OUTPUT_PATH.stem + "_in_log_order"))
    write_csv(rows_sorted, CSV_OUTPUT_PATH.with_stem(CSV_OUTPUT_PATH.stem + "_sorted"))
    print(
        f"Extracted {len(rows_in_log_order)} unique rows to: "
        f"{CSV_OUTPUT_PATH.with_stem(CSV_OUTPUT_PATH.stem + '_in_log_order').resolve()} and "
        f"{CSV_OUTPUT_PATH.with_stem(CSV_OUTPUT_PATH.stem + '_sorted').resolve()}"
    )
    print(f"Total match count: {total_match_count}")


if __name__ == "__main__":
    main()
