import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-games-home',
  templateUrl: './games-home.html',
  styleUrl: './games-home.scss',
})
export class GamesHome {
  protected readonly auth = inject(Auth);
}
