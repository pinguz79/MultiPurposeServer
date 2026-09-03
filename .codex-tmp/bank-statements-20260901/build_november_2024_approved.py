import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Servizio fotografico|Canone BNL|Canone Telepass Pay|Findomestic|Versamento contanti|Versamento contanti|Findomestic|Findomestic|Younited|Carta principale|Versamento PayPal|Agos|Cofidis|Assicurazione Casa|Prelievo|Foto Orian Bucci|Prelievo|Prelievo|American Express|Agos|Agos|Versamento PayPal|Fastweb|Prelievo|Polizza Reddito Protetto|Ritiro Folletto|Stipendio".split('|')
C="EntrateExtra|-|-|Prestiti|-|-|Prestiti|Prestiti|Prestiti|-|-|Prestiti|Prestiti|Casa|VitaSociale|EntrateExtra|VitaSociale|VitaSociale|-|Prestiti|Prestiti|-|Utenze|VitaSociale|Polizze|Casa|Entrate".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2024-11-01'<=x['bookingDate']<'2024-12-01']
assert len(rows)==len(D)==len(C)==27
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2024-11-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
