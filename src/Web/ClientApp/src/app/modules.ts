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
  { title: 'Categorías', path: '/categorias', icon: 'tags', section: 'Gestión', permission: Permissions.VerCategorias },
  { title: 'Cobradores', path: '/cobradores', icon: 'wallet', section: 'Gestión', permission: Permissions.VerCobradores },
  { title: 'Socios', path: '/socios', icon: 'contact', section: 'Gestión', permission: Permissions.VerSocios },
  { title: 'Roles', path: '/roles', icon: 'shield', section: 'Seguridad', permission: Permissions.ManageSecurity },
  { title: 'Usuarios', path: '/users', icon: 'users', section: 'Seguridad', permission: Permissions.ManageSecurity }
];
