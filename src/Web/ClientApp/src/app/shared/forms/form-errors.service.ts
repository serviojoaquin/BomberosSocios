import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';

export interface AppliedServerErrors {
  general: string[];
}

@Injectable({ providedIn: 'root' })
export class FormErrorsService {
  apply(error: any, form: FormGroup): AppliedServerErrors {
    const general: string[] = [];
    const errors = this.extractErrors(error);

    if (!errors) {
      general.push(this.fallbackMessage(error));
      return { general };
    }

    let firstControlId: string | null = null;

    for (const property of Object.keys(errors)) {
      const messages: string[] = errors[property] ?? [];
      const controlName = this.camelize(property);
      const control = controlName ? form.get(controlName) : null;

      if (control) {
        control.setErrors({ serverError: messages[0] });
        control.markAsTouched();
        firstControlId ??= controlName;
      } else {
        general.push(...messages);
      }
    }

    if (firstControlId) {
      setTimeout(() => document.getElementById(firstControlId!)?.focus(), 50);
    }

    return { general };
  }

  firstMessage(error: any, fallback: string): string {
    const errors = this.extractErrors(error);
    if (errors) {
      const first = Object.values(errors)[0] as string[] | undefined;
      if (first?.length) {
        return first[0];
      }
    }
    return fallback;
  }

  private extractErrors(error: any): Record<string, string[]> | null {
    try {
      const body = typeof error?.response === 'string' ? JSON.parse(error.response) : error?.response ?? error;
      return body?.errors ?? null;
    } catch {
      return null;
    }
  }

  private fallbackMessage(error: any): string {
    if (error?.status === 403) {
      return 'You do not have permission to perform this action.';
    }
    return 'An unexpected error occurred. Please try again.';
  }

  private camelize(property: string): string {
    const path = property.split('.');
    return path.map(segment => segment.charAt(0).toLowerCase() + segment.slice(1)).join('.');
  }
}
