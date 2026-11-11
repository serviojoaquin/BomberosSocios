export enum Permissions {
  None = 0,
  ViewSampleItems = 1,
  CreateSampleItems = 2,
  EditSampleItems = 4,
  DeleteSampleItems = 8,
  ManageSecurity = 16,
  VerCategorias = 32,
  CrearCategorias = 64,
  EditarCategorias = 128,
  EliminarCategorias = 256,
  VerCobradores = 512,
  CrearCobradores = 1024,
  EditarCobradores = 2048,
  EliminarCobradores = 4096,
  VerSocios = 8192,
  CrearSocios = 16384,
  EditarSocios = 32768,
  EliminarSocios = 65536,
  VerCuotas = 131072,
  PagarCuotas = 262144,
  AdministrarCuotas = 524288,
  VerEstadisticas = 1048576,
  PagosMercadoPago = 2097152,
  VerMercadoPago = 4194304,
  AdministrarMercadoPago = 8388608,
  All = -1
}

export function hasPermission(granted: number, required: Permissions): boolean {
  return (granted & required) === required;
}
