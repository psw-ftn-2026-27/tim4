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

Simulator je privremen: kada stigne mobilna aplikacija, poziciju daje GPS.
