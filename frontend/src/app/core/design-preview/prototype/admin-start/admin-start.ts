import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { challenges, explorers } from '../mock';

// PROTOTYPE of the administrator's start page: not links, but what is waiting for them, each with
// the one button that deals with it.
@Component({
  imports: [RouterLink],
  selector: 'app-admin-start',
  styleUrl: './admin-start.scss',
  templateUrl: './admin-start.html',
})
export class AdminStart {
  protected readonly base = '/design-preview/prototype/admin';

  protected readonly tasks = [
    {
      count: explorers.filter((e) => e.balance === 0).length,
      title: 'turista bez EC',
      text: 'Novi nalozi ne mogu da kupe plaćene ture dok ne dobiju EC.',
      action: 'Uplati EC',
      link: '/wallets',
      tone: 'ec',
    },
    {
      count: challenges.filter((c) => c.status === 'Draft').length,
      title: 'izazova u nacrtu',
      text: 'Turisti ih ne vide dok ih ne objavite.',
      action: 'Pregledaj nacrte',
      link: '/challenges',
      tone: 'draft',
    },
    {
      count: challenges.filter((c) => c.status === 'Retired').length,
      title: 'povučen izazov',
      text: 'Objavite ga ponovo ili arhivirajte.',
      action: 'Otvori',
      link: '/challenges',
      tone: 'muted',
    },
  ];

  protected readonly stats = [
    { value: explorers.length, label: 'turista' },
    {
      value: challenges.filter((c) => c.status === 'Published').length,
      label: 'objavljenih izazova',
    },
    { value: explorers.reduce((sum, e) => sum + e.balance, 0), label: 'EC u novčanicima' },
  ];
}
