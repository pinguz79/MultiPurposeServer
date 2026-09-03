import csv, json
from pathlib import Path
ROOT = Path(__file__).parent
DESCRIPTIONS = ["Canone BNL","Findomestic","Younited","Younited","Interessi","Prelievo","Spese bonifico","Bonifico Daniele Mengarelli","Bonifico Ericka","Carta","Findomestic","Findomestic","Carta principale","Agos","Prelievo","Spese conto","Cofidis","Assicurazione Casa","Foto Martina","Foto Alessio Russo","PayPal","Versamento PayPal","Prestito Findomestic","Assicurazione Auto","Versamento PayPal","Burger King","Mondo Risparmio","American Express","Foto sfilata","Pacchetto fotografico","Versamento PayPal","Acconto fotografo","Luce/Gas","Fastweb","Versamento PayPal","Polizza Reddito Protetto","Prelievo","Vendita eBay","Farmacia Dr. Max","Eurospin","Ritiro Folletto","Commissioni Telepass","Rateo Spese","Telepass"]
CATEGORIES = [None,"Prestiti","Prestiti","Prestiti",None,"VitaSociale",None,"Matrimonio","Casa",None,"Prestiti","Prestiti",None,"Prestiti","VitaSociale",None,"Prestiti","Casa","EntrateExtra","EntrateExtra",None,None,None,"Auto",None,"VitaSociale","Spesa",None,"EntrateExtra","EntrateExtra",None,"EntrateExtra","Utenze","Utenze",None,"Polizze","VitaSociale","EntrateExtra","SaluteBenessere","Spesa","Casa",None,"Prestiti",None]
with (ROOT / "raw-movements.csv").open(encoding="utf-8-sig", newline="") as stream:
    rows = [r for r in csv.DictReader(stream, delimiter=";") if "2024-03-01" <= r["bookingDate"] < "2024-04-01"]
if len(rows) != 44: raise RuntimeError("Conteggio marzo inatteso")
approved = [{"requestId":i,"date":r["bookingDate"],"description":d,"formula":r["amount"].replace(".",","),"categoryName":c} for i,(r,d,c) in enumerate(zip(rows,DESCRIPTIONS,CATEGORIES),1)]
(ROOT / "2024-03-approved.json").write_text(json.dumps(approved,ensure_ascii=False,indent=2)+"\n",encoding="utf-8",newline="")
