import csv,json
from pathlib import Path
R=Path(__file__).parent
D="Agos|Foto Marinetta e Parco delle Rose|Trasferimento|Foto Cinzia Panello|Canone BNL|Younited|Versamento PayPal|Foto Lorenzo Rossi|Prelievo|Findomestic|Findomestic|Findomestic|Stripe|Beccaria|Carta principale|PayPal|Versamento PayPal|Prestito Findomestic|Prestito Findomestic|Versamento contanti|PayPal|PayPal|PayPal|Telepass|Stripe|Prelievo|Cofidis|Assicurazione Casa|PayPal|Benzina|Eurospin|PayPal|Versamento PayPal|Parcheggio CityLife|Savona SS|L'Arte del Dolce|Compass|Prelievo|American Express|Rimborso PayPal|Agos|Findomestic|Prelievo|Recco SS|Fastweb|Versamento PayPal|Versamento PayPal|Polizza Reddito Protetto|Ritiro Folletto|Stipendio|Finanziamento 2539606".split('|')
C="Prestiti|EntrateExtra|-|EntrateExtra|-|Prestiti|-|EntrateExtra|VitaSociale|Prestiti|Prestiti|Prestiti|EntrateExtra|Auto|-|-|-|-|-|-|-|-|-|-|EntrateExtra|VitaSociale|Prestiti|Casa|-|Auto|Spesa|-|-|VitaSociale|-|VitaSociale|Prestiti|VitaSociale|-|-|Prestiti|Prestiti|VitaSociale|-|Utenze|-|-|Polizze|Casa|Entrate|Prestiti".split('|')
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s:rows=[x for x in csv.DictReader(s,delimiter=';')if'2025-08-01'<=x['bookingDate']<'2025-09-01']
assert len(rows)==len(D)==len(C)==51
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':None if c=='-' else c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2025-08-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
