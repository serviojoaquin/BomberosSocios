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
  { title: 'Home', path: '/', icon: 'house', section: 'General' },
  { title: 'Counter', path: '/counter', icon: 'hash', section: 'Samples' },
  { title: 'Weather', path: '/weather', icon: 'cloud-sun', section: 'Samples', requiresAuth: true },
  { title: 'Tasks', path: '/todo', icon: 'list-checks', section: 'Samples', requiresAuth: true },
  { title: 'Sample items', path: '/sample-items', icon: 'box', section: 'Samples', permission: Permissions.ViewSampleItems },
  { title: 'Roles', path: '/roles', icon: 'shield', section: 'Security', permission: Permissions.ManageSecurity },
  { title: 'Categorías', path: '/categorias', icon: 'tags', section: 'Socios', permission: Permissions.VerCategorias },
  { title: 'Users', path: '/users', icon: 'users', section: 'Security', permission: Permissions.ManageSecurity }
];
