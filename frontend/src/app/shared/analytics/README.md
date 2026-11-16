# Analitika korišćenja (`shared/analytics`)

Aplikacija beleži preglede stranica i nekoliko važnih događaja u Google Analytics 4, da bi
tim modula video gde korisnici odustaju u njegovom delu aplikacije. Beleži se samo kada je
posetilac to prihvatio na baneru ispod zaglavlja. Izbor može da promeni u svakom trenutku,
preko linka „Cookie settings” u podnožju.

Timovi modula ne uče Google Analytics. Koriste samo servis `Analytics` i njegovu metodu
`track`, opisanu ispod. Nijedan modul ne poziva Google Analytics direktno.

## Kako se beleži događaj

Događaj se beleži u servisu grupe slučajeva korišćenja, odmah posle uspešne komande. Ako
komanda ne uspe, `await` baci grešku, pa se događaj ne beleži.

```ts
@Injectable({ providedIn: 'root' })
export class TourAuthoring {
  private readonly http = inject(HttpClient);
  private readonly analytics = inject(Analytics);

  async publish(id: string, dto: PublishTourDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, dto));
    this.analytics.track('tour_published', {});
  }
}
```

Servis sam proverava saglasnost i dodaje ulogu korisnika (`guest`, `explorer` ili
`administrator`), pa poziv nema `if` i ne prosleđuje ulogu. Pregledi stranica se beleže
sami, pri svakoj promeni rute, i za njih ne treba ništa da se doda. Autor ture nije
posebna uloga: ture pravi istraživač, pa `tour_created` i `tour_published` stižu sa
ulogom `explorer`.

## Katalog događaja

Sva imena i parametri nalaze se u `events.ts`. Servis prihvata samo ime iz kataloga, pa
nepoznato ime ne prolazi kompajliranje.

| Događaj | Kada se beleži | Parametri |
| ---------------- | ---------------------------------
| `page_view` | pri svakoj promeni rute (sam) | `page_location` |
| `sign_up` | posle uspešne registracije | nema |
| `login` | posle uspešne prijave | nema |
| `tour_created` | posle uspešnog kreiranja ture | nema |
| `tour_published` | posle uspešnog objavljivanja ture | nema |

Nov događaj u katalog dodaje platformski tim, na zahtev tima modula. U zahtevu navedite
ime, trenutak kada se beleži i parametre. Pravila za ime i parametre:

- ime je oblika `<objekat>_<radnja u prošlom vremenu>`, malim slovima sa donjom crtom, na
  engleskom (`challenge_solved`); preporučena imena Google Analytics-a (`page_view`,
  `sign_up`, `login`) zadržavaju se;
- nijedan parametar ne nosi ime, mejl ni drugi lični podatak.

## Okruženja

Produkcioni build (`npm run build`) čita ID za merenje tima iz
`src/environments/environment.ts`. Lokalni razvoj (`npm start`) i testovi koriste
`environment.development.ts`, gde ID ne postoji, pa se ništa ne šalje i podaci tima ostaju
čisti. Baner se ipak prikazuje, da bi tok saglasnosti mogao da se isproba.

## Provera rada na nalogu tima

Događaji se šalju u Google Analytics nalog tima, na property čiji je ID za merenje u
`environment.ts`. Podatke za prijavu na nalog daje platformski tim.

### 1. Pristup

1. Prijavite se na analytics.google.com nalogom tima.
2. U biraču gore levo izaberite property tima. Njegov ID za merenje (**Admin → Data
   streams**) mora da bude isti kao u `environment.ts`.
3. Funnel „Od ideje do objavljene ture” nalazi se u **Explore**.

### 2. Priprema

1. Pokrenite server i klijent kao i obično (`npm start` u `frontend`).
2. Privremeno upišite ID tima iz `environment.ts` u `environment.development.ts`:
   `gaMeasurementId: '<ID iz environment.ts>' as string | null`. Ovu izmenu **ne commit-ujte**.
3. Isključite blokator reklama za `localhost` (u Brave-u: ikonica lava → Shields down). Inače
   blokator zameni skriptu Google Analytics-a praznom, pa se ništa ne šalje.
4. Otvorite alat za programere (F12) → **Network**, uključite **Preserve log** i u filter
   upišite `collect`. Događaji idu u zahtevima `…/g/collect?…` sa statusom 204; jedan zahtev
   može da nosi više događaja, a ime događaja je u parametru `en`.
5. U **Application → Local storage** obrišite `explorer.analytics-consent`, pa osvežite
   stranicu. Tako počinjete kao posetilac koji još nije izabrao.

### 3. Šta proveriti

| Korak                                                           | Očekivano                                                                              |
| --------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| Otvorite bilo koju stranicu i ne birajte ništa                  | Baner ispod zaglavlja; nema zahteva `collect` ni skripte `googletagmanager`            |
| Pređite na drugu stranicu bez izbora                            | Baner ostaje, i dalje se ništa ne šalje                                                |
| **Accept**                                                      | Baner nestaje; učitava se `gtag/js`, a u `collect` stiže `en=page_view`                |
| Prelazite između stranica                                       | Za svaki prelaz po jedan `page_view`                                                   |
| Pogledajte parametre zahteva (adresu ili payload)               | `ep.role=guest` dok niste prijavljeni                                                  |
| Prijavite se kao `explorer1@explorer.dev`                       | `en=login` sa `ep.role=explorer`                                                       |
| Otvorite `/exploration/create` i napravite turu                 | `page_view` za formu, pa `en=tour_created`                                             |
| Na `/exploration/mine` turi dodajte vreme prevoza i objavite je | `en=tour_published`                                                                    |
| Odjavite se i prijavite kao `admin1@explorer.dev`               | `en=login` sa `ep.role=administrator`                                                  |
| **Cookie settings** u podnožju                                  | Baner se ponovo vidi, sa porukom o trenutnom izboru                                    |
| **Decline**                                                     | Nema više zahteva `collect`; u **Application → Cookies** nema kolačića `_ga` i `_ga_…` |
| Osvežite stranicu                                               | Baner se ne vraća, `gtag/js` se ne učitava                                             |

Tura može da se objavi samo ako ima opis od bar 100 znakova i bar jedno vreme prevoza.
Administrator ne može da pravi ture, pa tok sa turom prolazite kao istraživač.

Nijedan zahtev ne sme da sadrži mejl, ime ili drugi lični podatak.

### 4. Provera u Google Analytics-u

- **Reports → Realtime**: događaji se vide posle oko minut. Na kartici „Event count by Event
  name” kliknite događaj, pa parametar `role`, da vidite vrednosti po ulogama.
- **Explore → „Od ideje do objavljene ture”**: podaci stižu tek posle obrade, što obično traje
  nekoliko sati, a može i do dva dana. Opseg datuma postavite na **Last 7 days**; „Today”
  ne obuhvata podatke od juče.
- `first_visit` i `session_start` dodaje sam Google Analytics, ne naš kod. Ako se pojave
  `scroll`, `form_start` ili slični događaji, u podešavanjima toka podataka (**Admin → Data
  streams**) uključeno je **Enhanced measurement**, koje treba da bude isključeno.

### 5. Kraj

Vratite `environment.development.ts` i proverite da nema izmena:

```
git restore frontend/src/environments/environment.development.ts
git status --short
```

Ponovo uključite blokator reklama za `localhost`.

### Kada nešto ne radi

| Simptom | Uzrok |
| ------------------------------------------------------
| Nema zahteva `gtag/js` posle **Accept** | ID je `null` u `environment.development.ts`; upišite ga i osvežite stranicu |
| `gtag/js` vraća 307 ka `google-analytics_analytics.js` | Blokator reklama je još uključen (Brave) |
| `collect` se šalje, a Realtime je prazan | Izabran je pogrešan property; uporedite ID u Google Analytics-u i u `environment.ts` |
| Funnel je prazan, a Realtime nije | Opseg datuma ne obuhvata dan provere, ili podaci još nisu obrađeni |
| Funnel ne broji tok kao jednog korisnika | Tok nije prošao u istom pregledaču, ili je u međuvremenu kliknuto **Decline**, što briše kolačić `_ga` |
