import csv
import json
from pathlib import Path


ROOT = Path(__file__).parent
DESCRIPTIONS = [
    "Canone BNL",
    "Stipendio",
    "Findomestic",
    "Mantenimento Debora",
    "Bonifico Ericka",
    "Younited",
    "Carta principale",
    "Agos",
    "Cofidis",
    "Assicurazione Casa",
    "Affitto",
    "Versamento PayPal",
    "Spese conto",
    "Multa",
    "Assicurazione Auto",
    "Assicurazione Auto",
    "Bonifico Ericka",
    "American Express",
    "Versamento PayPal",
    "Versamento PayPal",
    "Prelievo",
    "Versamento contanti",
    "Fastweb",
    "Polizza Reddito Protetto",
    "Imposte e tasse",
    "Ritiro Folletto",
    "Prestito Findomestic",
    "Commissioni Telepass",
    "Bonifico Anna La Rocca",
    "Telepass",
]
CATEGORIES = [
    None,
    "Entrate",
    "Prestiti",
    "Mantenimento",
    "Casa",
    "Prestiti",
    None,
    "Prestiti",
    "Prestiti",
    "Casa",
    "Casa",
    None,
    None,
    "Multe",
    "Auto",
    "Auto",
    "Casa",
    None,
    None,
    None,
    "VitaSociale",
    None,
    "Utenze",
    "Polizze",
    "Tasse",
    "Spesa",
    None,
    None,
    None,
    None,
]


with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [
        row for row in csv.DictReader(stream, delimiter=";")
        if "2023-07-01" <= row["bookingDate"] < "2023-08-01"
    ]

if len(rows) != len(DESCRIPTIONS) or len(rows) != len(CATEGORIES):
    raise RuntimeError("Il numero dei movimenti di luglio non coincide con la revisione approvata.")

approved = []
for request_id, (row, description, category) in enumerate(zip(rows, DESCRIPTIONS, CATEGORIES), start=1):
    approved.append(
        {
            "requestId": request_id,
            "date": row["bookingDate"],
            "description": description,
            "formula": row["amount"].replace(".", ","),
            "categoryName": category,
            "sourceDescription": row["description"],
        }
    )

(ROOT / "2023-07-approved.json").write_text(
    json.dumps(approved, ensure_ascii=False, indent=2) + "\n",
    encoding="utf-8",
    newline="",
)
