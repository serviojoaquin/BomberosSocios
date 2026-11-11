import { Injectable, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

export interface Language {
  code: string;
  label: string;
}

export const LANGUAGES: Language[] = [
  { code: 'en', label: 'English' },
  { code: 'es', label: 'Español' }
];

export const DEFAULT_LANGUAGE = 'en';

const STORAGE_KEY = 'language';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  readonly languages = LANGUAGES;
  readonly current = signal(DEFAULT_LANGUAGE);

  constructor(private translate: TranslateService) {}

  async init(): Promise<void> {
    await this.use(this.stored() ?? this.fromBrowser());
  }

  async use(code: string): Promise<void> {
    const language = LANGUAGES.some(l => l.code === code) ? code : DEFAULT_LANGUAGE;
    await firstValueFrom(this.translate.use(language));
    this.current.set(language);
    document.documentElement.lang = language;
    try {
      localStorage.setItem(STORAGE_KEY, language);
    } catch {
    }
  }

  private stored(): string | null {
    try {
      return localStorage.getItem(STORAGE_KEY);
    } catch {
      return null;
    }
  }

  private fromBrowser(): string {
    const browser = (navigator.language || DEFAULT_LANGUAGE).slice(0, 2).toLowerCase();
    return LANGUAGES.some(l => l.code === browser) ? browser : DEFAULT_LANGUAGE;
  }
}
