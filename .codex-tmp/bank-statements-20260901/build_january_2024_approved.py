import csv
import json
from pathlib import Path


ROOT = Path(__file__).parent
DESCRIPTIONS = [
    "Cofidis", "Foto natalizie", "Canone BNL", "Interessi", "Findomestic", "Giroconto", "Giroconto", "Younited", "Bonifico Ericka",
    "Findomestic", "Findomestic", "Prelievo", "Carta principale", "Agos", "Versamento PayPal", "Assicurazione Casa", "Prelievo",
    "Bonifico Daniele Mengarelli", "Cofidis", "Prodotti Occhipinti", "Spese conto", "Affitto", "Assicurazione Auto", "Prelievo",
    "Prestito Younited", "Assicurazione Auto", "American Express", "Giroconto", "Fastweb", "Polizza Reddito Protetto", "Multa",
    "Ritiro Folletto", "Cofidis", "Commissioni Telepass", "Stipendio", "Telepass", "Rateo Spese", "Giroconto",
]
CATEGORIES = [
    "Prestiti", "EntrateExtra", None, None, "Prestiti", None, None, "Prestiti", "Casa", "Prestiti", "Prestiti", "VitaSociale", None,
    "Prestiti", None, "Casa", "VitaSociale", None, "Prestiti", None, None, "Casa", "Auto", "VitaSociale", None, "Auto", None, None,
    "Utenze", "Polizze", "Multe", "Casa", "Prestiti", None, "Entrate", None, "Prestiti", None,
]

with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [row for row in csv.DictReader(stream, delimiter=";") if "2024-01-01" <= row["bookingDate"] < "2024-02-01"]

if len(rows) != len(DESCRIPTIONS) or len(rows) != len(CATEGORIES):
    raise RuntimeError("Il numero dei movimenti di gennaio 2024 non coincide con la revisione approvata.")

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

(ROOT / "2024-01-approved.json").write_text(json.dumps(approved, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")
