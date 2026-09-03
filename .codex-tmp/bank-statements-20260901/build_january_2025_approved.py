import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Rimborso spesa|Canone BNL|Interessi|Canone Telepass Pay|Findomestic|Versamento PayPal|Findomestic|Findomestic|Findomestic|Younited|Versamento PayPal|Versamento contanti|Bonifico|Agos|PayPal|Carta principale|Bonifico Patricia|Assicurazione Casa|Parcheggio Porto Antico|Cofidis|Agos|Agos|American Express|Prelievo|Versamento PayPal|Parcella avvocato|Spesa|Fastweb|Polizza Reddito Protetto|Ritiro Folletto|Commissioni Telepass|Stipendio|Prelievo|Telepass".split('|')
C="-|-|-|-|Prestiti|-|Prestiti|Prestiti|Prestiti|Prestiti|-|-|-|Prestiti|-|-|-|Casa|VitaSociale|Prestiti|Prestiti|Prestiti|-|VitaSociale|-|SpeseLegali|Spesa|Utenze|Polizze|Casa|-|Entrate|VitaSociale|-".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-01-01'<=x['bookingDate']<'2025-02-01']
assert len(rows)==len(D)==len(C)==34
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-01-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
