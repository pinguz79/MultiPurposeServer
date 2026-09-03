import csv
import json
from collections import defaultdict
from decimal import Decimal
from pathlib import Path


ROOT = Path(__file__).parent
OUTPUT = ROOT / "proposal"


def format_formula(value: Decimal) -> str:
    sign = "-" if value < 0 else ""
    absolute = abs(value)
    integer, decimals = f"{absolute:.2f}".split(".")
    groups = []
    while integer:
        groups.insert(0, integer[-3:])
        integer = integer[:-3]
    return f"{sign}{'.'.join(groups)},{decimals}"


def clean_description(value: str) -> str:
    replacements = {
        "Canone Conto BNL": "Canone BNL",
    }
    return replacements.get(value, value)


with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [row for row in csv.DictReader(stream, delimiter=";") if row["bookingDate"] < "2026-01-01"]

by_month = defaultdict(list)
for row in rows:
    by_month[row["bookingDate"][:7]].append(row)

OUTPUT.mkdir(exist_ok=True)
summary = []
for month, month_rows in sorted(by_month.items()):
    items = []
    for request_id, row in enumerate(month_rows, start=1):
        items.append(
            {
                "requestId": request_id,
                "date": row["bookingDate"],
                "description": clean_description(row["description"]),
                "formula": format_formula(Decimal(row["amount"])),
            }
        )
    payload = {
        "options": {
            "persistenceStrategy": 0,
            "evaluationStrategy": 1,
        },
        "items": items,
    }
    target = OUTPUT / f"{month}-movimenti-proposti.json"
    target.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")
    summary.append(
        {
            "month": month,
            "count": len(month_rows),
            "total": format_formula(sum((Decimal(row["amount"]) for row in month_rows), Decimal("0"))),
            "file": target.name,
        }
    )

(OUTPUT / "riepilogo.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")

print(json.dumps({"months": len(summary), "movements": len(rows), "total": format_formula(sum((Decimal(row["amount"]) for row in rows), Decimal("0")))}, ensure_ascii=False))
