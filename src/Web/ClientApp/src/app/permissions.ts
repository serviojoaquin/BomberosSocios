export enum Permissions {
  None = 0,
  ViewCategories = 1,
  CreateCategories = 2,
  EditCategories = 4,
  DeleteCategories = 8,
  ManageSecurity = 16,
  All = -1
}

export function hasPermission(granted: number, required: Permissions): boolean {
  return (granted & required) === required;
}
