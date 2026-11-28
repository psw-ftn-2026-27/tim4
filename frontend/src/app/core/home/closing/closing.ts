import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

// The last call to sign up, next to a card that invites future tour authors.
@Component({
  imports: [RouterLink],
  selector: 'app-closing',
  styleUrl: './closing.scss',
  templateUrl: './closing.html',
})
export class Closing {}
