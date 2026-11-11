import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface PaletteColor {
  token: string;
  role: string;
  value: string;
  example: string;
}

interface Sketch {
  path: string;
  title: string;
}

// Mock data: the values repeat _tokens.scss so the page can print them; keep the two in step.
const PALETTE: PaletteColor[] = [
  { token: '--color-bg', role: 'Pozadina', value: '#f6f1e7', example: 'Pozadina cele stranice' },
  { token: '--color-surface', role: 'Površina', value: '#fffdf8', example: 'Kartica, forma, tabela' },
  { token: '--color-border', role: 'Ivica', value: '#e0d6c2', example: 'Ivica kartice, linija u tabeli' },
  {
    token: '--color-control-border',
    role: 'Ivica kontrole',
    value: '#8c8170',
    example: 'Ivica polja forme i sekundarnog dugmeta',
  },
  { token: '--color-text', role: 'Tekst', value: '#2b2a26', example: 'Običan tekst' },
  {
    token: '--color-text-muted',
    role: 'Prigušen tekst',
    value: '#635c50',
    example: 'Labela polja, sporedna informacija',
  },
  { token: '--color-accent', role: 'Akcenat', value: '#2d6a4f', example: 'Primarno dugme, link' },
  {
    token: '--color-accent-soft',
    role: 'Blagi akcenat',
    value: '#e4efe8',
    example: 'Polje za upload dok se fajl prevlači',
  },
  {
    token: '--color-danger',
    role: 'Greška',
    value: '#b3261e',
    example: 'Poruka o grešci, dugme za brisanje',
  },
  { token: '--color-success-bg', role: 'Uspeh', value: '#e3f1e7', example: 'Poruka o uspehu (tekst #1e5a3a)' },
  { token: '--color-muted-bg', role: 'Prigušeno', value: '#ebe5d9', example: 'Stornirano, arhivirano, povučeno' },
  { token: '--color-ec-bg', role: 'EC', value: '#fbefc8', example: 'Iznos u EC (tekst #7a5300)' },
  { token: '--color-xp-bg', role: 'XP', value: '#ece4fb', example: 'Nagrada u XP (tekst #5b3fa8)' },
  {
    token: '--color-state-draft-bg',
    role: 'Stanje: u pripremi',
    value: '#dfe8ee',
    example: 'Tura u pripremi, izazov u nacrtu (tekst #2f4f66)',
  },
  {
    token: '--color-state-published-bg',
    role: 'Stanje: objavljeno',
    value: '#2d6a4f',
    example: 'Objavljena tura ili izazov (tekst #ffffff)',
  },
  {
    token: '--color-difficulty-easy-bg',
    role: 'Težina: lako',
    value: '#e6eed8',
    example: 'Laka tura (tekst #3d5a1a)',
  },
  {
    token: '--color-difficulty-moderate-bg',
    role: 'Težina: srednje',
    value: '#f5e3c8',
    example: 'Srednje teška tura (tekst #7d4a12)',
  },
  {
    token: '--color-difficulty-hard-bg',
    role: 'Težina: teško',
    value: '#f4d9cf',
    example: 'Teška tura (tekst #8a2e14)',
  },
];

const SKETCHES: Sketch[] = [
  { path: 'user-wallet', title: 'Novčanik korisnika' },
  { path: 'admin-wallet', title: 'Administratorski pregled novčanika' },
  { path: 'tour-search', title: 'Pretraga tura' },
  { path: 'tour-editing', title: 'Autorsko uređivanje ture' },
  { path: 'tour-preferences', title: 'Preference tura' },
  { path: 'admin-challenges', title: 'Administratorski pregled izazova i kviza' },
  { path: 'map', title: 'Mapa' },
];

@Component({
  imports: [RouterLink],
  selector: 'app-overview',
  styleUrl: './overview.scss',
  templateUrl: './overview.html',
})
export class Overview {
  protected readonly palette = PALETTE;
  protected readonly sketches = SKETCHES;
}
