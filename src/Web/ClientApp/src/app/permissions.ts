export enum Permissions {
  None = 0,
  ViewSampleItems = 1,
  CreateSampleItems = 2,
  EditSampleItems = 4,
  DeleteSampleItems = 8,
  ManageSecurity = 16,
  All = -1
}

export function hasPermission(granted: number, required: Permissions): boolean {
  return (granted & required) === required;
}
