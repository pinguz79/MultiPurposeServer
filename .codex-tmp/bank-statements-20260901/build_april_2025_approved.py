import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Canone BNL|Findomestic|Versamento PayPal|Canone Telepass Pay|Versamento contanti|Younited|Findomestic|Findomestic|Findomestic|Versamento PayPal|Versamento PayPal|Carta principale|Bonifico|Agos|Prestito Findomestic|Prestito Findomestic|Assicurazione Casa|Versamento PayPal|Cofidis|Versamento PayPal|Ballo|Olio|Versamento PayPal|Versamento PayPal|Prelievo|Officina del Gioiello|Pratiche visto Mero Cedeno|Spese|PayPal|Versamento PayPal|Debiti|Versamento PayPal|Agos|Agos|American Express|Versamento PayPal|Versamento PayPal|Versamento contanti|Fastweb|Polizza Reddito Protetto|Ritiro Folletto|Prestito Agos|Prestito Findomestic|Commissioni Telepass|Stipendio|Telepass|Finanziamento 2539606|Saldo finale".split('|')
C="-|Prestiti|-|-|-|Prestiti|Prestiti|Prestiti|Prestiti|-|-|-|-|Prestiti|-|-|Casa|-|Prestiti|-|Ballo|Spesa|-|-|VitaSociale|VitaSociale|-|-|-|-|-|-|Prestiti|Prestiti|-|-|-|-|Utenze|Polizze|Casa|-|-|-|Entrate|-|Prestiti|-".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-04-01'<=x['bookingDate']<'2025-05-01']
assert len(rows)==len(D)==len(C)==48
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-04-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
