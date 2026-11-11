export function hasFlag(permissions: number, key: number): boolean {
  return (permissions & key) === key;
}

export function addFlag(permissions: number, key: number): number {
  return permissions | key;
}

export function deleteFlag(permissions: number, key: number): number {
  return permissions & ~key;
}
