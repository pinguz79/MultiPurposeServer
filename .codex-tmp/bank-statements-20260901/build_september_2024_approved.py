import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Versamento PayPal|Canone BNL|Canone Telepass Pay|Findomestic|Bonifico Michelle Minotta|Younited|Findomestic|Findomestic|Findomestic|Carta principale|Versamento PayPal|Agos|Assicurazione Casa|Vendita eBay|Cofidis|Spese conto|Conad|Servizio fotografico|Luce/Gas|Christian Canessa|Imposte e tasse|American Express|Parcheggio Piazza del Popolo|Agos|Agos|Fastweb|Polizza Reddito Protetto|Versamento PayPal|Ritiro Folletto|Stipendio|Canone Telepass Pay".split('|')
C="-|-|-|Prestiti|EntrateExtra|Prestiti|Prestiti|Prestiti|Prestiti|-|-|Prestiti|Casa|-|Prestiti|-|Spesa|EntrateExtra|Utenze|Subbuteo|Tasse|-|VitaSociale|Prestiti|Prestiti|Utenze|Polizze|-|Casa|Entrate|-".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2024-09-01'<=x['bookingDate']<'2024-10-01']
assert len(rows)==len(D)==len(C)==31
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2024-09-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
