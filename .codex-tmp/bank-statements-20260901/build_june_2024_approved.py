import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Canone BNL|Canone Telepass Pay|Findomestic|Younited|Findomestic|Findomestic|Carta principale|Prelievo|Agos|Cofidis|Assicurazione Casa|Prelievo|Bonifico Claudio Vergano|Foto Ileana|Spese conto|Prelievo|Avvocato Fabio Mezzogori|Assicurazione Auto|Prelievo|Prelievo|Imposte e tasse|Spese bonifico|Visita Ericka|Polizia di Stato|American Express|Folletto|Agos|Agos|Versamento PayPal|Fastweb|Polizza Reddito Protetto|Ritiro Folletto|Stipendio".split('|')
C="-|-|Prestiti|Prestiti|Prestiti|Prestiti|-|VitaSociale|Prestiti|Prestiti|Casa|VitaSociale|Matrimonio|EntrateExtra|-|VitaSociale|SpeseLegali|Auto|VitaSociale|VitaSociale|Tasse|-|SaluteBenessere|Matrimonio|-|-|Prestiti|Prestiti|-|Utenze|Polizze|Casa|Entrate".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2024-06-01'<=x['bookingDate']<'2024-07-01']
assert len(rows)==len(D)==len(C)==33
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2024-06-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
