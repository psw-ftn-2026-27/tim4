# Mapa lokacija (`shared/map`)

`LocationMap` (`app-location-map`) je prikazna komponenta platforme koja prikazuje listu
lokacija na mapi. Svaka lokacija je jedan pin, a klik na pin prikazuje njen naziv pored
pina. Svi pinovi su uvek u vidnom polju. Kada je putanja uključena, pinovi su povezani
linijom redom kojim su predati, a komponenta šalje dužinu putanje u kilometrima.

Mapa koristi biblioteku Leaflet i podlogu OpenStreetMap, besplatno i bez ključa. Timovi
modula ne uče Leaflet: koriste samo ulaze i izlaz opisane ispod.

Živi primer je probna stranica `/map-demo` u `core/map-demo/`.

## Ulazi i izlaz

| Ime                | Vrsta                       | Značenje                                                         |
| ------------------ | --------------------------- | ---------------------------------------------------------------- |
| `locations`        | ulaz, obavezan              | `MapLocation[]`: `latitude`, `longitude` i `name` svake lokacije |
| `showPath`         | ulaz, podrazumevano `false` | da li linija spaja lokacije redom kojim su predate               |
| `pathLengthChange` | izlaz, `number`             | dužina putanje u kilometrima, šalje se pri svakoj promeni ulaza  |

Dužina putanje je zbir pravolinijskih razdaljina između uzastopnih lokacija; poslednja
lokacija nije spojena sa prvom, a za manje od dve lokacije dužina je 0 km. Komponenta
uvek šalje dužinu lokacija onim redom kojim su predate, i kada je `showPath` isključen;
stranica sama odlučuje da li je prikazuje.

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
  protected readonly pathLength = signal(0);
}
```

```html
<app-location-map
  [locations]="keyPoints()"
  [showPath]="true"
  (pathLengthChange)="pathLength.set($event)"
/>
<p>Dužina putanje: {{ pathLength().toFixed(2) }} km</p>
```

## Izuzetak od Angular podskupa

Ova komponenta je jedini izuzetak od Angular podskupa iz `frontend/AGENTS.md`: da bi
upravljala Leaflet mapom, koja živi van stabla komponenti, koristi `inject(ElementRef)`,
`afterNextRender`, `effect()` i `DestroyRef`. Izuzetak je zatvoren u `shared/map`. Timovi
modula koriste samo njene ulaze i izlaz i ne pišu ove konstrukcije u svojim modulima.
Izmena komponente je platformska odluka.
