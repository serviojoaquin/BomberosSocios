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
  { title: 'Inicio', path: '/', icon: 'house', section: 'General' },
  { title: 'Socios', path: '/socios', icon: 'contact', section: 'Gestión', permission: Permissions.VerSocios },
  { title: 'Cobradores', path: '/cobradores', icon: 'wallet', section: 'Gestión', permission: Permissions.VerCobradores },
  { title: 'Categorías', path: '/categorias', icon: 'tags', section: 'Gestión', permission: Permissions.VerCategorias },
  { title: 'Estadísticas', path: '/dashboard', icon: 'chart-column', section: 'Gestión', permission: Permissions.VerEstadisticas },
  { title: 'Usuarios', path: '/users', icon: 'users', section: 'Seguridad', permission: Permissions.ManageSecurity },
  { title: 'Roles', path: '/roles', icon: 'shield', section: 'Seguridad', permission: Permissions.ManageSecurity },
  { title: 'Control de acceso', path: '/control-de-acceso', icon: 'lock-keyhole', section: 'Seguridad', permission: Permissions.ManageSecurity },
  { title: 'Cajas', path: '/cajas', icon: 'qr-code', section: 'Seguridad', permission: Permissions.ManageSecurity }
];
