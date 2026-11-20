# Simulator pozicije (`shared/position`)

Turista bez mobilne aplikacije klikom na mapu označi gde se nalazi, pa ekrani koji zavise od
njegove blizine (izvođenje ture, aktivacija i rešavanje izazova) rade i na računaru, bez
GPS-a. Server čuva najviše jednu, trenutnu poziciju prijavljenog turiste; nova zamenjuje
prethodnu. Pozicija postoji tek kada je turista postavi.

Servis `PositionSimulation` je jedino mesto na klijentu koje zna adresu `/api/positions/mine`
za upis. Ima jednu komandu, `setPosition`, koja šalje `CoordinatesDto` (`shared/api`):
`latitude` i `longitude` u decimalnim stepenima. Servis ne čuva stanje i ne čita poziciju.
Čitanje i upis su dostupni samo ulozi `explorer`.

Živi primer je sekcija "Simulator pozicije" na stranici `/design-preview/map`
(`core/design-preview/map-view/`).

## Čitanje pozicije

Stranica čita poziciju sama, svojim `httpResource`, kao svaki upit. Server odgovara sa 204 kada
pozicija nije postavljena, pa je vrednost resursa tada `null`. Stranica pravi zahtev samo za
prijavljenog turistu; za ostale funkcija adrese vraća `undefined`.

```ts
protected readonly position = httpResource<CoordinatesDto | null>(() =>
  this.isExplorer() ? '/api/positions/mine' : undefined,
);
```

## Postavljanje pozicije

Mapa iz `shared/map` hvata klik kroz `selectable` i `selectedChange`, a oznaku crta iz ulaza
`position` (uputstvo je u njenom README-u). Stranica posle uspešne komande ponovo učita svoj
resurs, pa se oznaka pomera tek kada server sačuva poziciju.

```ts
protected async setPosition(coordinates: MapCoordinates): Promise<void> {
  this.pending.set(true);
  this.error.set(null);
  try {
    await this.positionSimulation.setPosition(coordinates);
    this.position.reload();
  } catch (failure) {
    this.error.set(serverMessage(failure, 'Poziciju nije moguće sačuvati.'));
  } finally {
    this.pending.set(false);
  }
}
```

## Periodična provera

Funkcija `repeatEvery(intervalMs, callback)` iz `repeat-every.ts` ponavlja proveru dok je turista
na stranici, pa E22 i G22 prepoznaju blizinu bez novog klika. Radnja se pozove odmah, a zatim na
svaki interval. Funkcija vraća objekat sa jednom metodom, `stop()`, kojom stranica zaustavi
ponavljanje dok je turista još na njoj. Kada turista napusti stranicu, ponavljanje se zaustavi
samo. Funkcija ne šalje zahteve i ne zna ništa o pozicijama, turama ni izazovima.

Stranica je poziva u inicijalizatoru polja, kao `httpResource`, i to posle resursa pozicije, jer
se prvi otkucaj izvrši odmah. Pri svakom otkucaju stranica pošalje poslednju sačuvanu poziciju
svojim zahtevom; dok pozicija nije postavljena ili učitana, ne šalje ništa. Odluku o blizini
donosi modul na serveru. Interval zadaje stranica; E22 i G22 koriste 10 sekundi.

**E22 (izvođenje ture).** Kada server javi da je tura završena, stranica zaustavi proveru.

```ts
private readonly check = repeatEvery(10_000, () => this.checkProximity());

private async checkProximity(): Promise<void> {
  const position = this.position.hasValue() ? this.position.value() : null;
  if (!position) {
    return;
  }
  this.error.set(null);
  try {
    // The group's service sends the position with Exploration's own request.
    const result = await this.tourExecution.checkKeyPoint(position);
    this.result.set(result);
    if (result.tourCompleted) {
      this.check.stop();
    }
  } catch (failure) {
    this.error.set(serverMessage(failure, 'Proveru blizine nije moguće izvršiti.'));
  }
}
```

**G22 (izazov u blizini).** Isto, sa zahtevom modula Games; kada server javi da je igrač ispao
ili rešio izazov, stranica zaustavi proveru. Odluku o ispadanju posle 10 sekundi bez lokacije
donosi server, prema trenutku kada je primio zahtev. Pregledači usporavaju tajmere u
pozadinskim karticama, pa igrač koji pređe u drugu karticu može da ispadne.

Imena `checkKeyPoint`, `tourCompleted` i ostala u primeru su ilustracija; zahteve i odgovore
dizajnira modul koji poseduje posledicu. Ponavljanje ne čeka da se prethodni zahtev završi.

Živi primer je sekcija "Periodična provera" na stranici `/design-preview/map`. Dugme
"Zaustavi" tamo zamenjuje odgovor servera posle kog provera prestaje.

### Izuzetak od Angular podskupa

`repeatEvery` koristi `setInterval` i `inject(DestroyRef)`, kojih nema u Angular podskupu iz
`frontend/AGENTS.md`. Izuzetak je zatvoren u `shared/position`: moduli pozivaju samo
`repeatEvery` i `stop()` i nikada ne pišu `setInterval` ni `DestroyRef`. Izmena funkcije je
platformska odluka.

Simulator je privremen: kada stigne mobilna aplikacija, poziciju daje GPS.
