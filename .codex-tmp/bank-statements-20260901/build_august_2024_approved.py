import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Versamento PayPal|Canone BNL|Findomestic|Findomestic|Findomestic|Findomestic|Younited|Carta principale|Versamento PayPal|Prelievo|Agos|Assicurazione Casa|Spese conto|Cofidis|Servizio fotografico|American Express|Versamento contanti|Agos|Agos|Versamento PayPal|Versamento PayPal|Fastweb|Versamento PayPal|Vendita eBay|Polizza Reddito Protetto|Ritiro Folletto|Bonifico Martina Romeo|Cofidis|Stipendio".split('|')
C="-|-|Prestiti|Prestiti|Prestiti|Prestiti|Prestiti|-|-|VitaSociale|Prestiti|Casa|-|Prestiti|EntrateExtra|-|-|Prestiti|Prestiti|-|-|Utenze|-|-|Polizze|Casa|EntrateExtra|Prestiti|Entrate".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2024-08-01'<=x['bookingDate']<'2024-09-01']
assert len(rows)==len(D)==len(C)==29
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2024-08-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
