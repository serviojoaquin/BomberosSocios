import {
  Component, ElementRef, EventEmitter, Inject, InjectionToken, Input, OnChanges, OnDestroy, Optional, Output,
  PLATFORM_ID, SimpleChanges, ViewChild, AfterViewInit
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import DataTable from 'datatables.net-dt';
import 'datatables.net-buttons-dt';
import 'datatables.net-buttons/js/buttons.html5.mjs';
import 'datatables.net-buttons/js/buttons.print.mjs';
import 'datatables.net-responsive-dt';
import 'datatables.net-select-dt';
import JSZip from 'jszip';
import pdfMake from 'pdfmake/build/pdfmake';
import pdfFonts from 'pdfmake/build/vfs_fonts';

pdfMake.addVirtualFileSystem?.(pdfFonts);
DataTable.Buttons.pdfMake(pdfMake);
DataTable.Buttons.jszip(JSZip);

export const DATA_TABLE_LANGUAGE = new InjectionToken<Record<string, any>>('DATA_TABLE_LANGUAGE');

export interface DataTableColumn {
  data: string;
  title: string;
  render?: (value: any, type: string, row: any) => string;
  className?: string;
  width?: string;
  orderable?: boolean;
  searchable?: boolean;
}

export interface DataTableMenuItem {
  name: string;
  label: string;
  visible?: (row: any) => boolean;
  divider?: boolean;
}

export interface DataTableAction {
  name: string;
  label: string;
  icon: 'pencil' | 'trash' | 'eye' | 'key' | 'ban' | 'circle-check' | 'tasks' | 'user-plus';
  visible?: (row: any) => boolean;
  disabled?: (row: any) => boolean;
  menu?: DataTableMenuItem[] | ((row: any) => DataTableMenuItem[]);
}

export interface DataTableActionEvent {
  action: string;
  row: any;
}

const ICONS: Record<DataTableAction['icon'], string> = {
  pencil: '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"/><path d="m15 5 4 4"/></svg>',
  trash: '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/><line x1="10" x2="10" y1="11" y2="17"/><line x1="14" x2="14" y1="11" y2="17"/></svg>',
  eye: '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2.062 12.348a1 1 0 0 1 0-.696 10.75 10.75 0 0 1 19.876 0 1 1 0 0 1 0 .696 10.75 10.75 0 0 1-19.876 0"/><circle cx="12" cy="12" r="3"/></svg>',
  key: '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m15.5 7.5 2.3 2.3a1 1 0 0 0 1.4 0l2.1-2.1a1 1 0 0 0 0-1.4L19 4"/><path d="m21 2-9.6 9.6"/><circle cx="7.5" cy="15.5" r="5.5"/></svg>',
  ban: '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="m4.9 4.9 14.2 14.2"/></svg>',
  'circle-check': '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg>',
  tasks: '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m3 17 2 2 4-4"/><path d="m3 7 2 2 4-4"/><path d="M13 6h8"/><path d="M13 12h8"/><path d="M13 18h8"/></svg>',
  'user-plus': '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 21a8 8 0 0 1 13.292-6"/><circle cx="10" cy="8" r="5"/><path d="M19 16v6"/><path d="M22 19h-6"/></svg>'
};

function escapeHtml(value: unknown): string {
  return String(value ?? '')
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

@Component({
  standalone: false,
  selector: 'app-data-table',
  templateUrl: './data-table.component.html'
})
export class DataTableComponent implements AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('table') tableRef: ElementRef<HTMLTableElement>;

  @Input({ required: true }) columns: DataTableColumn[] = [];
  @Input() data: any[] | null = null;
  @Input() actions: DataTableAction[] = [];
  @Input() options: Record<string, any> = {};
  @Input() exportTitle = '';
  @Input() rowId = 'id';
  @Input() selectable = false;
  @Input() columnFilters = false;
  @Input() footer = false;

  @Output() action = new EventEmitter<DataTableActionEvent>();
  @Output() selectionChange = new EventEmitter<any[]>();

  private table: any = null;
  private responsiveEnabled = false;
  private readonly isBrowser: boolean;
  private readonly onClick = (event: Event) => this.handleClick(event);
  private readonly onToggle = (event: Event) => this.handleToggle(event);
  private readonly onDocumentClick = (event: Event) => this.closeMenus(event);

  constructor(
    @Inject(PLATFORM_ID) platformId: Object,
    @Optional() @Inject(DATA_TABLE_LANGUAGE) private language: Record<string, any> | null
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngAfterViewInit(): void {
    if (!this.isBrowser) {
      return;
    }
    if (this.footer) {
      this.addFooter();
    }
    this.table = new DataTable(this.tableRef.nativeElement, this.buildOptions());
    this.tableRef.nativeElement.closest('.dt-container')
      ?.querySelectorAll<HTMLInputElement>('.dt-search input')
      .forEach(input => {
        input.setAttribute('autocomplete', 'off');
        input.setAttribute('name', 'dt-search-' + Math.random().toString(36).slice(2));
        input.setAttribute('readonly', '');
        const unlock = () => input.removeAttribute('readonly');
        input.addEventListener('focus', unlock);
        input.addEventListener('pointerdown', unlock);
      });
    this.tableRef.nativeElement.addEventListener('click', this.onClick);
    this.tableRef.nativeElement.addEventListener('toggle', this.onToggle, true);
    document.addEventListener('click', this.onDocumentClick);
    if (this.selectable) {
      this.table.on('select deselect', () => this.selectionChange.emit(this.selectedRows()));
    }
    if (this.columnFilters) {
      this.addColumnFilters();
    }
  }

  private addFooter(): void {
    const tfoot = document.createElement('tfoot');
    const row = document.createElement('tr');
    const total = this.columns.length + (this.actions.length ? 1 : 0);
    for (let i = 0; i < total; i++) {
      row.appendChild(document.createElement('th'));
    }
    tfoot.appendChild(row);
    this.tableRef.nativeElement.appendChild(tfoot);
  }

  private addColumnFilters(): void {
    const thead = this.tableRef.nativeElement.querySelector('thead');
    if (!thead) {
      return;
    }
    const row = document.createElement('tr');
    row.className = 'dt-column-filters';
    this.table.columns().every((index: number) => {
      const column = this.table.column(index);
      const cell = document.createElement('th');
      if (index < this.columns.length && (this.columns[index].searchable ?? true)) {
        const input = document.createElement('input');
        input.type = 'search';
        input.setAttribute('autocomplete', 'off');
        input.setAttribute('aria-label', this.columns[index].title);
        input.addEventListener('input', () => {
          if (column.search() !== input.value) {
            column.search(input.value).draw();
          }
        });
        input.addEventListener('click', event => event.stopPropagation());
        cell.appendChild(input);
      }
      row.appendChild(cell);
    });
    thead.appendChild(row);

    const sync = () => {
      this.table.columns().every((index: number) => {
        const cell = row.children[index] as HTMLElement | undefined;
        if (cell) {
          cell.style.display = this.table.column(index).visible() && !this.isResponsiveHidden(index) ? '' : 'none';
        }
      });
    };
    this.table.on('column-visibility responsive-resize', sync);
    sync();
  }

  private isResponsiveHidden(index: number): boolean {
    if (!this.responsiveEnabled) {
      return false;
    }
    const column = this.table.column(index);
    try {
      return typeof column.responsiveHidden === 'function' ? column.responsiveHidden() === false : false;
    } catch {
      return false;
    }
  }

  selectedRows(): any[] {
    return this.table ? this.table.rows({ selected: true }).data().toArray() : [];
  }

  clearSelection(): void {
    this.table?.rows().deselect();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.table) {
      this.table.clear().rows.add(this.data ?? []).draw(false);
      if (this.selectable) {
        this.selectionChange.emit([]);
      }
    }
  }

  ngOnDestroy(): void {
    this.tableRef?.nativeElement.removeEventListener('click', this.onClick);
    this.tableRef?.nativeElement.removeEventListener('toggle', this.onToggle, true);
    document.removeEventListener('click', this.onDocumentClick);
    this.table?.destroy();
    this.table = null;
  }

  private buildOptions(): Record<string, any> {
    const columns: any[] = this.columns.map(c => ({
      data: c.data,
      title: c.title,
      render: c.render ?? ((value: unknown) => escapeHtml(value)),
      className: c.className,
      width: c.width,
      orderable: c.orderable ?? true,
      searchable: c.searchable ?? true
    }));

    if (this.actions.length) {
      columns.push({
        data: null,
        title: '',
        className: 'dt-actions',
        orderable: false,
        searchable: false,
        width: `${this.actions.length * 2.25}rem`,
        render: (_: unknown, type: string, row: any) => type === 'display' ? this.renderActions(row) : ''
      });
    }

    const defaults: Record<string, any> = {
      data: this.data ?? [],
      columns,
      rowId: this.rowId,
      pageLength: 10,
      lengthMenu: [10, 25, 50, 100],
      responsive: false,
      stateSave: false,
      select: this.selectable ? { style: 'multi', selector: 'td:not(.dt-actions)' } : false,
      orderClasses: false,
      order: [[0, 'asc']],
      language: { search: '', searchPlaceholder: 'Search…', ...(this.language ?? {}) },
      layout: {
        topStart: 'search',
        topEnd: 'buttons',
        bottomStart: 'info',
        bottomEnd: 'paging'
      },
      buttons: [
        { extend: 'copyHtml5', text: 'Copy', title: this.exportTitle, exportOptions: { columns: ':not(.dt-actions)' } },
        { extend: 'excelHtml5', text: 'Excel', title: this.exportTitle, exportOptions: { columns: ':not(.dt-actions)' } },
        { extend: 'pdfHtml5', text: 'PDF', title: this.exportTitle, orientation: 'landscape', pageSize: 'A4', exportOptions: { columns: ':not(.dt-actions)' } },
        { extend: 'print', text: 'Print', title: this.exportTitle, exportOptions: { columns: ':not(.dt-actions)' } }
      ]
    };

    const merged = { ...defaults, ...this.options };
    this.responsiveEnabled = !!merged['responsive'];
    return merged;
  }

  private renderActions(row: any): string {
    return this.actions
      .filter(a => a.visible ? a.visible(row) : true)
      .map(a => {
        const disabled = a.disabled?.(row) ? ' disabled' : '';
        if (a.menu) {
          const items = (typeof a.menu === 'function' ? a.menu(row) : a.menu)
            .filter(i => i.visible ? i.visible(row) : true);
          const list = items
            .map(i => i.divider ? '<li class="dt-row-menu-divider"></li>' : `<li><button type="button" class="dt-action" data-action="${i.name}">${escapeHtml(i.label)}</button></li>`)
            .join('');
          return `<details class="dt-row-menu"><summary class="icon-btn" title="${escapeHtml(a.label)}" aria-label="${escapeHtml(a.label)}">${ICONS[a.icon]}</summary><ul>${list}</ul></details>`;
        }
        return `<button type="button" class="icon-btn dt-action" data-action="${a.name}" aria-label="${escapeHtml(a.label)}" title="${escapeHtml(a.label)}"${disabled}>${ICONS[a.icon]}</button>`;
      })
      .join('');
  }

  private handleClick(event: Event): void {
    const button = (event.target as HTMLElement).closest<HTMLElement>('button.dt-action');
    if (!button) {
      return;
    }
    const tr = button.closest('tr');
    const row = tr ? this.table.row(tr).data() : null;
    button.closest<HTMLDetailsElement>('details.dt-row-menu')?.removeAttribute('open');
    if (row) {
      this.action.emit({ action: button.dataset['action']!, row });
    }
  }

  private handleToggle(event: Event): void {
    const details = event.target as HTMLDetailsElement;
    if (!details.classList.contains('dt-row-menu')) {
      return;
    }
    if (details.open) {
      this.tableRef.nativeElement.querySelectorAll<HTMLDetailsElement>('details.dt-row-menu[open]')
        .forEach(other => {
          if (other !== details) {
            other.removeAttribute('open');
          }
        });
    }
    const anyOpen = !!this.tableRef.nativeElement.querySelector('details.dt-row-menu[open]');
    this.tableRef.nativeElement.closest<HTMLElement>('.dt-layout-table')?.classList.toggle('dt-menu-open', anyOpen);
  }

  private closeMenus(event: Event): void {
    if ((event.target as HTMLElement).closest('details.dt-row-menu')) {
      return;
    }
    this.tableRef?.nativeElement.querySelectorAll<HTMLDetailsElement>('details.dt-row-menu[open]')
      .forEach(details => details.removeAttribute('open'));
  }
}
