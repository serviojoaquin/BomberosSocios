import { Component, computed } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ThemeService, Theme } from '../theme.service';
import { LanguageService } from '../i18n/language.service';

@Component({
  standalone: false,
  selector: 'app-theme-toggle',
  templateUrl: './theme-toggle.component.html'
})
export class ThemeToggleComponent {
  constructor(public themeService: ThemeService, private translate: TranslateService, private language: LanguageService) {}

  icon = computed(() => {
    switch (this.themeService.theme()) {
      case 'light': return 'sun';
      case 'dark':  return 'moon';
      default:      return 'laptop';
    }
  });

  label = computed(() => {
    this.language.current();
    switch (this.themeService.theme()) {
      case 'light': return this.translate.instant('layout.theme.light');
      case 'dark':  return this.translate.instant('layout.theme.dark');
      default:      return this.translate.instant('layout.theme.system');
    }
  });

  cycle(): void {
    const next: Record<Theme, Theme> = { auto: 'light', light: 'dark', dark: 'auto' };
    this.themeService.setTheme(next[this.themeService.theme()]);
  }
}
