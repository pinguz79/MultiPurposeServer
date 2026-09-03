import csv
import json
from pathlib import Path


ROOT = Path(__file__).parent
DESCRIPTIONS = [
    "Versamento PayPal", "Canone BNL", "Stipendio", "Luce/Gas", "Luce/Gas", "Findomestic", "Bonifico Ericka", "Prestito", "Younited",
    "Findomestic", "Carta principale", "Giroconto", "Agos", "Assicurazione Casa", "Versamento PayPal", "Golden", "Prelievo", "Bonifico Ericka",
    "Cofidis", "Prelievo", "Affitto", "Prestito", "Spese conto", "Edison", "Prestito Findomestic", "Bonifico Marleny Velasquez", "Edison",
    "Assicurazione Auto", "Assicurazione Auto", "Estinzione Rateo Spese", "Estinzione finanziamento 2310847", "Bonifico Ines Adosio",
    "American Express", "Essere buoni ripaga", "Fastweb", "Polizza Reddito Protetto", "Ritiro Folletto", "Imposte e tasse",
    "Commissioni Telepass", "Stipendio", "Telepass",
]
CATEGORIES = [
    None, None, "Entrate", "Utenze", "Utenze", "Prestiti", "Casa", None, "Prestiti", "Prestiti", None, None, "Prestiti", "Casa", None,
    "VitaSociale", "VitaSociale", "Casa", "Prestiti", "VitaSociale", "Casa", None, None, "Utenze", None, None, "Utenze", "Auto", "Auto",
    "Prestiti", "Prestiti", None, None, "EntrateExtra", "Utenze", "Polizze", "Casa", "Tasse", None, "Entrate", None,
]

with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [row for row in csv.DictReader(stream, delimiter=";") if "2023-10-01" <= row["bookingDate"] < "2023-11-01"]

if len(rows) != len(DESCRIPTIONS) or len(rows) != len(CATEGORIES):
    raise RuntimeError("Il numero dei movimenti di ottobre non coincide con la revisione approvata.")

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

(ROOT / "2023-10-approved.json").write_text(json.dumps(approved, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")
