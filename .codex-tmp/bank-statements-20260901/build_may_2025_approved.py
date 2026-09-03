import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Canone BNL|Prelievo|Canone Telepass Pay|Findomestic|Findomestic|Findomestic|Findomestic|Younited|Carta principale|Bonifico Clementina Cedeno|Estinzione Findomestic|Alice Pizza|Nespresso|Agos|Assicurazione Casa|Stripe|Cofidis|Bonifico|Prestito Findomestic|Prestito Findomestic|Storno Agos|Stripe|Prelievo|American Express|Agos|Findomestic|Versamento PayPal|Stripe|Versamento PayPal|Fastweb|Versamento PayPal|Polizza Reddito Protetto|Polizza M16154241|Ritiro Folletto|Stipendio|Finanziamento 2539606".split('|')
C="-|VitaSociale|-|Prestiti|Prestiti|Prestiti|Prestiti|Prestiti|-|-|Prestiti|Spesa|Spesa|Prestiti|Casa|EntrateExtra|Prestiti|-|-|-|-|EntrateExtra|VitaSociale|-|Prestiti|Prestiti|-|EntrateExtra|-|Utenze|-|Polizze|Auto|Casa|Entrate|Prestiti".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-05-01'<=x['bookingDate']<'2025-06-01']
assert len(rows)==len(D)==len(C)==36
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-05-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
