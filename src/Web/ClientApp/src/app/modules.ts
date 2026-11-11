import { Permissions } from './permissions';

export interface AppModule {
  title: string;
  path: string;
  icon: string;
  section: string;
  permission?: Permissions;
  requiresAuth?: boolean;
}

export const APP_MODULES: AppModule[] = [
  { title: 'menu.inicio', path: '/', icon: 'house', section: 'secciones.general' },
  { title: 'menu.socios', path: '/socios', icon: 'contact', section: 'secciones.gestion', permission: Permissions.VerSocios },
  { title: 'menu.cobradores', path: '/cobradores', icon: 'wallet', section: 'secciones.gestion', permission: Permissions.VerCobradores },
  { title: 'menu.categorias', path: '/categorias', icon: 'tags', section: 'secciones.gestion', permission: Permissions.VerCategorias },
  { title: 'menu.estadisticas', path: '/dashboard', icon: 'chart-column', section: 'secciones.gestion', permission: Permissions.VerEstadisticas },
  { title: 'menu.usuarios', path: '/users', icon: 'users', section: 'secciones.seguridad', permission: Permissions.ManageSecurity },
  { title: 'menu.roles', path: '/roles', icon: 'shield', section: 'secciones.seguridad', permission: Permissions.ManageSecurity },
  { title: 'menu.controlDeAcceso', path: '/control-de-acceso', icon: 'lock-keyhole', section: 'secciones.seguridad', permission: Permissions.ManageSecurity },
  { title: 'menu.cajas', path: '/cajas', icon: 'qr-code', section: 'secciones.seguridad', permission: Permissions.ManageSecurity }
];
