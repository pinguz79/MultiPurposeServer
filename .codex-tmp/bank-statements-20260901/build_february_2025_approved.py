import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Georgina Piano|Canone BNL|Canone Telepass Pay|Eurospin|Findomestic|Younited|Versamento contanti|Georgina|Findomestic|Findomestic|Findomestic|Carta principale|Versamento PayPal|Scuola di ballo|Realista Digital|Prestito Findomestic|Prestito Findomestic|Agos|Cofidis|Assicurazione Casa|Vendita eBay|Vendita eBay|Vendita eBay|American Express|Agos|Agos|Versamento PayPal|Bonifico Ines|Fastweb|Polizza Reddito Protetto|Ritiro Folletto|Stipendio".split('|')
C="-|-|-|Spesa|Prestiti|Prestiti|-|-|Prestiti|Prestiti|Prestiti|-|-|Ballo|EntrateExtra|-|-|Prestiti|Prestiti|Casa|-|-|-|-|Prestiti|Prestiti|-|-|Utenze|Polizze|Casa|Entrate".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-02-01'<=x['bookingDate']<'2025-03-01']
assert len(rows)==len(D)==len(C)==32
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-02-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
