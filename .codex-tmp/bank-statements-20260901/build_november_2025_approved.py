import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Agos|Canone BNL|Younited|Versamento PayPal|Findomestic|Findomestic|Findomestic|Versamento PayPal|Prelievo|Multa|Carta principale|Adeguamento polizza M16460349|Versamento PayPal|Sorgenia|Cofidis|Affitto|Biglietto mamma Patricia|Prelievo|Internet|Compass|Telepass|Versamento PayPal|Versamento PayPal|American Express|Foto Marinetta|Versamento PayPal|Versamento contanti|Versamento contanti|Agos|Findomestic|Leonardo Da Vinci Auto|Bonifico|Restituzione prestito|Fastweb|Polizza Reddito Protetto|Stipendio|Finanziamento 2629956".split('|')
C="Prestiti|-|Prestiti|-|Prestiti|Prestiti|Prestiti|-|VitaSociale|Multe|-|Auto|-|Utenze|Prestiti|Casa|-|VitaSociale|Utenze|-|-|-|-|-|EntrateExtra|-|-|-|Prestiti|Prestiti|Auto|-|-|Utenze|Polizze|Entrate|Prestiti".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-11-01'<=x['bookingDate']<'2025-12-01']
assert len(rows)==len(D)==len(C)==37
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-11-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
