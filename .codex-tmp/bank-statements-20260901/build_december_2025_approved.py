import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Agos|Canone BNL|Prelievo|Prelievo|Mercato Corso Sardegna|Younited|Findomestic|Findomestic|Findomestic|Dr. Max|Cofidis|Prelievo|Carta principale|Affitto|Prestito amico|Versamento PayPal|Prelievo|Telepass|Tredicesima|Fattorie Garofalo|Pagamento CBILL|American Express|Sorgenia|Agos|Findomestic|Versamento PayPal|Fastweb|Assicurazione Casa|Prestito amico|Prelievo|Polizza Reddito Protetto|PayPal|Prelievo|Finanziamento 2629956|Stipendio".split('|')
C="Prestiti|-|VitaSociale|-|Spesa|Prestiti|Prestiti|Prestiti|Prestiti|SaluteBenessere|Prestiti|VitaSociale|-|Casa|-|-|-|-|Entrate|Spesa|Utenze|-|Utenze|Prestiti|Prestiti|-|Utenze|Casa|-|VitaSociale|Polizze|-|-|Prestiti|Entrate".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-12-01'<=x['bookingDate']<'2026-01-01']
assert len(rows)==len(D)==len(C)==35
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-12-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
