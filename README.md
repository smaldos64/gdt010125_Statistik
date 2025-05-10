```
Dette Repository indeholder kode til at lave statistiske beregninger på både grupperede - og ugrupperede observationer.

Hvis man udelukkende ønsker at arbejde med et ugrupperet datasæt af observationer, laver man en tekstfil, som f.eks. kan hedde Ugrupperet.txt og anigver denne som inout fil, når man kører programmet.
Et eksempel på ugrupperet.txt er vist herunder:

Ugrupperet.txt
249
249
248
247
244
253
242
242
240
240
240
240
240
239
239
239
238
238
238
237

Hvis man kører programmet med filen herover som input tekstfil, vil programmet skrive alle beregnede statistiske parametre ud på skærmen samt lave en anden teksfil med navnet : Ugrupperet_Ugrupperet_Out.txt . 

De beregnede statistiske parametre indeholdt i Ugrupperet_Ugrupperet_Out.txt er vist herunder:

Ugrupperet_Ugrupperet_Out.txt

Ugrupperet Data Tabel
---------------------
Observation	Hyppighed h(x)	Summeret Hyppighed H(x)	Frekvens f(x)	Summeret Frekvns F(x)	Observation * hyppighed
	237		1		1		0,05		0,05			237
	238		3		4		0,15		0,20			714
	239		3		7		0,15		0,35			717
	240		5		12		0,25		0,60			1200
	242		2		14		0,10		0,70			484
	244		1		15		0,05		0,75			244
	247		1		16		0,05		0,80			247
	248		1		17		0,05		0,85			248
	249		2		19		0,10		0,95			498
	253		1		20		0,05		1,00			253

Ugrupperet Statistik
--------------------
Observationer * Hyppighed samlet       : 4842
Antal Observationer                    : 20
Middelværdi                            : 4842 / 20 = 242,10
Varians                                : 20,19
Standardafvigelse/Spredning            : 4,49

Minimums værdi                         : 237,00
Maksimums værdi                        : 253,00
Variationsbredde                       : 16,00
Typetal                                : 240,00
Antal gange Typetal forekommer         : 5

Kvartilsæt
----------
Nedre Kvartil                          : 239,00
Median                                 : 240,00
Øvre Kvartil                           : 245,50


Hvis man ønsker at arbejde med et grupperet datasæt af observationer, kan man gøre dette på 2 måder. Den første måde er at lave en tekstfil, som udelukkende indeholder intervaller og antal observationer i hvert af de angivne intervaller. Her skal man være opmærksom på, at man godt kan angive 0 som antal observationer i et eller flere intervaller, eller endda lade et eller flere intervaller være tomme for observationer (det samme som at angive 0 observationer i et givet interval). Et eksempel på et grupperet datasæt efter denne første måde er vist herunder i filen Grupperet_Metode1_LeftOpen.txt. 
Som default vil programmet regne med, at alle angive intervaller i den angivne tekstfil skal opfattes som værende med den nederste intervalgrænse værende lukket. 
Med udgangspunkt i filen Grupperet_Metode1_LeftOpen.txt herunder, betyder dette altså, at det første interval i denne skal opfattes som : [236 - 238[ . Vi kan også vælge at skrive linjen : 
INTERVALS: LEFT_OPEN 
et sted i vores tekstfil for at være sikre på, at programmet arbejder med lukkede nedre interval grænser og åbne øvre interval grænser. Dette er også angivet i Grupperet_Metode1_LeftOpen.txt herunder. Men det ændrer ikke ved programmets default mode. Men linjen kan være god at have med, hvis ikke man lige kan huske programmets default mode. 

Grupperet_Metode1_LeftOpen.txt
INTERVALS: LEFT_CLOSED
236-238-4
238-240-8
240-242-2
242-244-1
244-246-0
246-248-2
248-250-2
250-252-0
252-253-1

```
