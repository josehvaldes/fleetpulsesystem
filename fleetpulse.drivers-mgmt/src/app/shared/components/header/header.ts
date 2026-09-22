import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthUser } from '@app/types/authuser';

@Component({
  imports: [RouterLink],
  selector: 'fp-header',
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class Header {
  title = input.required<string>();
  user = input.required<AuthUser | null>();
}