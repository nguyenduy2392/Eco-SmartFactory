import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import Handsontable from 'handsontable';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { PMCService, PMCWeekDto, PMCRowDto, SavePMCWeekRequest, SavePMCRowRequest, PMCWeekListItemDto } from '../../../services/pmc.service';
import { format } from 'date-fns';

@Component({
  selector: 'app-pmc',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, DropdownModule, TooltipModule, ToastModule],
  providers: [MessageService],
  templateUrl: './pmc.component.html',
  styleUrls: ['./pmc.component.scss']
})
export class PmcComponent implements OnInit, AfterViewInit {
  title = 'Bảng Kế Hoạch Sản Xuất PMC';
  
  @ViewChild('hotContainer', { static: false }) hotContainer?: ElementRef;
  
  // Handsontable config
  hotSettings: Handsontable.GridSettings = {};
  hotData: any[][] = [];
  private hotInstance?: Handsontable;
  
  // PMC data
  currentWeek?: PMCWeekDto;
  weekVersions: PMCWeekListItemDto[] = [];
  selectedVersion?: PMCWeekListItemDto;
  loading = false;
  hasChanges = false;
  
  // Grid metadata
  private mergeCells: any[] = [];
  private rowMetadata: Map<number, PMCRowDto> = new Map();
  
  constructor(
    private pmcService: PMCService,
    private messageService: MessageService
  ) {}
  
  ngOnInit(): void {
    this.initHandsontable();
    this.loadCurrentWeek();
  }
  
  ngAfterViewInit(): void {
    // Initialize Handsontable after view init
    setTimeout(() => {
      if (this.hotContainer) {
        this.hotInstance = new Handsontable(this.hotContainer.nativeElement, this.hotSettings);
        console.log('Handsontable initialized');
        
        // If we already have data, render it now
        if (this.hotData.length > 0) {
          console.log('Rendering existing data:', this.hotData.length, 'rows');
          this.renderHandsontable();
        }
      }
    }, 100);
  }
  
  /**
   * Render Handsontable with current data
   */
  private renderHandsontable() {
    if (!this.hotInstance) {
      console.log('Handsontable instance not ready, will render when ready');
      return;
    }
    
    console.log('Rendering Handsontable with', this.hotData.length, 'rows');
    this.hotInstance.loadData(this.hotData);
    this.hotInstance.updateSettings({
      colHeaders: this.hotSettings.colHeaders,
      mergeCells: this.mergeCells
    });
    this.hotInstance.render();
  }
  
  /**
   * Initialize Handsontable settings
   */
  initHandsontable() {
    this.hotSettings = {
      licenseKey: 'non-commercial-and-evaluation',
      data: [],
      colHeaders: true,
      rowHeaders: false,
      contextMenu: ['copy', 'cut'],
      height: 'auto',
      width: '100%',
      stretchH: 'all',
      manualColumnResize: true,
      manualRowResize: false,
      fixedColumnsStart: 5, // Freeze first 5 columns
      fixedRowsTop: 0,
      mergeCells: [],
      cells: this.cellRenderer.bind(this),
      afterChange: this.onCellChange.bind(this),
      beforeChange: this.beforeCellChange.bind(this),
    };
  }
  
  /**
   * Cell renderer for styling
   */
  cellRenderer(row: number, col: number): any {
    const cellProperties: any = {};
    
    const rowData = this.rowMetadata.get(row);
    if (!rowData) return cellProperties;
    
    // Fixed columns styling
    if (col < 5) {
      cellProperties.readOnly = true;
      cellProperties.className = 'fixed-column';
    }
    
    // Plan type based coloring
    if (rowData.planType === 'REQUIREMENT') {
      cellProperties.className = (cellProperties.className || '') + ' cell-requirement';
      cellProperties.readOnly = true;
    } else if (rowData.planType === 'PRODUCTION') {
      cellProperties.className = (cellProperties.className || '') + ' cell-production';
      cellProperties.readOnly = col < 5; // Allow editing date columns
    } else if (rowData.planType === 'CLAMP') {
      cellProperties.className = (cellProperties.className || '') + ' cell-clamp';
      cellProperties.readOnly = col < 5;
    }
    
    // Numeric columns
    if (col >= 4) {
      cellProperties.type = 'numeric';
      cellProperties.numericFormat = {
        pattern: '0,0',
        culture: 'vi-VN'
      };
    }
    
    return cellProperties;
  }
  
  /**
   * Load current or next week PMC
   */
  loadCurrentWeek() {
    this.loading = true;
    this.pmcService.getPMCWeek().subscribe({
      next: (week) => {
        console.log('Received PMC week from backend:', week);
        if (week) {
          this.currentWeek = week;
          console.log('Current week rows count:', week.rows?.length || 0);
          this.buildGrid();
          this.loadWeekVersions();
        } else {
          // No PMC found, prompt to create
          this.messageService.add({
            severity: 'info',
            summary: 'Chưa có PMC',
            detail: 'Chưa có PMC cho tuần này. Bấm "Tạo PMC Tuần Mới" để bắt đầu.'
          });
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading PMC week:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải dữ liệu PMC'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Load all versions of current week
   */
  loadWeekVersions() {
    if (!this.currentWeek) return;
    
    const weekStart = this.currentWeek.weekStartDate;
    this.pmcService.getPMCWeeks(weekStart, weekStart).subscribe({
      next: (versions) => {
        this.weekVersions = versions;
        this.selectedVersion = versions.find(v => v.id === this.currentWeek!.id);
      },
      error: (error) => {
        console.error('Error loading week versions:', error);
      }
    });
  }
  
  /**
   * Build Handsontable data from PMC
   */
  buildGrid() {
    if (!this.currentWeek) {
      console.log('No currentWeek data');
      return;
    }
    
    console.log('Building grid with week:', this.currentWeek);
    console.log('Week has rows:', this.currentWeek.rows?.length || 0);
    console.log('Week rows detail:', this.currentWeek.rows);
    
    // Build column headers
    const headers = [
      'Mã vật liệu',
      'Tên linh kiện',
      'Kế hoạch',
      'Tổng',
      'Khách hàng',
      ...this.currentWeek.weekDates.map(date => format(new Date(date), 'd-MMM'))
    ];
    
    console.log('Headers:', headers);
    
    // Build data rows
    const data: any[][] = [];
    this.mergeCells = [];
    this.rowMetadata.clear();
    
    const groupedRows = this.groupRowsByProduct();
    let rowIndex = 0;
    
    console.log('Grouped rows:', groupedRows);
    
    groupedRows.forEach(group => {
      const startRow = rowIndex;
      
      group.rows.forEach((row, idx) => {
        const rowData = [
          group.productCode,
          group.componentName,
          row.planTypeDisplay,
          row.totalValue || 0,
          group.customerName || '',
          ...row.cells.map(cell => cell.value || 0)
        ];
        
        data.push(rowData);
        this.rowMetadata.set(rowIndex, row);
        rowIndex++;
      });
      
      const endRow = rowIndex - 1;
      const rowCount = endRow - startRow + 1;
      
      // Merge cells for product code, component name, and customer
      if (rowCount > 1) {
        this.mergeCells.push({ row: startRow, col: 0, rowspan: rowCount, colspan: 1 }); // Product code
        this.mergeCells.push({ row: startRow, col: 1, rowspan: rowCount, colspan: 1 }); // Component name
        this.mergeCells.push({ row: startRow, col: 4, rowspan: rowCount, colspan: 1 }); // Customer
      }
    });
    
    this.hotData = data;
    
    console.log('Final grid data:', data);
    console.log('Merge cells:', this.mergeCells);
    
    // Update settings with new data
    this.hotSettings = {
      ...this.hotSettings,
      colHeaders: headers,
      data: data,
      mergeCells: this.mergeCells,
      columns: headers.map((_, idx) => {
        if (idx >= 3) {
          return { type: 'numeric', numericFormat: { pattern: '0,0' } };
        }
        return { type: 'text' };
      })
    };
    
    // Render if instance exists
    this.renderHandsontable();
  }
  
  /**
   * Group rows by product and component
   */
  groupRowsByProduct() {
    if (!this.currentWeek) return [];
    
    const groups = new Map<string, {
      productCode: string;
      componentName: string;
      customerName?: string;
      rows: PMCRowDto[];
    }>();
    
    this.currentWeek.rows.forEach(row => {
      if (!groups.has(row.rowGroup)) {
        groups.set(row.rowGroup, {
          productCode: row.productCode,
          componentName: row.componentName,
          customerName: row.customerName,
          rows: []
        });
      }
      groups.get(row.rowGroup)!.rows.push(row);
    });
    
    return Array.from(groups.values());
  }
  
  /**
   * Handle cell value changes
   */
  onCellChange(changes: any, source: string) {
    if (!changes || source === 'loadData') return;
    
    this.hasChanges = true;
    console.log('Cell changed:', changes);
  }
  
  /**
   * Before cell change validation
   */
  beforeCellChange(changes: any, source: string) {
    if (!changes) return;
    
    // Validate numeric input
    changes.forEach((change: any) => {
      const [row, col, oldVal, newVal] = change;
      
      // Only allow numbers in date columns
      if (col >= 5 && newVal !== null && newVal !== '') {
        const num = Number(newVal);
        if (isNaN(num)) {
          change[3] = oldVal; // Revert to old value
          this.messageService.add({
            severity: 'warn',
            summary: 'Giá trị không hợp lệ',
            detail: 'Chỉ được nhập số'
          });
        }
      }
    });
  }
  
  /**
   * Create new PMC week
   */
  createNewWeek(copyFromPrevious: boolean = false) {
    this.loading = true;
    this.pmcService.createPMCWeek({
      copyFromPreviousWeek: copyFromPrevious
    }).subscribe({
      next: (week) => {
        console.log('Created PMC week:', week);
        console.log('New week rows count:', week.rows?.length || 0);
        this.currentWeek = week;
        this.buildGrid();
        this.loadWeekVersions();
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: `Đã tạo PMC ${week.weekName}`
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error creating PMC week:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tạo PMC mới'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Save current PMC (creates new version)
   */
  saveCurrentWeek() {
    if (!this.currentWeek || !this.hasChanges || !this.hotInstance) return;
    
    this.loading = true;
    
    // Build save request from grid data
    const rows: SavePMCRowRequest[] = [];
    const data = this.hotInstance.getData();
    
    data.forEach((rowData, rowIndex) => {
      const metadata = this.rowMetadata.get(rowIndex);
      if (!metadata) return;
      
      const cellValues: { [key: string]: number } = {};
      
      // Start from column 5 (date columns)
      this.currentWeek!.weekDates.forEach((dateStr, index) => {
        const value = rowData[5 + index];
        cellValues[dateStr] = Number(value) || 0;
      });
      
      rows.push({
        id: metadata.id,
        productCode: rowData[0],
        componentName: rowData[1],
        planType: metadata.planType,
        totalValue: Number(rowData[3]) || 0,
        cellValues: cellValues
      });
    });
    
    const request: SavePMCWeekRequest = {
      pmcWeekId: this.currentWeek.id,
      rows: rows
    };
    
    this.pmcService.savePMCWeek(request).subscribe({
      next: (week) => {
        this.currentWeek = week;
        this.buildGrid();
        this.loadWeekVersions();
        this.hasChanges = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Đã lưu',
          detail: `Đã lưu version ${week.version}`
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error saving PMC week:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể lưu PMC'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Load previous week
   */
  loadPreviousWeek() {
    if (!this.currentWeek) return;
    
    this.loading = true;
    this.pmcService.getPreviousPMCWeek(this.currentWeek.weekStartDate).subscribe({
      next: (week) => {
        this.currentWeek = week;
        this.buildGrid();
        this.loadWeekVersions();
        this.messageService.add({
          severity: 'info',
          summary: 'Đã chuyển',
          detail: `Đang xem ${week.weekName}`
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading previous week:', error);
        this.messageService.add({
          severity: 'warn',
          summary: 'Không có dữ liệu',
          detail: 'Không tìm thấy PMC tuần trước'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Load next week
   */
  loadNextWeek() {
    if (!this.currentWeek) return;
    
    const nextWeekDate = new Date(this.currentWeek.weekStartDate);
    nextWeekDate.setDate(nextWeekDate.getDate() + 7);
    
    this.loading = true;
    this.pmcService.getPMCWeek(undefined, nextWeekDate.toISOString().split('T')[0]).subscribe({
      next: (week) => {
        this.currentWeek = week;
        this.buildGrid();
        this.loadWeekVersions();
        this.messageService.add({
          severity: 'info',
          summary: 'Đã chuyển',
          detail: `Đang xem ${week.weekName}`
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading next week:', error);
        this.messageService.add({
          severity: 'warn',
          summary: 'Không có dữ liệu',
          detail: 'Không tìm thấy PMC tuần sau'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Load specific version
   */
  onVersionChange() {
    if (!this.selectedVersion) return;
    
    this.loading = true;
    this.pmcService.getPMCWeek(this.selectedVersion.id).subscribe({
      next: (week) => {
        this.currentWeek = week;
        this.buildGrid();
        this.messageService.add({
          severity: 'info',
          summary: 'Đã chuyển',
          detail: `Đang xem version ${week.version}`
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading version:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải version này'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Debug: Check available POs for PMC
   */
  debugAvailablePOs() {
    this.loading = true;
    
    // Try debug endpoint first
    this.pmcService.getAvailablePOs().subscribe({
      next: (result) => {
        console.log('=== DEBUG: Available POs for PMC ===');
        console.log('Raw response:', result);
        console.log('Total POs:', result?.totalPOs ?? result?.TotalPOs);
        console.log('Eligible POs:', result?.eligiblePOsCount ?? result?.EligiblePOsCount);
        console.log('Eligible POs Detail:', result?.eligiblePOs ?? result?.EligiblePOs);
        console.log('By Processing Type:', result?.allPOsGroupByProcessingType ?? result?.AllPOsGroupByProcessingType);
        console.log('By Status:', result?.allPOsGroupByStatus ?? result?.AllPOsGroupByStatus);
        console.log('====================================');
        
        const totalPOs = result?.totalPOs ?? result?.TotalPOs ?? 0;
        const eligibleCount = result?.eligiblePOsCount ?? result?.EligiblePOsCount ?? 0;
        
        let message = `Tìm thấy ${eligibleCount}/${totalPOs} PO phù hợp.`;
        
        if (eligibleCount === 0 && totalPOs > 0) {
          message += ' Có thể do: ProcessingType không đúng, Status không phù hợp, hoặc PO chưa có Products/Parts.';
        } else if (totalPOs === 0) {
          message += ' Chưa có PO nào trong hệ thống.';
        }
        
        this.messageService.add({
          severity: eligibleCount > 0 ? 'success' : 'warn',
          summary: 'Debug Info',
          detail: message + ' Xem Console (F12) để biết chi tiết.'
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error checking available POs:', error);
        console.error('Error detail:', error.error);
        console.error('Error status:', error.status);
        
        // If endpoint not found (404), API might not be rebuilt
        if (error.status === 404) {
          this.messageService.add({
            severity: 'warn',
            summary: 'API chưa được cập nhật',
            detail: 'Debug endpoint chưa có. Vui lòng rebuild API backend.'
          });
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: error.error?.message || error.message || 'Không thể kiểm tra PO'
          });
        }
        this.loading = false;
      }
    });
  }
}
