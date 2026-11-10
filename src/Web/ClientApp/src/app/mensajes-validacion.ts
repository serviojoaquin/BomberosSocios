import { FieldErrorMessages } from './shared/forms/field-error.component';

export const MENSAJES_VALIDACION: FieldErrorMessages = {
  required: () => 'Este campo es requerido.',
  maxlength: e => `No puede exceder los ${e.requiredLength} caracteres.`,
  minlength: e => `Debe tener al menos ${e.requiredLength} caracteres.`,
  min: e => `El valor mínimo es ${e.min}.`,
  max: e => `El valor máximo es ${e.max}.`,
  email: () => 'Ingresá un email válido.',
  pattern: () => 'El formato no es válido.',
  serverError: e => String(e)
};
