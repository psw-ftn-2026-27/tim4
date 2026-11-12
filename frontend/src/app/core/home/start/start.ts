import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

// What a signed-in user sees at `/`: a greeting and links grouped by goal. It only
// links to existing module routes; an administrator gets the reading links only.
@Component({
  imports: [RouterLink],
  selector: 'app-start',
  styleUrl: './start.scss',
  templateUrl: './start.html',
})
export class Start {
  readonly name = input.required<string>();
  readonly isAdmin = input(false);
}
