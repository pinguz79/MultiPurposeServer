import csv
import json
from pathlib import Path


ROOT = Path(__file__).parent
DESCRIPTIONS = [
    "Canone BNL", "Findomestic", "Liquidazione Key Technologies", "Giroconto", "Findomestic", "Findomestic", "Younited",
    "ADS Castagnolo Est", "Prelievo", "Carta principale", "Agos", "Assicurazione Casa", "Spese conto", "Cofidis", "Affitto",
    "Assicurazione Auto", "Assicurazione Auto", "Prelievo", "American Express", "Bonifico Anna La Rocca", "Prelievo",
    "Il Mondo Capovolto", "Fastweb", "Polizza Reddito Protetto", "Prelievo", "Ritiro Folletto", "Cofidis",
    "Commissioni Telepass", "Stipendio", "Telepass",
]
CATEGORIES = [
    None, "Prestiti", "Entrate", None, "Prestiti", "Prestiti", "Prestiti", "Auto", "VitaSociale", None, "Prestiti", "Casa", None,
    "Prestiti", "Casa", "Auto", "Auto", "VitaSociale", None, None, "VitaSociale", "Matrimonio", "Utenze", "Polizze", "VitaSociale",
    "Casa", "Prestiti", None, "Entrate", None,
]

with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [row for row in csv.DictReader(stream, delimiter=";") if "2023-11-01" <= row["bookingDate"] < "2023-12-01"]

if len(rows) != len(DESCRIPTIONS) or len(rows) != len(CATEGORIES):
    raise RuntimeError("Il numero dei movimenti di novembre non coincide con la revisione approvata.")

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

(ROOT / "2023-11-approved.json").write_text(json.dumps(approved, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")
