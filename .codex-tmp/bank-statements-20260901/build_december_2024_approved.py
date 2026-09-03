import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Canone BNL|Canone Telepass Pay|Findomestic|Younited|Versamento PayPal|Prestito Findomestic|Versamento contanti|Findomestic|Findomestic|Findomestic|Bonifico Natale Anna La Rocca|Prestito Findomestic|Prestito Findomestic|Cart|Carta principale|Agos|Assicurazione Casa|Versamento PayPal|Bonifico Anna La Rocca|Cofidis|Versamento PayPal|Bonifico Occhipinti|Bonifico Maria Rita Congiu|Prelievo|Fratelli Carli|Ekom|Quintessenza|Tredicesima|Parcheggio Porto Antico|Versamento contanti|American Express|Agos|Agos|Fastweb|Polizza Reddito Protetto|Ritiro Folletto|Stipendio".split('|')
C="-|-|Prestiti|Prestiti|-|-|-|Prestiti|Prestiti|Prestiti|-|-|-|Spesa|-|Prestiti|Casa|-|-|Prestiti|-|SaluteBenessere|VitaSociale|VitaSociale|Spesa|Spesa|SaluteBenessere|Entrate|VitaSociale|-|-|Prestiti|Prestiti|Utenze|Polizze|Casa|Entrate".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2024-12-01'<=x['bookingDate']<'2025-01-01']
assert len(rows)==len(D)==len(C)==37
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2024-12-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
