import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

// The first thing a visitor sees: the slogan, "Registruj se" and the picture.
@Component({
  imports: [RouterLink],
  selector: 'app-hero',
  styleUrl: './hero.scss',
  templateUrl: './hero.html',
})
export class Hero {}
