import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Cofidis|Canone BNL|Canone Telepass Pay|Findomestic|Bonifico Occhipinti|Servizio fotografico|Bonifico Ericka|Younited|Findomestic|Findomestic|Agos|Pietro Debenedetti|Gelateria Margot|Risparmio Casa|Carta principale|Assicurazione Casa|Luce/Gas|Cofidis|Primark|Spese conto|Luce/Gas|Prelievo|Assicurazione Auto|Mondial Food Market|Anna Cameirano|Officina del Gioiello|Imposte e tasse|Prelievo|American Express|Agos|Agos|Giroconto|Prestito Findomestic|Prestito Findomestic|Fastweb|Polizza Reddito Protetto|Ritiro Folletto|Cofidis|Commissioni Telepass|Stipendio|Telepass|Canone Telepass Pay".split('|')
C="Prestiti|-|-|Prestiti|SaluteBenessere|EntrateExtra|Casa|Prestiti|Prestiti|Prestiti|Prestiti|Casa|VitaSociale|Spesa|-|Casa|Utenze|Prestiti|Abbigliamento|-|Utenze|VitaSociale|Auto|Spesa|Abbigliamento|Abbigliamento|Tasse|VitaSociale|-|Prestiti|Prestiti|-|-|-|Utenze|Polizze|Casa|Prestiti|-|Entrate|-|-".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2024-07-01'<=x['bookingDate']<'2024-08-01']
assert len(rows)==len(D)==len(C)==42
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2024-07-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
