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
sami, pri svakoj promeni rute, i za njih ne treba ništa da se doda.

## Katalog događaja

Sva imena i parametri nalaze se u `events.ts`. Servis prihvata samo ime iz kataloga, pa
nepoznato ime ne prolazi kompajliranje.

| Događaj          | Kada se beleži                    | Parametri       |
| ---------------- | --------------------------------- | --------------- |
| `page_view`      | pri svakoj promeni rute (sam)     | `page_location` |
| `sign_up`        | posle uspešne registracije        | nema            |
| `login`          | posle uspešne prijave             | nema            |
| `tour_created`   | posle uspešnog kreiranja ture     | nema            |
| `tour_published` | posle uspešnog objavljivanja ture | nema            |

Nov događaj u katalog dodaje platformski tim, na zahtev tima modula. U zahtevu navedite
ime, trenutak kada se beleži i parametre. Pravila za ime i parametre:

- ime je oblika `<objekat>_<radnja u prošlom vremenu>`, malim slovima sa donjom crtom, na
  engleskom (`challenge_solved`); preporučena imena Google Analytics-a (`page_view`,
  `sign_up`, `login`) zadržavaju se;
- nijedan parametar ne nosi ime, mejl ni drugi lični podatak.

## Okruženja

ID za merenje (`G-XXXXXXXXXX`) čita se iz `src/environments/environment.ts`. U lokalnom
razvoju i u testovima koristi se `environment.development.ts`, gde ID ne postoji, pa se
ništa ne šalje. Baner se ipak prikazuje, da bi tok saglasnosti mogao da se isproba.

Da biste događaje videli u Google Analytics DebugView na probnom nalogu, privremeno
upišite ID u `environment.development.ts` i nemojte ga commit-ovati.
