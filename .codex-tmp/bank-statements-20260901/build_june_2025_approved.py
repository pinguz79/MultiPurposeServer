import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Agos|Versamento PayPal|Canone BNL|Younited|Findomestic|Findomestic|Findomestic|Versamento PayPal|Stripe|Carta principale|Versamento PayPal|Stripe|Carta|Assicurazione Casa|Versamento PayPal|Versamento PayPal|Versamento PayPal|Prelievo|Cofidis|Telepass|Versamento contanti|Carta|Compass|Prelievo|Prelievo|Prestito Findomestic|Prestito Findomestic|American Express|Agos|Findomestic|Realista Digital|Regalo Martina|Fastweb|Shooting Genova|Polizza Reddito Protetto|Ritiro Folletto|Stipendio|Finanziamento 2539606".split('|')
C="Prestiti|-|-|Prestiti|Prestiti|Prestiti|Prestiti|-|EntrateExtra|-|-|EntrateExtra|-|Casa|-|-|-|VitaSociale|Prestiti|-|-|-|Prestiti|VitaSociale|VitaSociale|-|-|-|Prestiti|Prestiti|EntrateExtra|-|Utenze|EntrateExtra|Polizze|Casa|Entrate|Prestiti".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-06-01'<=x['bookingDate']<'2025-07-01']
assert len(rows)==len(D)==len(C)==38
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-06-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
