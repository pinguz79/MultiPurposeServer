import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Agos|Servizio fotografico|Canone BNL|Versamento PayPal|Younited|Prestito Agos|Versamento contanti|Versamento contanti|Findomestic|Findomestic|Findomestic|Versamento PayPal|Eurospin|Risparmio Casa|Benzina|Hotel Continental|Bagni Vicini|Carta principale|Carta|PayPal|Foto Andrea Barettini|Assicurazione Casa|Cofidis|Versamento PayPal|Compass|Telepass|Prestito Findomestic|Prestito Findomestic|Versamento PayPal|Versamento PayPal|Prelievo|Agos|American Express|Findomestic|Versamento contanti|Fastweb|Polizza Reddito Protetto|Ritiro Folletto|Versamento contanti|Luce|Foto Graziella|Versamento PayPal|Commissioni Telepass|Stipendio|Telepass|Finanziamento 2539606".split('|')
C="Prestiti|EntrateExtra|-|-|Prestiti|-|-|-|Prestiti|Prestiti|Prestiti|-|Spesa|Spesa|Auto|Vacanze|Vacanze|-|-|-|EntrateExtra|Casa|Prestiti|-|Prestiti|-|-|-|-|-|VitaSociale|Prestiti|-|Prestiti|-|Utenze|Polizze|Casa|-|Utenze|EntrateExtra|-|-|Entrate|-|Prestiti".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-07-01'<=x['bookingDate']<'2025-08-01']
assert len(rows)==len(D)==len(C)==46
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-07-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
