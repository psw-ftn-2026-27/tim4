# Mapa lokacija (`shared/map`)

`LocationMap` (`app-location-map`) je prikazna komponenta platforme koja prikazuje listu
lokacija na mapi. Svaka lokacija je jedan pin, a klik na pin prikazuje njen naziv pored
pina. Svi pinovi su uvek u vidnom polju. Kada je putanja uključena, pinovi su povezani
rutom koja prati pešačke puteve, redom kojim su predati, a komponenta šalje dužinu putanje u
kilometrima. Kada je
izbor uključen, korisnik klikom na mapu bira jednu lokaciju, a stranica dobija njene
koordinate.

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
| `locationClick`    | izlaz, `number`             | pozicija u `locations` pina na koji je korisnik kliknuo                                |

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
