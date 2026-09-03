import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Canone BNL|Findomestic|Servizio fotografico|Younited|Versamento PayPal|Prestito Findomestic|Findomestic|Findomestic|Findomestic|Foto Francesca Cordas|Versamento PayPal|Carta principale|Agos|Foto evento|Prelievo|Prelievo|Assicurazione Casa|Servizio fotografico|Spese conto|Cofidis|Luce/Gas|Foto Selene Nosenzo|Prestito Findomestic|Versamento PayPal|Agos|Agos|American Express|Versamento contanti|Versamento PayPal|Fastweb|Polizza Reddito Protetto|Ritiro Folletto|Versamento PayPal|Commissioni Telepass|Stipendio|Telepass".split('|')
C="-|Prestiti|EntrateExtra|Prestiti|-|-|Prestiti|Prestiti|Prestiti|EntrateExtra|-|-|Prestiti|EntrateExtra|VitaSociale|VitaSociale|Casa|EntrateExtra|-|Prestiti|Utenze|EntrateExtra|-|-|Prestiti|Prestiti|-|-|-|Utenze|Polizze|Casa|-|-|Entrate|-".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2024-10-01'<=x['bookingDate']<'2024-11-01']
assert len(rows)==len(D)==len(C)==36
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2024-10-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
