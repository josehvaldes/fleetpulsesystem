import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from './shared/components/header/header';
import { AuthUser } from './types/authuser';

@Component({
  imports: [RouterOutlet, Header],
  selector: 'app-root',
  styleUrls: ['./app.css'],
  templateUrl: './app.html',
})
export class App {
  protected readonly title = signal('fleetpulse.drivers-mgmt');
  protected readonly user = signal<AuthUser | null>({
    token: 'default-token',
    userId: '1',
    username: 'admin'
  });
}
