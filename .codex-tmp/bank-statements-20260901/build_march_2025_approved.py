import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Versamento PayPal|Canone BNL|Canone Telepass Pay|Findomestic|Versamento contanti|Younited|Findomestic|Findomestic|Findomestic|Versamento contanti|Prestito|Agos|Cofidis|Assicurazione Casa|Versamento PayPal|Cart|Panificio Rustico|Ipercoop|Carta principale|Bonifico Giuseppe Gallo|Spese marzo|Prelievo|PayPal|PayPal|American Express|Versamento PayPal|Agos|Agos|Prestito Findomestic|Regalo Patrizia|Fastweb|Versamento PayPal|Polizza Reddito Protetto|Versamento contanti|Benzina|Versamento PayPal|Prelievo|Gidue|Ritiro Folletto|Stipendio".split('|')
C="-|-|-|Prestiti|-|Prestiti|Prestiti|Prestiti|Prestiti|-|-|Prestiti|Prestiti|Casa|-|Spesa|Spesa|Spesa|-|SpeseLegali|-|VitaSociale|-|-|-|-|Prestiti|Prestiti|-|-|Utenze|-|Polizze|-|Auto|-|VitaSociale|Auto|Casa|Entrate".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-03-01'<=x['bookingDate']<'2025-04-01']
assert len(rows)==len(D)==len(C)==40
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-03-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
