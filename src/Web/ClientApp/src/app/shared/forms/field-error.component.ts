import { Component, Inject, InjectionToken, Input, OnChanges, OnDestroy, Optional, signal } from '@angular/core';
import { AbstractControl, FormGroupDirective } from '@angular/forms';
import { ControlStateWatcher } from './form-state';
import { TranslateService } from '@ngx-translate/core';

export type FieldErrorMessages = Record<string, (error: any) => string>;

export const FIELD_ERROR_MESSAGES = new InjectionToken<FieldErrorMessages>('FIELD_ERROR_MESSAGES');

const DEFAULT_MESSAGES: FieldErrorMessages = {
  required: () => 'This field is required.',
  maxlength: e => `Maximum length is ${e.requiredLength} characters.`,
  minlength: e => `Minimum length is ${e.requiredLength} characters.`,
  min: e => `Minimum value is ${e.min}.`,
  max: e => `Maximum value is ${e.max}.`,
  email: () => 'Enter a valid email address.',
  pattern: () => 'The value has an invalid format.',
  serverError: e => String(e)
};

@Component({
  standalone: false,
  selector: 'app-field-error',
  template: `@if (message()) {<small class="field-error">{{ message() }}</small>}`
})
export class FieldErrorComponent implements OnChanges, OnDestroy {
  @Input({ required: true }) control: AbstractControl | null = null;
  @Input() messages: Record<string, string> = {};

  readonly message = signal<string | null>(null);

  private readonly defaults: FieldErrorMessages;
  private readonly overrides: FieldErrorMessages;
  private readonly watcher = new ControlStateWatcher(() => this.refresh());

  constructor(
    @Optional() @Inject(FIELD_ERROR_MESSAGES) overrides: FieldErrorMessages | null,
    @Optional() private readonly form: FormGroupDirective | null,
    @Optional() private readonly translate: TranslateService | null
  ) {
    this.overrides = overrides ?? {};
    this.defaults = { ...DEFAULT_MESSAGES, ...this.overrides };
  }

  ngOnChanges(): void {
    this.watcher.watch(this.control, this.form);
  }

  ngOnDestroy(): void {
    this.watcher.dispose();
  }

  private refresh(): void {
    const control = this.control;
    if (!this.watcher.shouldShow(control) || !control?.errors) {
      this.message.set(null);
      return;
    }
    const [key, value] = Object.entries(control.errors)[0];
    if (this.messages[key]) {
      this.message.set(this.messages[key]);
      return;
    }
    if (this.translate && !this.overrides[key]) {
      const translated = this.translate.instant(`validation.${key}`, typeof value === 'object' && value !== null ? value : {});
      if (typeof translated === 'string' && translated !== `validation.${key}`) {
        this.message.set(translated);
        return;
      }
    }
    const render = this.defaults[key];
    this.message.set(render ? render(value) : 'Invalid value.');
  }
}
