import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

// The page for an address that does not exist, and, with the route data kind 'forbidden', for a
// page the signed-in user's role may not open (see requireRole).
@Component({
  imports: [RouterLink],
  selector: 'app-not-found',
  styleUrl: './not-found.scss',
  templateUrl: './not-found.html',
})
export class NotFound {
  readonly kind = input<'missing' | 'forbidden'>('missing');
}
