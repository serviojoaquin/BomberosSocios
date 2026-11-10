import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { BRANDING } from './branding';

@Component({
  standalone: false,
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  constructor(title: Title) {
    title.setTitle(BRANDING.name);
  }
}
