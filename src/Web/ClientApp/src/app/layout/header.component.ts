import { Component, ElementRef, EventEmitter, Inject, OnDestroy, Output, PLATFORM_ID, ViewChild, computed } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from 'src/api-authorization/auth.service';
import { LanguageService } from '../i18n/language.service';

@Component({
  standalone: false,
  selector: 'app-header',
  templateUrl: './header.component.html'
})
export class HeaderComponent implements OnDestroy {
  @Output() menuToggle = new EventEmitter<void>();
  @ViewChild('accountMenu') accountMenuRef: ElementRef<HTMLDetailsElement>;

  currentUser = toSignal(this.authService.currentUser$, { initialValue: null });
  initials = computed(() => {
    const name = this.currentUser()?.userName ?? '';
    const base = name.split('@')[0].replace(/[^a-zA-Z0-9]+/g, ' ').trim();
    const parts = base.split(' ').filter(p => p.length);
    const letters = parts.length >= 2 ? parts[0][0] + parts[1][0] : base.slice(0, 2);
    return letters.toUpperCase();
  });

  private readonly isBrowser: boolean;
  private readonly onDocumentClick = (event: Event) => this.closeOnOutsideClick(event);

  constructor(
    private authService: AuthService,
    private router: Router,
    public language: LanguageService,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
    if (this.isBrowser) {
      document.addEventListener('click', this.onDocumentClick);
    }
  }

  ngOnDestroy(): void {
    if (this.isBrowser) {
      document.removeEventListener('click', this.onDocumentClick);
    }
  }

  setLanguage(code: string): void {
    this.closeMenu();
    void this.language.use(code);
  }

  logout(): void {
    this.closeMenu();
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login'])
    });
  }

  navigate(path: string): void {
    this.closeMenu();
    this.router.navigate([path]);
  }

  closeMenu(): void {
    this.accountMenuRef?.nativeElement.removeAttribute('open');
  }

  private closeOnOutsideClick(event: Event): void {
    if (!(event.target as HTMLElement).closest('details.menu-dropdown')) {
      this.closeMenu();
    }
  }
}
