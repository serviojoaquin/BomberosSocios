import { Component, ElementRef, EventEmitter, Input, Output, ViewChild, signal } from '@angular/core';

export interface YearSelectorResult {
  year: number;
  checkValue: boolean;
  startSelectedDate?: Date;
  endSelectedDate?: Date;
}

export interface YearSelectorOptions {
  initialYear: number;
  maxYears?: number;
  showCheck?: boolean;
  checkText?: string;
  showDateRangePicker?: boolean;
}

@Component({
  standalone: false,
  selector: 'app-year-selector',
  templateUrl: './year-selector.component.html'
})
export class YearSelectorComponent {
  @ViewChild('dialog') dialogRef: ElementRef<HTMLDialogElement>;

  @Output() selected = new EventEmitter<YearSelectorResult>();

  years = signal<number[]>([]);
  showCheck = signal(false);
  checkText = signal('');
  showDateRangePicker = signal(false);
  checkValue = false;
  startDate = '';
  endDate = '';
  minDate = '';
  maxDate = '';

  open(options: YearSelectorOptions): void {
    const maxYears = options.maxYears ?? 2;
    const years: number[] = [];
    for (let i = 0; i < maxYears; i++) {
      years.push(options.initialYear + i);
    }
    this.years.set(years);
    this.showCheck.set(!!options.showCheck);
    this.checkText.set(options.checkText ?? '');
    this.showDateRangePicker.set(!!options.showDateRangePicker);
    this.checkValue = false;
    this.startDate = '';
    this.endDate = '';
    this.minDate = `${options.initialYear}-01-01`;
    this.maxDate = `${options.initialYear + maxYears - 1}-12-31`;
    this.dialogRef.nativeElement.showModal();
  }

  close(): void {
    this.dialogRef.nativeElement.close();
  }

  choose(year: number): void {
    const result: YearSelectorResult = { year, checkValue: this.checkValue };
    if (this.showDateRangePicker() && this.startDate && this.endDate) {
      result.startSelectedDate = new Date(this.startDate + 'T00:00:00');
      result.endSelectedDate = new Date(this.endDate + 'T00:00:00');
    }
    this.close();
    this.selected.emit(result);
  }
}
