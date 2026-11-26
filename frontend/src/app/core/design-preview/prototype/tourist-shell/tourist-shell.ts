import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

// PROTOTYPE. The tourist's application shell as proposed in the redesign: sections by goal in the
// middle, "mine" on the right (cart, wallet, user menu), and a strip while a tour is in progress.
// It is drawn inside a frame under the real bar, so the real bar stays usable.
@Component({
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  selector: 'app-tourist-shell',
  styleUrl: '../proto-shell.scss',
  templateUrl: './tourist-shell.html',
})
export class TouristShell {
  protected readonly base = '/design-preview/prototype/tourist';
}
