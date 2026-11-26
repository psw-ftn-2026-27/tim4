import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

// PROTOTYPE. The visitor's shell: the brand, one link to the story and the two ways in, with
// registration as the primary button. The landing page itself stays as it is (D12).
@Component({
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  selector: 'app-guest-shell',
  styleUrls: ['../proto-shell.scss', './guest-shell.scss'],
  templateUrl: './guest-shell.html',
})
export class GuestShell {
  protected readonly base = '/design-preview/prototype/guest';
}
