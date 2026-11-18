# Mapa lokacija (`shared/map`)

`LocationMap` (`app-location-map`) je prikazna komponenta platforme koja prikazuje listu
lokacija na mapi. Svaka lokacija je jedan pin, a klik na pin prikazuje njen naziv pored
pina. Svi pinovi su uvek u vidnom polju. Kada je putanja uključena, pinovi su povezani
rutom koja prati pešačke puteve, redom kojim su predati, a komponenta šalje dužinu putanje u
kilometrima. Kada je
izbor uključen, korisnik klikom na mapu bira jednu lokaciju, a stranica dobija njene
koordinate. Mapa još crta poziciju turiste, pešačku rutu od nje do sledeće tačke i krug zadat
centrom i poluprečnikom u metrima.

Mapa koristi biblioteku Leaflet i podlogu OpenStreetMap, besplatno i bez ključa. Rute traži od
javnog OSRM servera zajednice OpenStreetMap (`https://routing.openstreetmap.de/routed-foot`),
takođe besplatno i bez ključa: komponenta tom spoljnom servisu šalje koordinate svake dve
uzastopne lokacije. Presretač iz `core/auth` dodaje token samo zahtevima ka `/api/`, pa token
prijavljenog korisnika ne odlazi tom servisu. Timovi modula ne uče Leaflet ni servis za
rutiranje: koriste samo ulaze i izlaze opisane ispod.

Živi primer je stranica `/design-preview/map` u stilskom vodiču (`core/design-preview/map-view/`).

## Ulazi i izlazi

| Ime                | Vrsta                       | Značenje                                                                               |
| ------------------ | --------------------------- | -------------------------------------------------------------------------------------- |
| `locations`        | ulaz, obavezan              | `MapLocation[]`: `latitude`, `longitude` i `name` svake lokacije                       |
| `showPath`         | ulaz, podrazumevano `false` | da li ruta spaja lokacije redom kojim su predate                                       |
| `pathLengthChange` | izlaz, `number`             | dužina putanje u km, šalje se dok je `showPath` uključen, kada stignu rute svih delova |
| `selectable`       | ulaz, podrazumevano `false` | da li klik na mapu bira lokaciju                                                       |
| `selected`         | ulaz, podrazumevano `null`  | `MapCoordinates \| null`: izabrani pin, ako postoji                                    |
| `selectedChange`   | izlaz, `MapCoordinates`     | koordinate mesta na koje je korisnik kliknuo, dok je izbor uključen                    |
| `position`         | ulaz, podrazumevano `null`  | `MapCoordinates \| null`: pozicija turiste, ako je postavljena                         |
| `locationClick`    | izlaz, `number`             | pozicija u `locations` pina na koji je korisnik kliknuo                                |
| `target`           | ulaz, podrazumevano `null`  | `MapCoordinates \| null`: sledeća tačka; uz `position` mapa crta rutu do nje           |
| `area`             | ulaz, podrazumevano `null`  | `MapArea \| null`: krug sa centrom `latitude`, `longitude` i poluprečnikom `radiusMeters` |

Dužina putanje je zbir dužina pešačkih ruta po putevima između uzastopnih lokacija;
poslednja lokacija nije spojena sa prvom. Za svaki deo putanje (dve uzastopne lokacije)
komponenta šalje po jedan zahtev servisu za rutiranje. Deo za koji servis ne odgovori ili ne
nađe rutu crta se pravom linijom i ulazi u dužinu pravolinijskom razdaljinom; ostali delovi
prate puteve, a posebne poruke nema. Zahtev koji ne dobije odgovor za 5 sekundi smatra se
neuspelim, pa servis koji visi ne ostavlja stranicu zauvek na "računa se…". Po jedan zahtev
po delu, a ne jedan za celu putanju, je svesna odluka: pad servisa za jedan deo menja pravom
linijom samo taj deo. Za manje od dve lokacije dužina je 0 km i servis se ne
pita.

Rute se traže samo dok je `showPath` uključen, i to jednom za svaku listu lokacija: dok ne
stignu svi delovi, na mapi su samo pinovi, bez linije, a `pathLengthChange` se šalje tek kada
su stigli svi. Linija je u svom sloju, pa njeno iscrtavanje ne pomera vidno polje koje je
korisnik u međuvremenu namestio. Odgovor za stariju listu lokacija se odbacuje. Isključivanje i
ponovno uključivanje `showPath` za istu listu ne traži rute ponovo i ne menja dužinu. Dok je
`showPath` isključen, komponenta ne pita servis za rutiranje i ne šalje dužinu; stranica koja
želi dužinu uključuje putanju.

Stranica koja prikazuje dužinu čuva je kao `number | null` i vraća je na `null` kada menja
lokacije; dok je `null`, prikazuje da se dužina računa. Ako predaje istu listu kao i ranije,
komponenta ne šalje novu dužinu, pa je ne treba ni brisati.

Prazna lista prikazuje mapu Novog Sada bez pinova, bez greške.

Tip `MapLocation` izvozi `location-map.ts`. Stranica modula pretvara svoje DTO tipove u
`MapLocation` pre predaje komponenti.

## Upotreba: izazovi u gradu, bez putanje

```ts
import { LocationMap, MapLocation } from '../../../../shared/map/location-map';

@Component({ imports: [LocationMap] /* ... */ })
export class ChallengeMap {
  protected readonly challenges = signal<MapLocation[]>([
    { latitude: 45.2551, longitude: 19.8452, name: 'Kviz na Trgu slobode' },
    { latitude: 45.2517, longitude: 19.8618, name: 'Skrivena lokacija na tvrđavi' },
  ]);
}
```

```html
<app-location-map [locations]="challenges()" />
```

## Upotreba: ključne tačke ture, sa putanjom i dužinom

```ts
export class TourMap {
  protected readonly keyPoints = signal<MapLocation[]>([
    { latitude: 45.2655, longitude: 19.8293, name: '1. Železnička stanica' },
    { latitude: 45.2551, longitude: 19.8452, name: '2. Trg slobode' },
    { latitude: 45.2517, longitude: 19.8618, name: '3. Petrovaradinska tvrđava' },
  ]);
  // null dok mapa ne pošalje dužinu po putevima.
  protected readonly pathLength = signal<number | null>(null);
  protected readonly pathLengthText = computed(() => {
    const length = this.pathLength();
    return length === null ? 'računa se…' : `${length.toFixed(2)} km`;
  });
}
```

```html
<app-location-map
  [locations]="keyPoints()"
  [showPath]="true"
  (pathLengthChange)="pathLength.set($event)"
/>
<p>Dužina putanje: {{ pathLengthText() }}</p>
```

## Upotreba: izbor lokacije u formi

Kada je `selectable` uključen, svaki klik na mapu šalje `selectedChange` sa koordinatama tog
mesta: `latitude` i `longitude` u decimalnim stepenima (tip `MapCoordinates` izvozi
`location-map.ts`). Komponenta sama ne pomera pin: izabrani pin je uvek ono što stranica
preda kroz `selected`. Stranica zato čuva izabranu lokaciju u svojoj formi, upisuje u nju
vrednost iz `selectedChange` i vraća je mapi kroz `selected`. Na mapi je najviše jedan
izabrani pin; prikazuje se kao krug i razlikuje se od pinova iz `locations`, koji se ne
pomeraju. Izabrani pin ne može da se ukloni, samo da se premesti.

Za izmenu postojećeg entiteta stranica u formu upiše sačuvanu lokaciju, pa mapa počinje sa
njom kao izabranim pinom. Proveru da li je lokacija izabrana radi forma stranice, običnim
`required` validatorom, a ne komponenta. Kada `selectable` nije uključen, klik na mapu ne
radi ništa.

```ts
import { FormField, form, required } from '@angular/forms/signals';
import { LocationMap, MapCoordinates, MapLocation } from '../../../../shared/map/location-map';

@Component({ imports: [FormField, LocationMap] /* ... */ })
export class CreateChallenge {
  protected readonly otherChallenges = signal<MapLocation[]>([
    { latitude: 45.2517, longitude: 19.8618, name: 'Skrivena lokacija na tvrđavi' },
  ]);

  protected readonly form = form(
    signal({ name: '', location: null as MapCoordinates | null }),
    (path) => {
      required(path.name, { message: 'Naziv je obavezan.' });
      required(path.location, { message: 'Izaberi lokaciju na mapi' });
    },
  );
}
```

```html
<app-location-map
  [locations]="otherChallenges()"
  [selectable]="true"
  [selected]="form.location().value()"
  (selectedChange)="form.location().value.set($event)"
/>
@for (message of form.location().errors(); track $index) {
<p class="error">{{ message.message }}</p>
}
```

Pri slanju komande stranica uzima `form.location().value()` i šalje njene `latitude` i
`longitude` zajedno sa ostalim poljima forme. Sekcija "Izbor lokacije" na `/design-preview/map`
pokazuje samu mapu za izbor, sa novom lokacijom i izmenom postojeće; forma iz ovog primera
je posao stranice modula.

## Upotreba: simulator pozicije

Ulaz `position` crta oznaku pozicije turiste: malu tamnu tačku sa svetlim okvirom, koja se
razlikuje i od pinova iz `locations` i od izabranog kruga. Kao i izabrani pin, oznaku pomera
samo stranica, kroz ulaz; promena ulaza ne menja vidno polje. Za `null` oznake nema.

Klik se hvata postojećim `selectable` i `selectedChange`. Stranica simulatora ne postavlja
`selected`, pa klik na njenoj mapi znači samo postavljanje pozicije, nikad i izbor lokacije.
Stranica šalje koordinate komandom servisa iz `shared/position` i posle uspeha ponovo učita
svoj resurs, pa se oznaka pomera tek kada server sačuva poziciju (vidi README u
`shared/position`).

```ts
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { LocationMap, MapCoordinates } from '../../../../shared/map/location-map';
import { PositionSimulation } from '../../../../shared/position/position-simulation';

@Component({ imports: [LocationMap] /* ... */ })
export class TourRun {
  private readonly positionSimulation = inject(PositionSimulation);
  // null kada pozicija nije postavljena (server odgovara sa 204).
  protected readonly position = httpResource<CoordinatesDto | null>(() => '/api/positions/mine');
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async setPosition(coordinates: MapCoordinates): Promise<void> {
    // komanda, pa this.position.reload(), kao u README-u iz shared/position
  }
}
```

```html
<app-location-map
  [locations]="keyPoints()"
  [selectable]="true"
  [position]="position.value() ?? null"
  (selectedChange)="setPosition($event)"
/>
```

Sekcija "Simulator pozicije" na `/design-preview/map` pokazuje ceo tok, sa porukom ispod mape
i stanjima stranice.

## Upotreba: ruta od pozicije do sledeće tačke

Kada stranica preda i `position` i `target`, mapa nacrta pešačku rutu po putevima od pozicije
do te tačke. Rutu traži od istog servisa za rutiranje kao putanju ture, istim zahtevom za jedan
deo: zahtev bez odgovora za 5 sekundi, ili bez nađene rute, daje pravu liniju od pozicije do
tačke, bez poruke. Ruta se ponovo traži posle svake promene `position` ili `target`; dok nova ne
stigne, stara se ne vidi, a odgovor za stariju poziciju ili tačku se odbacuje. Dok je `position`
ili `target` `null`, servis se ne pita i rute nema. Iscrtavanje rute ne menja vidno polje.

Stranica bira sledeću tačku sama, na primer klikom na pin kroz `locationClick`, a poziciju čita
sa servera svojim resursom, kao u simulatoru. Klik na pin ne stiže do mape, pa ne pomera
poziciju: jedan klik nikad ne znači dve stvari.

```ts
protected readonly nextIndex = signal<number | null>(null);
protected readonly nextPoint = computed(() => {
  const index = this.nextIndex();
  return index === null ? null : this.keyPoints()[index];
});
```

```html
<app-location-map
  [locations]="keyPoints()"
  [selectable]="true"
  [position]="position.value() ?? null"
  [target]="nextPoint()"
  (selectedChange)="setPosition($event)"
  (locationClick)="nextIndex.set($event)"
/>
```

`MapLocation` ima sva polja koja `MapCoordinates` traži, pa se ključna tačka predaje kroz
`target` bez pretvaranja. Sekcija "Ruta do sledeće tačke" na `/design-preview/map` pokazuje ceo
tok, sa porukama ispod mape i stanjima stranice.

## Upotreba: oblast na mapi

Ulaz `area` crta krug oko centra, sa poluprečnikom u metrima (tip `MapArea` izvozi
`location-map.ts`). Krug je u metrima, a ne u pikselima, pa pri zumiranju ostaje iste veličine u
odnosu na ulice. Krug ne reaguje na klik i ne pomera se klikom; pomera ga samo stranica, kroz
ulaz, a promena ulaza ne menja vidno polje. Za `null` kruga nema.

```ts
protected readonly area = signal<MapArea | null>({
  latitude: 45.2551,
  longitude: 19.8452,
  radiusMeters: 500,
});
```

```html
<app-location-map [locations]="challenges()" [area]="area()" />
```

Sekcija "Oblast na mapi" na `/design-preview/map` pokazuje oblast pretrage (500 m oko Trga
slobode) i manju oblast traženja oko skrivene lokacije.

## Funkcionalnosti modula koje zavise od mape i pozicije

Dva pravila važe za sve funkcionalnosti ispod:

- Stranica trenutnu poziciju turiste čita sa servera, svojim `httpResource` od
  `/api/positions/mine` (vidi README u `shared/position`), a ne iz stanja mape na drugom
  ekranu. Dok pozicija nije postavljena (vrednost resursa je `null`), stranica ne šalje nijedan
  zahtev koji traži koordinate, nego prikazuje "Pozicija nije postavljena — kliknite na mapu da
  označite gde se nalazite".
- Odluku da li je turista dovoljno blizu donosi modul koji poseduje posledicu, na serveru, a ne
  platforma ni mapa. Stranica šalje koordinate turiste u zahtevu svog modula (komanda kada
  ishod menja stanje, upit kada ga samo vraća); modul na serveru ne čita sačuvanu poziciju.

**E21 (putanja i dužina ture).** Mapa: `locations` sa ključnim tačkama redom, `showPath` i
`pathLengthChange`, dužina po putevima. Pozicija se ne koristi. Posao modula Exploration: daje
ključne tačke iz svog upita i, ako dužinu čuva kao poslovni podatak, šalje je svojom komandom.

**E22 (izvođenje ture).** Mapa: `locations` sa ključnim tačkama, `position` iz resursa,
`target` sa sledećom ključnom tačkom, za rutu po putevima. Pozicija: resurs `/api/positions/mine`
i simulator za postavljanje. Posao modula Exploration: zna koja je tačka sledeća, i proverava
blizinu na svakih 10 sekundi tako što šalje poslednju sačuvanu poziciju iz resursa svojim
zahtevom (ne traži od turiste nov klik); server odlučuje da li je tačka dostignuta.

**E23 (pretraga po oblasti).** Mapa: `area` sa centrom i poluprečnikom u metrima. Posao modula
Exploration: poluprečnik je polje njegove forme, a ne deo mape; stranica iz forme sastavi
`MapArea` i preda ga mapi, a centar i poluprečnik šalje svojim zahtevom.

**G21 (skrivena lokacija).** Mapa: `area` sa oblašću traženja i `position` iz resursa.
Pozicija: resurs i simulator. Posao modula Games: određuje oblast koju prikazuje, prima
koordinate turiste u svom zahtevu i na serveru odlučuje da li je turista na 5 m od skrivene
lokacije.

**G22 (izazov u blizini).** Mapa: `locations` sa izazovima i `position` iz resursa. Pozicija:
resurs i simulator. Posao modula Games: na svakih 10 sekundi šalje poslednju sačuvanu poziciju
svojim zahtevom, a server odlučuje šta je "dovoljno blizu" i vraća ishod.

## Izuzetak od Angular podskupa

Ova komponenta je jedini izuzetak od Angular podskupa iz `frontend/AGENTS.md`: da bi
upravljala Leaflet mapom, koja živi van stabla komponenti, koristi `inject(ElementRef)`,
`afterNextRender`, `effect()` i `DestroyRef`, a rute čita kroz `HttpClient` i `firstValueFrom`
iz komponente, ne kroz `httpResource` stranice. Sabiranje delova putanje je obična funkcija
`routedPath` u `routed-path.ts`, koja se testira bez Angular-a. Izuzetak je zatvoren u
`shared/map`. Timovi
modula koriste samo njene ulaze i izlaze i ne pišu ove konstrukcije u svojim modulima.
Izmena komponente je platformska odluka.

## Stilovi biblioteke

JavaScript Leaflet-a stiže lenjo, zajedno sa stranicom koja koristi mapu. Njegov CSS
(`leaflet.css`, oko 15 kB) je, naprotiv, svesno u globalnim stilovima (`angular.json`, niz
`styles`), pa se učitava na svakoj stranici. Leaflet sam pravi DOM mape, a stilovi komponente
se pod podrazumevanom (emulated) enkapsulacijom ne primenjuju na elemente koje komponenta
nije napisala u šablonu; `::ng-deep` i isključivanje enkapsulacije su van pravila projekta.
Globalni CSS je zato najjednostavnije rešenje koje radi.
