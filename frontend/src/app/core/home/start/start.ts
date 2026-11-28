import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

// What an administrator sees at `/`: a greeting and their two sections. A tourist gets
// TouristStart instead.
@Component({
  imports: [RouterLink],
  selector: 'app-start',
  styleUrl: './start.scss',
  templateUrl: './start.html',
})
export class Start {
  readonly name = input.required<string>();
}
