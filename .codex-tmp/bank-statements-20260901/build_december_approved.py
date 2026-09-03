import csv
import json
from pathlib import Path


ROOT = Path(__file__).parent
DESCRIPTIONS = [
    "Acconto", "Canone BNL", "Findomestic", "Younited", "Versamento PayPal", "Pizzeria da Diego", "Eurospin", "Findomestic",
    "Findomestic", "Folletto", "Pizzeria Gomez", "Carta principale", "Edison", "Versamento PayPal", "Agos", "Cristian Huamani", "Prelievo",
    "Cofidis", "Edison", "Assicurazione Casa", "Prestito", "Affitto", "Primark", "Spese conto", "Assicurazione Auto", "Assicurazione Auto",
    "PayPal", "Prelievo", "American Express", "Tredicesima", "Prelievo", "Foto Martina", "Fastweb", "Foto", "Foto", "Foto sfilata",
    "Versamento PayPal", "Polizza Reddito Protetto", "Versamento contanti", "Ritiro Folletto", "Foto Greta", "Commissioni Telepass",
    "Stipendio", "Telepass",
]
CATEGORIES = [
    None, None, "Prestiti", "Prestiti", None, "VitaSociale", "Spesa", "Prestiti", "Prestiti", None, "Stadio", None, "Utenze", None,
    "Prestiti", None, "VitaSociale", "Prestiti", "Utenze", "Casa", None, "Casa", "Abbigliamento", None, "Auto", "Auto", None,
    "VitaSociale", None, "Entrate", "VitaSociale", "EntrateExtra", "Utenze", "EntrateExtra", "EntrateExtra", "EntrateExtra", None,
    "Polizze", None, "Casa", "EntrateExtra", None, "Entrate", None,
]

with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [row for row in csv.DictReader(stream, delimiter=";") if "2023-12-01" <= row["bookingDate"] < "2024-01-01"]

if len(rows) != len(DESCRIPTIONS) or len(rows) != len(CATEGORIES):
    raise RuntimeError("Il numero dei movimenti di dicembre non coincide con la revisione approvata.")

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

(ROOT / "2023-12-approved.json").write_text(json.dumps(approved, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")
