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
  { title: 'modules.home', path: '/', icon: 'house', section: 'sections.general' },
  { title: 'modules.counter', path: '/counter', icon: 'hash', section: 'sections.samples' },
  { title: 'modules.weather', path: '/weather', icon: 'cloud-sun', section: 'sections.samples', requiresAuth: true },
  { title: 'modules.tasks', path: '/todo', icon: 'list-checks', section: 'sections.samples', requiresAuth: true },
  { title: 'modules.sampleItems', path: '/sample-items', icon: 'box', section: 'sections.samples', permission: Permissions.ViewSampleItems },
  { title: 'modules.roles', path: '/roles', icon: 'shield', section: 'sections.security', permission: Permissions.ManageSecurity },
  { title: 'modules.users', path: '/users', icon: 'users', section: 'sections.security', permission: Permissions.ManageSecurity },
  { title: 'modules.accessControl', path: '/access-control', icon: 'lock-keyhole', section: 'sections.security', permission: Permissions.ManageSecurity }
];
