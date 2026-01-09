import { Component } from '@angular/core';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, CellClassParams } from 'ag-grid-community';
import { CommonModule } from '@angular/common';

interface GridData {
  maVatLieu: string;
  tenLinhKien: string;
  keHoach: number;
  tong: number;
  khachHang: string;
  nov30: number;
  dec1: number;
  dec2: number;
  dec3: number;
  dec4: number;
  dec5: number;
  dec6: number;
  dec7: number;
  dec8: number;
  dec9: number;
  dec10: number;
  dec11: number;
  dec12: number;
  dec13: number;
  rowType?: string;
}

@Component({
  selector: 'app-pmc',
  standalone: true,
  imports: [AgGridAngular, CommonModule],
  templateUrl: './pmc.component.html',
  styleUrls: ['./pmc.component.scss']
})
export class PmcComponent {
  title = 'Bảng Kế Hoạch Sản Xuất PMC';

  // Column Definitions
  colDefs: ColDef<GridData>[] = [
    { 
      field: 'maVatLieu', 
      headerName: 'Mã vật liệu',
      width: 150,
      pinned: 'left',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'tenLinhKien', 
      headerName: 'Tên linh kiện',
      width: 200,
      pinned: 'left',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        if (params.data?.rowType === 'highlight') return 'cell-yellow';
        if (params.data?.rowType === 'green') return 'cell-green';
        return '';
      }
    },
    { 
      field: 'keHoach', 
      headerName: 'Kế hoạch',
      width: 120,
      type: 'numericColumn',
      valueFormatter: (params) => {
        if (params.value == null) return '';
        return params.value.toLocaleString('vi-VN');
      },
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        if (params.data?.rowType === 'highlight') return 'cell-yellow';
        if (params.data?.rowType === 'green') return 'cell-green';
        return '';
      }
    },
    { 
      field: 'tong', 
      headerName: 'Tổng',
      width: 100,
      type: 'numericColumn',
      valueFormatter: (params) => {
        if (params.value == null) return '';
        return params.value.toLocaleString('vi-VN');
      },
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        if (params.data?.rowType === 'highlight') return 'cell-yellow';
        if (params.data?.rowType === 'green') return 'cell-green';
        return '';
      }
    },
    { 
      field: 'khachHang', 
      headerName: 'Khách hàng',
      width: 150,
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        if (params.data?.rowType === 'highlight') return 'cell-yellow';
        if (params.data?.rowType === 'green') return 'cell-green';
        return '';
      }
    },
    { 
      field: 'nov30', 
      headerName: '30-Nov',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        if (params.data?.rowType === 'highlight') return 'cell-yellow';
        return '';
      }
    },
    { 
      field: 'dec1', 
      headerName: '1-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec2', 
      headerName: '2-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec3', 
      headerName: '3-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec4', 
      headerName: '4-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec5', 
      headerName: '5-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec6', 
      headerName: '6-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        if (params.data?.rowType === 'highlight') return 'cell-yellow';
        return '';
      }
    },
    { 
      field: 'dec7', 
      headerName: '7-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec8', 
      headerName: '8-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec9', 
      headerName: '9-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec10', 
      headerName: '10-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec11', 
      headerName: '11-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec12', 
      headerName: '12-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    },
    { 
      field: 'dec13', 
      headerName: '13-Dec',
      width: 90,
      type: 'numericColumn',
      cellClass: (params: CellClassParams) => {
        if (params.data?.rowType === 'header') return 'cell-header';
        return '';
      }
    }
  ];

  // Row Data - Mock data
  rowData: GridData[] = [
    {
      maVatLieu: '',
      tenLinhKien: '',
      keHoach: 0,
      tong: 0,
      khachHang: '#CN Cần SDV',
      nov30: 0,
      dec1: 10,
      dec2: 5,
      dec3: 1,
      dec4: 1,
      dec5: 2,
      dec6: 5,
      dec7: 0,
      dec8: 4,
      dec9: 4,
      dec10: 3,
      dec11: 0,
      dec12: 0,
      dec13: 0
    },
    {
      maVatLieu: '',
      tenLinhKien: '',
      keHoach: 0,
      tong: 0,
      khachHang: '#CN Cần VM',
      nov30: 0,
      dec1: 5,
      dec2: 4,
      dec3: 3,
      dec4: 1,
      dec5: 1,
      dec6: 4,
      dec7: 0,
      dec8: 0,
      dec9: 0,
      dec10: 0,
      dec11: 0,
      dec12: 0,
      dec13: 0
    },
    {
      maVatLieu: '',
      tenLinhKien: '',
      keHoach: 0,
      tong: 0,
      khachHang: '#CN Cần LUCKY',
      nov30: 0,
      dec1: 26,
      dec2: 32,
      dec3: 37,
      dec4: 38,
      dec5: 32,
      dec6: 27,
      dec7: 0,
      dec8: 28,
      dec9: 30,
      dec10: 29,
      dec11: 31,
      dec12: 30,
      dec13: 30
    },
    {
      maVatLieu: '',
      tenLinhKien: '',
      keHoach: 0,
      tong: 0,
      khachHang: 'TỔNG CẦN CN',
      nov30: 0,
      dec1: 41,
      dec2: 41,
      dec3: 41,
      dec4: 40,
      dec5: 35,
      dec6: 36,
      dec7: 0,
      dec8: 32,
      dec9: 34,
      dec10: 32,
      dec11: 31,
      dec12: 30,
      dec13: 30,
      rowType: 'header'
    },
    {
      maVatLieu: '',
      tenLinhKien: '',
      keHoach: 0,
      tong: 0,
      khachHang: '#CN Có',
      nov30: 41,
      dec1: 41,
      dec2: 41,
      dec3: 41,
      dec4: 41,
      dec5: 41,
      dec6: 41,
      dec7: 42,
      dec8: 43,
      dec9: 44,
      dec10: 45,
      dec11: 46,
      dec12: 47,
      dec13: 48,
      rowType: 'highlight'
    },
    {
      maVatLieu: '18084',
      tenLinhKien: '',
      keHoach: 127930240,
      tong: 0,
      khachHang: 'DT THEO KHSX',
      nov30: 0,
      dec1: 23735435,
      dec2: 21149060,
      dec3: 0,
      dec4: 21024560,
      dec5: 0,
      dec6: 0,
      dec7: 0,
      dec8: 19097560,
      dec9: 19762560,
      dec10: 16308060,
      dec11: 14028560,
      dec12: 13958060,
      dec13: 13887560
    },
    {
      maVatLieu: '4.0',
      tenLinhKien: '',
      keHoach: 147600000,
      tong: 87,
      khachHang: 'DT THEO VCLTĐT',
      nov30: 0,
      dec1: 24600000,
      dec2: 24600000,
      dec3: 0,
      dec4: 24600000,
      dec5: 0,
      dec6: 0,
      dec7: 24600000,
      dec8: 24600000,
      dec9: 24600000,
      dec10: 24600000,
      dec11: 24600000,
      dec12: 24600000,
      dec13: 24600000,
      rowType: 'highlight'
    },
    {
      maVatLieu: '',
      tenLinhKien: '',
      keHoach: 0,
      tong: 0,
      khachHang: 'VL CTL',
      nov30: 0,
      dec1: 0,
      dec2: 86,
      dec3: 86,
      dec4: 89,
      dec5: 85,
      dec6: 0,
      dec7: 89,
      dec8: 81,
      dec9: 0,
      dec10: 0,
      dec11: 0,
      dec12: 0,
      dec13: 0
    },
    {
      maVatLieu: '',
      tenLinhKien: '',
      keHoach: 0,
      tong: 0,
      khachHang: '#CN Thiếu',
      nov30: 41,
      dec1: 0,
      dec2: 0,
      dec3: 0,
      dec4: 1,
      dec5: 6,
      dec6: 5,
      dec7: 42,
      dec8: 11,
      dec9: 10,
      dec10: 13,
      dec11: 15,
      dec12: 17,
      dec13: 18,
      rowType: 'highlight'
    },
    {
      maVatLieu: 'H02-2027$PV4-001',
      tenLinhKien: '28508-F23 ||圓 Mặt',
      keHoach: 40000,
      tong: 0,
      khachHang: 'Yêu cầu KH POK早 Kế hoạch',
      nov30: 0,
      dec1: 0,
      dec2: 0,
      dec3: 0,
      dec4: 0,
      dec5: 0,
      dec6: 0,
      dec7: 0,
      dec8: 0,
      dec9: 0,
      dec10: 0,
      dec11: 0,
      dec12: 0,
      dec13: 0
    },
    {
      maVatLieu: '',
      tenLinhKien: '',
      keHoach: 30800,
      tong: 0,
      khachHang: 'Kế hoạch',
      nov30: 0,
      dec1: 18800,
      dec2: 0,
      dec3: 2000,
      dec4: 0,
      dec5: 0,
      dec6: 0,
      dec7: 0,
      dec8: 0,
      dec9: 0,
      dec10: 0,
      dec11: 0,
      dec12: 0,
      dec13: 0
    },
    {
      maVatLieu: '',
      tenLinhKien: '',
      keHoach: 0,
      tong: 0,
      khachHang: '# Kẹp 調進用',
      nov30: 0,
      dec1: 1,
      dec2: 0,
      dec3: 1,
      dec4: 0,
      dec5: 0,
      dec6: 0,
      dec7: 0,
      dec8: 0,
      dec9: 0,
      dec10: 0,
      dec11: 0,
      dec12: 0,
      dec13: 0
    },
    {
      maVatLieu: 'H03-69503PCC-002',
      tenLinhKien: 'PRM233 曲位 chỗ ngồi',
      keHoach: 8500,
      tong: 0,
      khachHang: 'Yêu cầu KH POK早 Kế hoạch',
      nov30: 0,
      dec1: 0,
      dec2: 2500,
      dec3: 3000,
      dec4: 3000,
      dec5: 0,
      dec6: 0,
      dec7: 0,
      dec8: 0,
      dec9: 0,
      dec10: 3000,
      dec11: 0,
      dec12: 3000,
      dec13: 0
    },
    {
      maVatLieu: '700A 年夢（編油',
      tenLinhKien: '',
      keHoach: 28000,
      tong: 0,
      khachHang: 'Yêu cầu KH POK早 Kế hoạch',
      nov30: 0,
      dec1: 17000,
      dec2: 0,
      dec3: 2000,
      dec4: 0,
      dec5: 0,
      dec6: 0,
      dec7: 0,
      dec8: 0,
      dec9: 0,
      dec10: 0,
      dec11: 0,
      dec12: 0,
      dec13: 0,
      rowType: 'green'
    }
  ];

  // Default column definition
  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true,
    editable: true  // Enable editing for all cells
  };

  // Grid options
  public gridOptions = {
    domLayout: 'autoHeight' as const,
    suppressHorizontalScroll: false,
    enableCellTextSelection: true,
    ensureDomOrder: true
  };

  // Handle cell value changes
  onCellValueChanged(event: any) {
    console.log('Cell value changed:', event);
  }
}
