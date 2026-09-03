import csv
import json
from pathlib import Path


ROOT = Path(__file__).parent
DESCRIPTIONS = [
    "Canone BNL", "Stipendio", "Benzina Esso", "Spesa alimentare", "L'Ortobello", "Findomestic", "Affitto", "Luce/Gas", "Younited",
    "Essere buoni ripaga", "Pizzeria da Diego", "Benzina Esso", "Carta principale", "Agos", "Assicurazione Casa", "Spese conto",
    "Bonifico Findomestic", "Cofidis", "Affitto", "Storno bonifico Findomestic", "Bonifico Ericka", "Christian Canessa", "Assicurazione Auto",
    "Assicurazione Auto", "Prestito", "Edison", "Edison", "American Express", "Versamento contanti", "Prelievo", "Bonifico Ericka", "Fastweb",
    "Polizza Reddito Protetto", "Ritiro Folletto", "Bonifico Ericka", "Bonifico Ericka", "Commissioni Telepass", "Telepass", "Finanziamento 2286061",
]
CATEGORIES = [
    None, "Entrate", "Auto", "Spesa", "Spesa", "Prestiti", "Casa", "Utenze", "Prestiti", "EntrateExtra", "VitaSociale", "Auto", None,
    "Prestiti", "Casa", None, None, "Prestiti", "Casa", None, "Casa", "Subbuteo", "Auto", "Auto", None, "Utenze", "Utenze", None, None,
    "VitaSociale", "Casa", "Utenze", "Polizze", "Casa", "Casa", "Casa", None, None, "Prestiti",
]

with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [row for row in csv.DictReader(stream, delimiter=";") if "2023-08-01" <= row["bookingDate"] < "2023-09-01"]

if len(rows) != len(DESCRIPTIONS) or len(rows) != len(CATEGORIES):
    raise RuntimeError("Il numero dei movimenti di agosto non coincide con la revisione approvata.")

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

(ROOT / "2023-08-approved.json").write_text(json.dumps(approved, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")
