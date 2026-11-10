import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from 'src/api-authorization/auth.service';
import { APP_MODULES, AppModule } from '../modules';
import { hasPermission } from '../permissions';
import { BRANDING } from '../branding';

interface SidebarSection {
  name: string;
  modules: AppModule[];
}

const COLLAPSED_KEY = 'sidebarCollapsed';

@Component({
  standalone: false,
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
  private readonly authService = inject(AuthService);
  private readonly currentUser = toSignal(this.authService.currentUser$, { initialValue: null });

  readonly branding = BRANDING;

  collapsed = signal(this.readCollapsed());
  mobileOpen = signal(false);

  sections = computed<SidebarSection[]>(() => {
    const user = this.currentUser();
    const visible = APP_MODULES.filter(m => {
      if (m.permission !== undefined) {
        return user !== null && hasPermission(user.permissions, m.permission);
      }
      return !m.requiresAuth || user !== null;
    });
    const sections: SidebarSection[] = [];
    for (const module of visible) {
      let section = sections.find(s => s.name === module.section);
      if (!section) {
        section = { name: module.section, modules: [] };
        sections.push(section);
      }
      section.modules.push(module);
    }
    return sections;
  });

  toggleCollapsed(): void {
    this.collapsed.update(v => !v);
    try {
      localStorage.setItem(COLLAPSED_KEY, String(this.collapsed()));
    } catch {
    }
  }

  toggleMobile(): void {
    this.mobileOpen.update(v => !v);
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
  }

  private readCollapsed(): boolean {
    try {
      return localStorage.getItem(COLLAPSED_KEY) === 'true';
    } catch {
      return false;
    }
  }
}
