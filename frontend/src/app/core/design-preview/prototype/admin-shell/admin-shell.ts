import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

// PROTOTYPE. The administrator's shell: only the three areas an administrator manages.
@Component({
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  selector: 'app-admin-shell',
  styleUrl: '../proto-shell.scss',
  templateUrl: './admin-shell.html',
})
export class AdminShell {
  protected readonly base = '/design-preview/prototype/admin';
}
