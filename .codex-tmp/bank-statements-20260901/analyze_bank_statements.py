import csv
import json
import re
from datetime import datetime
from decimal import Decimal
from pathlib import Path

import pdfplumber


ROOT = Path(__file__).parent
DATE_PATTERN = re.compile(r"^\d{2}/\d{2}/\d{4}$")


def parse_amount(value: str) -> Decimal | None:
    cleaned = value.replace("€", "").replace("�", "").replace(".", "").replace(",", ".").strip()
    return Decimal(cleaned) if cleaned else None


def clean(value: str | None) -> str:
    return re.sub(r"\s+", " ", value or "").strip()


def extract_statement(path: Path) -> list[dict]:
    movements = []
    with pdfplumber.open(path) as pdf:
        first_page = clean(pdf.pages[0].extract_text())
        if "ESTRATTO CONTO N." not in first_page or "AI MOVIMENTI DAL" not in first_page:
            return movements

        for page_number, page in enumerate(pdf.pages, start=1):
            words = page.extract_words()
            closing_tops = [
                word["top"] for word in words
                if word["x0"] < 100 and word["text"].upper() == "IL" and word["top"] > 180
                and any(other["text"].upper() == "SALDO" and abs(other["top"] - word["top"]) < 1 for other in words)
            ]
            page_bottom = min((top - 5 for top in closing_tops), default=780)
            starts = [
                word for word in words
                if word["x0"] < 100 and DATE_PATTERN.match(word["text"]) and 155 <= word["top"] < page_bottom
            ]
            starts.sort(key=lambda word: word["top"])

            for index, start in enumerate(starts):
                top = start["top"]
                bottom = starts[index + 1]["top"] if index + 1 < len(starts) else page_bottom
                block = [word for word in words if top - 0.5 <= word["top"] < bottom - 0.5]
                booking_date = start["text"]
                value_date_words = [word for word in block if 110 <= word["x0"] < 180 and DATE_PATTERN.match(word["text"])]
                code_words = [word for word in block if 180 <= word["x0"] < 216 and word["top"] < top + 1]
                description_words = [word for word in block if 216 <= word["x0"] < 446]
                debit_words = [
                    word for word in block
                    if 446 <= word["x0"] < 506 and abs(word["top"] - top) < 1 and re.search(r"\d", word["text"])
                ]
                credit_words = [
                    word for word in block
                    if 506 <= word["x0"] and abs(word["top"] - top) < 1 and re.search(r"\d", word["text"])
                ]

                value_date = value_date_words[0]["text"] if value_date_words else ""
                code = clean(" ".join(word["text"] for word in sorted(code_words, key=lambda word: word["x0"])))
                description = clean(" ".join(word["text"] for word in sorted(description_words, key=lambda word: (word["top"], word["x0"]))))
                debit = parse_amount(debit_words[0]["text"]) if debit_words else None
                credit = parse_amount(credit_words[0]["text"]) if credit_words else None
                if description.upper().startswith("SALDO "):
                    continue
                if (debit is None) == (credit is None):
                    continue

                movements.append(
                    {
                        "source": path.name,
                        "page": page_number,
                        "bookingDate": datetime.strptime(booking_date, "%d/%m/%Y").date().isoformat(),
                        "valueDate": datetime.strptime(value_date, "%d/%m/%Y").date().isoformat() if DATE_PATTERN.match(value_date) else None,
                        "code": code,
                        "description": description,
                        "amount": str(credit if credit is not None else -debit),
                    }
                )
    return movements


movements = []
for pdf_path in sorted(ROOT.glob("*.pdf")):
    movements.extend(extract_statement(pdf_path))

movements.sort(key=lambda item: (item["bookingDate"], item["source"], item["page"], item["description"], item["amount"]))

with (ROOT / "raw-movements.json").open("w", encoding="utf-8", newline="") as stream:
    json.dump(movements, stream, ensure_ascii=False, indent=2)

with (ROOT / "raw-movements.csv").open("w", encoding="utf-8-sig", newline="") as stream:
    writer = csv.DictWriter(stream, fieldnames=["bookingDate", "valueDate", "description", "amount", "code", "source", "page"], delimiter=";")
    writer.writeheader()
    writer.writerows(movements)

print(json.dumps({
    "count": len(movements),
    "firstDate": movements[0]["bookingDate"] if movements else None,
    "lastDate": movements[-1]["bookingDate"] if movements else None,
    "statements": len(set(item["source"] for item in movements)),
}, ensure_ascii=False))
