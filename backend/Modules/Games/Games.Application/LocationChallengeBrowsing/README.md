# Igranje lokacijskih izazova

Istraživač na mapi koristi postojeći I21 simulator. Stranica učitava sačuvanu poziciju iz `/api/positions/mine`, a Games proverava udaljenost na serveru.

- Prag aktivacije je 50 m od početne lokacije. Okruženje ga može podesiti preko `Games__LocationChallenges__ActivationDistanceInMeters`. Games čuva jedan pokušaj po paru izazov–istraživač.
- Poluprečnik pretrage je rastojanje do najudaljenije skrivene tačke uvećano za nasumičan broj od 0 do 100 m. U postojećoj koloni `DiameterInMeters` već je sačuvan taj zbir, pa ga mapa sada koristi direktno kao poluprečnik.
- Istraživač dobija URL-ove foto-tragova iz Games API-ja tek kada postoji njegov uspešno aktiviran pokušaj. Odgovor za igranje izostavlja koordinate skrivenih tačaka. Administratorski pregled ostaje dostupan ulozi administratora.
- Foto-tragovi koriste postojeću javnu putanju `/images` sa nasumičnim nazivima. Zajednički `ImageFile` uklanja GPS i druge podržane metapodatke pri otpremanju i zadržava podatak o orijentaciji. Datoteke sačuvane pre te izmene treba zasebno obraditi, jer nova obrada važi pri otpremanju.
- Games proverava blizinu od najviše 5 m za tačku u sopstvenom pokušaju. Na poslednjoj tački zajedno čuva napredak, vreme završetka i dodeljeni XP.
