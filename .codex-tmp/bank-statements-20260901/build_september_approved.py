import csv
import json
from pathlib import Path


ROOT = Path(__file__).parent
DESCRIPTIONS = [
    "Canone BNL", "Findomestic", "Younited", "Stipendio", "Mantenimento Debora", "Findomestic", "Carta principale", "Bonifico Ericka",
    "Versamento PayPal", "Agos", "Cofidis", "Assicurazione Casa", "Spese conto", "Affitto", "Assicurazione Auto", "Assicurazione Auto",
    "American Express", "Fastweb", "Polizza Reddito Protetto", "Versamento contanti", "Versamento PayPal", "Bonifico Ericka", "Ritiro Folletto",
    "Commissioni Telepass", "Stipendio", "Telepass", "Rateo Spese",
]
CATEGORIES = [
    None, "Prestiti", "Prestiti", "Entrate", "Mantenimento", "Prestiti", None, "Casa", None, "Prestiti", "Prestiti", "Casa", None,
    "Casa", "Auto", "Auto", None, "Utenze", "Polizze", None, None, "Casa", "Casa", None, "Entrate", None, "Prestiti",
]

with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [row for row in csv.DictReader(stream, delimiter=";") if "2023-09-01" <= row["bookingDate"] < "2023-10-01"]

if len(rows) != len(DESCRIPTIONS) or len(rows) != len(CATEGORIES):
    raise RuntimeError("Il numero dei movimenti di settembre non coincide con la revisione approvata.")

approved = [
    {
        "requestId": request_id,
        "date": row["bookingDate"],
        "description": description,
        "formula": row["amount"].replace(".", ","),
        "categoryName": category,
        "sourceDescription": row["description"],
    }
    for request_id, (row, description, category) in enumerate(zip(rows, DESCRIPTIONS, CATEGORIES), start=1)
]

(ROOT / "2023-09-approved.json").write_text(json.dumps(approved, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")
