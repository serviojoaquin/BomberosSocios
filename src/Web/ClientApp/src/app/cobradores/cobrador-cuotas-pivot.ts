import { DataTableColumn } from '../shared/data-table/data-table.component';

export function formatPivotImporte(value: number | null | undefined): string {
  if (value === null || value === undefined) {
    return '';
  }
  return new Intl.NumberFormat('es-AR', { minimumFractionDigits: 0, maximumFractionDigits: 2 }).format(value);
}

export function pivotColumns(): DataTableColumn[] {
  const columns: DataTableColumn[] = [
    { data: 'socioId', title: 'socios.numero', className: 'dt-body-right' },
    { data: 'socioNombre', title: 'cobradores.socio' }
  ];
  for (let i = 0; i < 12; i++) {
    columns.push({
      data: `importes.${i}`,
      title: `C${i + 1}`,
      className: 'dt-body-right',
      orderable: false,
      render: (value: number | null, type: string) => type === 'display' ? formatPivotImporte(value) : String(value ?? '')
    });
  }
  columns.push({
    data: 'total',
    title: 'comun.total',
    className: 'dt-body-right',
    orderable: false,
    render: (value: number, type: string) => type === 'display' ? `<b>${formatPivotImporte(value)}</b>` : String(value ?? '')
  });
  return columns;
}

export function pivotPdfCustomize(alignment: 'right' | 'center'): (doc: any) => void {
  return (doc: any) => {
    doc.content[1].layout = {
      hLineWidth: () => 0.5,
      vLineWidth: () => 0.5,
      hLineColor: () => '#aaa',
      vLineColor: () => '#aaa',
      paddingLeft: () => 4,
      paddingRight: () => 4
    };
    doc.defaultStyle.alignment = alignment;
    doc.defaultStyle.fontSize = 9;
    const body = doc.content[1].table.body;
    const widths: string[] = [];
    for (let index = 0; index < body[0].length; index++) {
      widths.push(index === 1 ? '*' : index === body[0].length - 1 ? '9%' : '5%');
    }
    doc.content[1].table.widths = widths;
    for (let index = 0; index < body.length; index++) {
      body[index][1].alignment = index === 0 ? 'center' : 'left';
    }
  };
}
