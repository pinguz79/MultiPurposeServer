import csv
import json
from pathlib import Path


ROOT = Path(__file__).parent
DESCRIPTIONS = [
    "Canone BNL", "Findomestic", "Bonifico Ericka", "Findomestic", "Findomestic", "Younited", "Eurospin", "Carta principale", "PayPal",
    "Prelievo", "Agos", "Edison", "Parcheggio Porto Antico", "Spese conto", "Prelievo", "Cofidis", "Prelievo", "Mondo Risparmio",
    "Assicurazione Auto", "American Express", "Pizzeria San Giorgio", "Prelievo", "PayPal", "PayPal", "Imposte e tasse", "Fastweb",
    "Polizza Reddito Protetto", "Edison", "Ritiro Folletto", "Giroconto", "Giroconto", "Cofidis", "Commissioni Telepass", "Stipendio",
    "Telepass", "Rateo Spese",
]
CATEGORIES = [
    None, "Prestiti", "Casa", "Prestiti", "Prestiti", "Prestiti", "Spesa", None, None, "VitaSociale", "Prestiti", "Utenze",
    "VitaSociale", None, "VitaSociale", "Prestiti", "VitaSociale", "Spesa", "Auto", None, "VitaSociale", "VitaSociale", None, None,
    "Tasse", "Utenze", "Polizze", "Utenze", "Casa", None, None, "Prestiti", None, "Entrate", None, "Prestiti",
]

with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [row for row in csv.DictReader(stream, delimiter=";") if "2024-02-01" <= row["bookingDate"] < "2024-03-01"]

if len(rows) != len(DESCRIPTIONS) or len(rows) != len(CATEGORIES):
    raise RuntimeError("Il numero dei movimenti di febbraio 2024 non coincide con la revisione approvata.")

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

(ROOT / "2024-02-approved.json").write_text(json.dumps(approved, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")
