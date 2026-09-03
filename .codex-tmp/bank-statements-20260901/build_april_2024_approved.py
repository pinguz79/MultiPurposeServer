import csv,json
from pathlib import Path
R=Path(__file__).parent
D=["Canone BNL","Stipendio","Findomestic","Cofidis","Younited","Younited","Findomestic","Findomestic","Carta principale","Agos","Foto Erica","Mondo Risparmio","Assicurazione Casa","Cofidis","Spese conto","Assicurazione Auto","Mondo Risparmio","American Express","Prestito Agos","Estinzione Younited","Fastweb","Polizza Reddito Protetto","Ufficio postale","Ritiro Folletto","Cofidis","PayPal","PayPal","Canone Telepass Pay","Commissioni Telepass","Stipendio","Telepass","Prelievo"]
C=[None,"Entrate","Prestiti","Prestiti","Prestiti","Prestiti","Prestiti","Prestiti",None,"Prestiti","EntrateExtra","Spesa","Casa","Prestiti",None,"Auto","Spesa",None,None,"Prestiti","Utenze","Polizze",None,"Casa","Prestiti",None,None,None,None,"Entrate",None,"VitaSociale"]
with(R/'raw-movements.csv').open(encoding='utf-8-sig',newline='')as s: rows=[x for x in csv.DictReader(s,delimiter=';')if'2024-04-01'<=x['bookingDate']<'2024-05-01']
assert len(rows)==32
A=[{'requestId':i,'date':r['bookingDate'],'description':d,'formula':r['amount'].replace('.',','),'categoryName':c}for i,(r,d,c)in enumerate(zip(rows,D,C),1)]
(R/'2024-04-approved.json').write_text(json.dumps(A,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
