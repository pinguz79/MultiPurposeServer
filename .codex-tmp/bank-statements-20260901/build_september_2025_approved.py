import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Agos|Canone BNL|Versamento PayPal|Younited|Prestito|Prestito|Servizi fotografici|Findomestic|Findomestic|Findomestic|Carta principale|Versamento PayPal|Prelievo|Prelievo|Prelievo|Assicurazione Casa|Versamento PayPal|Cofidis|PayPal|Telepass|Trasferimento|Foto Graziella|Prestito Findomestic|Prestito Findomestic|Foto Cinzia Panello|Prelievo|Compass|Servizio fotografico|Versamento PayPal|Versamento contanti|Affitto ottobre e deposito|Biglietto mamma Patricia|Anticipo casa|Provvigione locazione|Versamento contanti|Benzina|American Express|Foto Sergio Ceccon|Agos|Findomestic|Servizio fotografico|Versamento contanti|Versamento contanti|Fastweb|Polizza Reddito Protetto|Versamento PayPal|Prelievo|Stipendio|Finanziamento 2539606".split('|')
C="Prestiti|-|-|Prestiti|-|-|EntrateExtra|Prestiti|Prestiti|Prestiti|-|-|-|VitaSociale|-|Casa|-|Prestiti|-|-|-|EntrateExtra|-|-|EntrateExtra|VitaSociale|Prestiti|EntrateExtra|-|-|Casa|-|-|Casa|-|Auto|-|EntrateExtra|Prestiti|Prestiti|EntrateExtra|-|-|Utenze|Polizze|-|VitaSociale|Entrate|Prestiti".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-09-01'<=x['bookingDate']<'2025-10-01']
assert len(rows)==len(D)==len(C)==49
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-09-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
