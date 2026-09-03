import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Agos|Canone BNL|Findomestic|Findomestic|Findomestic|Younited|Prestito amico|Versamento PayPal|Versamento PayPal|Internet|Carta principale|Versamento PayPal|Prestito Findomestic|Prestito Findomestic|Prestito|Prelievo|Prelievo|Foto Marinetta|Assicurazione Casa|Cofidis|Prelievo|Telepass|Compass|Prelievo|Biglietto mamma Patricia|Agos|Findomestic|American Express|Prelievo|Prelievo|Versamento PayPal|Prelievo|Bonifico Osorio Moreira|Fastweb|Prelievo|Polizza Reddito Protetto|Assicurazione Casa|Biglietto stadio Naya|Commissioni Telepass|Stipendio|Telepass".split('|')
C="Prestiti|-|Prestiti|Prestiti|Prestiti|Prestiti|-|-|-|Utenze|-|-|-|-|-|-|-|EntrateExtra|Casa|Prestiti|-|-|-|VitaSociale|-|Prestiti|Prestiti|-|VitaSociale|VitaSociale|-|VitaSociale|SaluteBenessere|Utenze|VitaSociale|Polizze|Casa|Stadio|-|Entrate|-".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-10-01'<=x['bookingDate']<'2025-11-01']
assert len(rows)==len(D)==len(C)==41
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-10-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
