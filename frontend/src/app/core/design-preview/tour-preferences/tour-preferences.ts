import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface TourPreference {
  difficulties: string[];
  transport: string;
  tags: string[];
  applyToSearch: boolean;
}

// Mock data: the preferences this user saved earlier.
const SAVED: TourPreference = {
  difficulties: ['Lako', 'Srednje'],
  transport: 'Bicikl',
  tags: ['priroda', 'istorija'],
  applyToSearch: true,
};

@Component({
  imports: [RouterLink],
  selector: 'app-tour-preferences',
  styleUrl: './tour-preferences.scss',
  templateUrl: './tour-preferences.html',
})
export class TourPreferences {
  protected readonly saved = SAVED;
}
