import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface ProtoScreen {
  path: string;
  title: string;
  scenario: string;
  ready: boolean;
}

const GUEST: ProtoScreen[] = [
  { path: 'guest', title: 'Naslovna (postojeća) u novoj traci', scenario: 'S0', ready: true },
  { path: 'guest/login', title: 'Prijava', scenario: 'S0', ready: true },
  {
    path: 'guest/register',
    title: 'Registracija sa pravilima lozinke',
    scenario: 'S0',
    ready: true,
  },
];

const TOURIST: ProtoScreen[] = [
  { path: 'tourist', title: 'Početna: prvi koraci i „šta sada“', scenario: 'S0–S5', ready: true },
  { path: 'tourist/tours', title: 'Ture: spisak i mapa', scenario: 'S1 Pronađi turu', ready: true },
  {
    path: 'tourist/tours/tvrdjava',
    title: 'Detalji ture i panel kupovine',
    scenario: 'S1–S3',
    ready: true,
  },
  {
    path: 'tourist/cart',
    title: 'Korpa → plaćanje → potvrda',
    scenario: 'S2 Kupi turu',
    ready: true,
  },
  { path: 'tourist/journeys', title: 'Moja putovanja', scenario: 'S3 Prođi turu', ready: true },
  { path: 'tourist/active-tour', title: 'Aktivna tura', scenario: 'S3 Prođi turu', ready: true },
  { path: 'tourist/challenges', title: 'Izazovi na mapi', scenario: 'S4 Igraj', ready: true },
  { path: 'tourist/quiz', title: 'Rešavanje kviza', scenario: 'S4 Igraj', ready: true },
  {
    path: 'tourist/wallet',
    title: 'Novčanik i poklon kartice',
    scenario: 'S5 Novčanik',
    ready: true,
  },
  {
    path: 'tourist/studio',
    title: 'Autorski studio i uređivač ture',
    scenario: 'S6–S7 Autor',
    ready: true,
  },
];

const ADMIN: ProtoScreen[] = [
  { path: 'admin', title: 'Početna: šta čeka administratora', scenario: 'S8–S11', ready: true },
  { path: 'admin/challenges', title: 'Izazovi (sva tri tipa)', scenario: 'S9–S11', ready: true },
  {
    path: 'admin/wallets',
    title: 'Novčanici: novi turisti i uplata EC',
    scenario: 'S8 Uplata EC',
    ready: true,
  },
];

// PROTOTYPE. The entry to the redesign prototypes: two applications, screens grouped by scenario.
@Component({
  imports: [RouterLink],
  selector: 'app-proto-index',
  styleUrl: './proto-index.scss',
  templateUrl: './proto-index.html',
})
export class ProtoIndex {
  protected readonly groups = [
    { title: 'Gost', start: 'guest', screens: GUEST },
    { title: 'Turista', start: 'tourist', screens: TOURIST },
    { title: 'Administrator', start: 'admin', screens: ADMIN },
  ];
}
