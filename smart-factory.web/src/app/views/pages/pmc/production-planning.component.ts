import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { format, addDays, startOfWeek, addWeeks, startOfDay, nextMonday } from 'date-fns';
import { PMCService, PMCWeekDto, PMCRowDto, SavePMCWeekRequest, SavePMCRowRequest } from '../../../services/pmc.service';

interface DailyData {
  date: Date;
  dateStr: string;
  value: number;
  cellId?: string;
}

interface ProductionDetail {
  rowGroupId: string;
  materialCode: string;
  partName: string;
  plan: string;
  poTotal: number;
  planTotal: number;
  clampTotal: number;
  customer: string;
  customerId?: string;
  poRequirement: DailyData[];
  productionPlan: DailyData[];
  clampCount: DailyData[];
  poRowId?: string;
  planRowId?: string;
  clampRowId?: string;
}

interface SummaryData {
  label: string;
  values: number[];
  isPercentage?: boolean;
  isHighlight?: boolean;
  isRevenue?: boolean;
  isEditable?: boolean;
}

@Component({
  selector: 'app-production-planning',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    TooltipModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './production-planning.component.html',
  styleUrls: ['./production-planning.component.scss']
})
export class ProductionPlanningComponent implements OnInit {
  title = 'Kế Hoạch Sản Xuất Chi Tiết';
  
  // PMC data
  currentWeek?: PMCWeekDto;
  weekStartDate?: Date;
  weekEndDate?: Date;
  hasChanges = false;
  
  // Date columns (6 days: Monday to Saturday)
  dateColumns: { date: Date; dateStr: string; dayOfWeek: string }[] = [];
  
  // Summary data (Phần 1)
  summaryData: SummaryData[] = [];
  
  // Production details (Phần 2)
  productionDetails: ProductionDetail[] = [];
  
  // Workers available data (editable)
  workersAvailable: number[] = [];
  
  loading = false;
  
  constructor(
    private messageService: MessageService,
    private pmcService: PMCService
  ) {}
  
  ngOnInit(): void {
    this.loadCurrentWeek();
  }
  
  /**
   * Initialize date columns for next week (Monday to Saturday)
   */
  initializeDateColumns(startDate: Date): void {
    this.dateColumns = [];
    for (let i = 0; i < 6; i++) {
      const date = addDays(startDate, i);
      this.dateColumns.push({
        date: date,
        dateStr: format(date, 'd-MMM'),
        dayOfWeek: format(date, 'EEEEEE') // Short day name
      });
    }
    
    this.weekStartDate = startDate;
    this.weekEndDate = addDays(startDate, 5);
  }
  
  /**
   * Get current week's Monday or next Monday if today is Sunday
   */
  getCurrentOrNextMonday(): Date {
    const today = startOfDay(new Date());
    const dayOfWeek = today.getDay();
    
    // If Sunday (0), get next Monday
    if (dayOfWeek === 0) {
      return nextMonday(today);
    }
    
    // If Monday to Saturday, get current week's Monday
    return startOfWeek(today, { weekStartsOn: 1 });
  }
  
  /**
   * Get next Monday from current date (for next week)
   */
  getNextMonday(): Date {
    const today = startOfDay(new Date());
    return nextMonday(today);
  }
  
  /**
   * Load current week PMC data
   */
  loadCurrentWeek(): void {
    this.loading = true;
    const today = new Date();
    const currentMonday = startOfWeek(today, { weekStartsOn: 1 }); // Get Monday of current week
    const nextMonday = this.getNextMonday();
    
    // Try to load current week first, if not found, try next week
    const weekStart = format(currentMonday, 'yyyy-MM-dd');
    
    this.pmcService.getPMCWeek(undefined, weekStart).subscribe({
      next: (data) => {
        this.currentWeek = data;
        this.initializeDateColumns(new Date(data.weekStartDate));
        this.loadPMCData(data);
        this.loading = false;
      },
      error: (error) => {
        console.log('No PMC for current week, trying next week...');
        // Try next week
        const nextWeekStart = format(nextMonday, 'yyyy-MM-dd');
        this.pmcService.getPMCWeek(undefined, nextWeekStart).subscribe({
          next: (data) => {
            this.currentWeek = data;
            this.initializeDateColumns(new Date(data.weekStartDate));
            this.loadPMCData(data);
            this.loading = false;
          },
          error: (error2) => {
            console.error('Error loading PMC week:', error2);
            // If no PMC exists, show empty state for next week
            this.currentWeek = undefined;
            this.initializeDateColumns(nextMonday);
            this.productionDetails = [];
            this.initializeSummaryData();
            this.loading = false;
          }
        });
      }
    });
  }
  
  /**
   * Load PMC data from server response
   */
  loadPMCData(pmcWeek: PMCWeekDto): void {
    console.log('loadPMCData called with:', pmcWeek);
    console.log('Rows in response:', pmcWeek.rows?.length || 0);
    
    // Group rows by product/component
    const groupedRows = new Map<string, PMCRowDto[]>();
    
    pmcWeek.rows.forEach(row => {
      console.log('Processing row:', row.productCode, row.componentName, row.planType);
      const key = `${row.productCode}_${row.componentName}_${row.customerName || ''}`;
      if (!groupedRows.has(key)) {
        groupedRows.set(key, []);
      }
      groupedRows.get(key)!.push(row);
    });
    
    console.log('Grouped into', groupedRows.size, 'groups');
    
    // Convert to ProductionDetail format
    this.productionDetails = [];
    let groupIndex = 0;
    
    groupedRows.forEach((rows, key) => {
      console.log('Processing group:', key, 'with', rows.length, 'rows');
      
      const poRow = rows.find(r => r.planType === 'REQUIREMENT' || r.planType === 'PO_REQUIREMENT');
      const planRow = rows.find(r => r.planType === 'PRODUCTION' || r.planType === 'PRODUCTION_PLAN');
      const clampRow = rows.find(r => r.planType === 'CLAMP' || r.planType === 'CLAMP_COUNT');
      
      console.log('Found rows - PO:', !!poRow, 'Plan:', !!planRow, 'Clamp:', !!clampRow);
      
      if (poRow || planRow || clampRow) {
        const poData = this.convertCellsToDailyData(poRow?.cells || []);
        const planData = this.convertCellsToDailyData(planRow?.cells || []);
        const clampData = this.convertCellsToDailyData(clampRow?.cells || []);
        
        // Helper function to ensure value is a valid number
        const ensureNumber = (value: any): number => {
          if (value === null || value === undefined || value === '') {
            return 0;
          }
          const num = Number(value);
          return isNaN(num) ? 0 : num;
        };
        
        const detail: ProductionDetail = {
          rowGroupId: `group_${groupIndex++}`,
          materialCode: (poRow || planRow || clampRow)!.productCode,
          partName: (poRow || planRow || clampRow)!.componentName,
          plan: '',
          // Use totalValue from backend, fallback to sum if not available
          poTotal: ensureNumber(poRow?.totalValue),
          planTotal: ensureNumber(planRow?.totalValue),
          clampTotal: ensureNumber(clampRow?.totalValue),
          customer: (poRow || planRow || clampRow)!.customerName || '',
          customerId: (poRow || planRow || clampRow)!.customerId,
          poRequirement: poData,
          productionPlan: planData,
          clampCount: clampData,
          poRowId: poRow?.id,
          planRowId: planRow?.id,
          clampRowId: clampRow?.id
        };
        
        console.log('Created detail:', detail.materialCode, detail.partName, 
          'Totals:', detail.poTotal, detail.planTotal, detail.clampTotal);
        this.productionDetails.push(detail);
      }
    });
    
    console.log('Total productionDetails created:', this.productionDetails.length);
    
    // Sort by customer name to group them together
    this.productionDetails.sort((a, b) => {
      const customerCompare = (a.customer || '').localeCompare(b.customer || '');
      if (customerCompare !== 0) return customerCompare;
      // If same customer, sort by material code
      return a.materialCode.localeCompare(b.materialCode);
    });
    
    console.log('After sorting by customer');
    
    this.initializeSummaryData();
  }
  
  /**
   * Convert PMC cells to DailyData format
   */
  convertCellsToDailyData(cells: any[]): DailyData[] {
    console.log('Converting cells to daily data, cells count:', cells?.length || 0);
    if (cells && cells.length > 0) {
      console.log('Sample cell:', cells[0]);
    }
    
    return this.dateColumns.map(col => {
      const cell = cells.find(c => {
        const cellDate = format(new Date(c.workDate), 'yyyy-MM-dd');
        const colDate = format(col.date, 'yyyy-MM-dd');
        return cellDate === colDate;
      });
      
      // Ensure value is always a valid number (0 if null/undefined/empty)
      let cellValue = 0;
      if (cell?.value !== undefined && cell?.value !== null && cell?.value !== '') {
        cellValue = Number(cell.value);
        // If conversion fails, default to 0
        if (isNaN(cellValue)) {
          cellValue = 0;
        }
      }
      
      if (cell) {
        console.log('Cell found for date', format(col.date, 'yyyy-MM-dd'), 'value:', cell.value, '-> converted:', cellValue);
      }
      
      return {
        date: col.date,
        dateStr: col.dateStr,
        value: cellValue,
        cellId: cell?.id
      };
    });
  }
  
  /**
   * Load previous week
   */
  loadPreviousWeek(): void {
    if (!this.weekStartDate) return;
    
    this.loading = true;
    const prevWeekStart = format(addWeeks(this.weekStartDate, -1), 'yyyy-MM-dd');
    
    this.pmcService.getPMCWeek(undefined, prevWeekStart).subscribe({
      next: (data) => {
        this.currentWeek = data;
        this.initializeDateColumns(new Date(data.weekStartDate));
        this.loadPMCData(data);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading previous week:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tìm thấy PMC tuần trước'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Load next week
   */
  loadNextWeek(): void {
    if (!this.weekStartDate) return;
    
    this.loading = true;
    const nextWeekStart = format(addWeeks(this.weekStartDate, 1), 'yyyy-MM-dd');
    
    this.pmcService.getPMCWeek(undefined, nextWeekStart).subscribe({
      next: (data) => {
        this.currentWeek = data;
        this.initializeDateColumns(new Date(data.weekStartDate));
        this.loadPMCData(data);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading next week:', error);
        this.messageService.add({
          severity: 'info',
          summary: 'Thông báo',
          detail: 'Chưa có PMC cho tuần sau'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Create new PMC week
   */
  createNewWeek(copyFromPrevious: boolean = false): void {
    this.loading = true;
    const targetMonday = this.getCurrentOrNextMonday();
    
    console.log('Creating PMC for week starting:', format(targetMonday, 'yyyy-MM-dd'));
    
    this.pmcService.createPMCWeek({
      weekStartDate: format(targetMonday, 'yyyy-MM-dd'),
      copyFromPreviousWeek: copyFromPrevious
    }).subscribe({
      next: (data) => {
        console.log('PMC created successfully:', data);
        console.log('Number of rows:', data.rows?.length || 0);
        
        this.currentWeek = data;
        this.initializeDateColumns(new Date(data.weekStartDate));
        this.loadPMCData(data);
        this.hasChanges = false;
        this.loading = false;
        
        console.log('After loadPMCData, productionDetails:', this.productionDetails.length);
        
        // Check if we have data
        if (this.productionDetails.length === 0) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Cảnh báo',
            detail: `Đã tạo PMC tuần ${data.weekName} nhưng chưa có dữ liệu. Backend trả về ${data.rows?.length || 0} rows. Kiểm tra console log.`,
            life: 8000
          });
        } else {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: `Đã tạo PMC tuần ${data.weekName} với ${this.productionDetails.length} sản phẩm`
          });
        }
      },
      error: (error) => {
        console.error('Error creating PMC week:', error);
        const errorMessage = error.error?.error || error.error?.message || error.message || 'Không thể tạo PMC tuần mới';
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Debug available POs
   */
  debugAvailablePOs(): void {
    this.loading = true;
    this.pmcService.getAvailablePOs().subscribe({
      next: (data: any) => {
        console.log('Available POs:', data);
        
        const totalPOs = data.totalPOs || 0;
        const eligiblePOs = data.eligiblePOsCount || 0;
        const phunInPOs = data.phunInPOsCount || 0;
        
        // Count products and parts from phunInPOs or eligiblePOs
        const posToCheck = data.phunInPOs?.length > 0 ? data.phunInPOs : data.eligiblePOs;
        let totalProducts = 0;
        let totalParts = 0;
        let posWithProducts = 0;
        let productsWithParts = 0;
        
        if (posToCheck) {
          posToCheck.forEach((po: any) => {
            if (po.products && po.products.length > 0) {
              posWithProducts++;
              totalProducts += po.products.length;
              po.products.forEach((product: any) => {
                if (product.parts && product.parts.length > 0) {
                  productsWithParts++;
                  totalParts += product.parts.length;
                }
              });
            }
          });
        }
        
        let message = `Tìm thấy:\n`;
        message += `• Tổng số PO: ${totalPOs}\n`;
        message += `• PO đủ điều kiện: ${eligiblePOs}\n`;
        message += `• PO PHUN/IN: ${phunInPOs}\n\n`;
        message += `Chi tiết PO sẽ dùng:\n`;
        message += `• PO có sản phẩm: ${posWithProducts}/${phunInPOs || eligiblePOs}\n`;
        message += `• Tổng sản phẩm: ${totalProducts}\n`;
        message += `• Sản phẩm có linh kiện: ${productsWithParts}\n`;
        message += `• Tổng linh kiện: ${totalParts}\n`;
        
        let severity: 'success' | 'warn' | 'info' = 'info';
        let summary = 'Thông tin PO';
        
        if (eligiblePOs === 0) {
          severity = 'warn';
          summary = 'Không có PO';
          message = 'Không có PO nào ở trạng thái DRAFT hoặc APPROVED. Hãy tạo hoặc phê duyệt PO trước.';
        } else if (totalProducts === 0) {
          severity = 'warn';
          summary = 'PO không có sản phẩm';
          message += '\n⚠️ Tất cả PO chưa có sản phẩm nào!';
        } else if (totalParts === 0) {
          severity = 'warn';
          summary = 'Sản phẩm chưa có linh kiện';
          message += '\n⚠️ Tất cả sản phẩm chưa có linh kiện (parts)!';
        } else {
          severity = 'success';
          summary = 'Đã sẵn sàng tạo PMC';
        }
        
        this.messageService.add({
          severity: severity,
          summary: summary,
          detail: message,
          life: severity === 'success' ? 5000 : 10000
        });
        
        this.loading = false;
      },
      error: (error) => {
        console.error('Error getting available POs:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể lấy thông tin PO'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Initialize summary statistics (Phần 1)
   */
  initializeSummaryData(): void {
    const daysCount = this.dateColumns.length;
    
    // Get unique customers from production details
    const uniqueCustomers = [...new Set(this.productionDetails.map(d => d.customer).filter(c => c))];
    console.log('Unique customers:', uniqueCustomers);
    
    // Calculate workers needed per day per company
    const customerWorkers: { [customer: string]: number[] } = {};
    
    uniqueCustomers.forEach(customer => {
      customerWorkers[customer] = new Array(daysCount).fill(0);
    });
    
    this.productionDetails.forEach(detail => {
      if (detail.customer) {
        detail.clampCount.forEach((day, index) => {
          customerWorkers[detail.customer][index] += day.value;
        });
      }
    });
    
    // Calculate total workers needed
    const totalNeeded = new Array(daysCount).fill(0);
    Object.values(customerWorkers).forEach(customerValues => {
      customerValues.forEach((val, index) => {
        totalNeeded[index] += val;
      });
    });
    
    // Workers available (initialize if empty or adjust length)
    if (this.workersAvailable.length !== daysCount) {
      this.workersAvailable = new Array(daysCount).fill(41);
    }
    
    // Calculate revenue
    const revenueByPlan = new Array(daysCount).fill(0);
    // DT THEO NGƯỜI = #CN Có * 600,000
    const revenueTarget = this.workersAvailable.map(workers => workers * 600000);
    
    this.productionDetails.forEach(detail => {
      detail.productionPlan.forEach((day, index) => {
        // DT THEO KHSX = (Kế hoạch SX * Kẹp) * 235
        const productionValue = day.value || 0;
        const clampValue = detail.clampCount[index]?.value || 0;
        revenueByPlan[index] += (productionValue * clampValue) * 235;
      });
    });
    
    // Calculate percentage
    const percentages = revenueByPlan.map((rev, i) => 
      revenueTarget[i] > 0 ? Math.round((rev / revenueTarget[i]) * 100) : 0
    );
    
    // Calculate workers shortage: CN Thiếu = CN Có - TỔNG CẦN CN
    const workersShortage = totalNeeded.map((needed, i) => 
      this.workersAvailable[i] - needed
    );
    
    // Build summary data dynamically
    this.summaryData = [];
    
    // Add customer rows
    uniqueCustomers.forEach(customer => {
      this.summaryData.push({
        label: `#CN Cần ${customer}`,
        values: customerWorkers[customer]
      });
    });
    
    // Add totals and other metrics
    this.summaryData.push(
      {
        label: 'TỔNG CẦN CN',
        values: totalNeeded,
        isHighlight: true
      },
      {
        label: '#CN Có',
        values: this.workersAvailable,
        isEditable: true
      },
      {
        label: 'DT THEO KHSX',
        values: revenueByPlan,
        isRevenue: true
      },
      {
        label: 'DT THEO NGƯỜI',
        values: revenueTarget,
        isRevenue: true
      },
      {
        label: 'VƯỢT',
        values: percentages,
        isPercentage: true,
        isHighlight: true
      },
      {
        label: '#CN Thiếu',
        values: workersShortage,
        isHighlight: true
      }
    );
  }
  
  /**
   * Save production planning data
   */
  saveData(): void {
    if (!this.currentWeek) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Không có dữ liệu PMC để lưu'
      });
      return;
    }
    
    this.loading = true;
    
    // DO NOT recalculate totals before saving - users enter them manually
    // this.recalculateRowTotals();
    
    const rows: SavePMCRowRequest[] = [];
    
    console.log('Production details before save:', this.productionDetails);
    console.log('Number of production details:', this.productionDetails.length);
    
    this.productionDetails.forEach((detail, index) => {
      console.log(`\n=== Processing detail ${index + 1} ===`);
      console.log('Material:', detail.materialCode, 'Part:', detail.partName);
      console.log('Totals - PO:', detail.poTotal, 'Plan:', detail.planTotal, 'Clamp:', detail.clampTotal);
      
      // Helper function to ensure value is a valid number
      const ensureNumber = (value: any): number => {
        if (value === null || value === undefined || value === '') {
          return 0;
        }
        const num = Number(value);
        return isNaN(num) ? 0 : num;
      };
      
      // PO Requirement row
      const cellValues: { [key: string]: number } = {};
      detail.poRequirement.forEach(day => {
        const dateKey = format(day.date, 'yyyy-MM-dd');
        cellValues[dateKey] = ensureNumber(day.value);
      });
      console.log('PO cellValues:', cellValues);
      console.log('PO cellValues keys:', Object.keys(cellValues));
      console.log('PO cellValues values:', Object.values(cellValues));
      
      // Check if all values are 0
      const allZero = Object.values(cellValues).every(v => v === 0);
      if (allZero) {
        console.warn('⚠️ All PO cell values are 0 for:', detail.materialCode, detail.partName);
      }
      
      rows.push({
        id: detail.poRowId,
        productCode: detail.materialCode,
        componentName: detail.partName,
        customerId: detail.customerId,
        planType: 'REQUIREMENT',
        totalValue: ensureNumber(detail.poTotal),
        cellValues: cellValues
      });
      
      // Production Plan row
      const planCellValues: { [key: string]: number } = {};
      detail.productionPlan.forEach(day => {
        const dateKey = format(day.date, 'yyyy-MM-dd');
        planCellValues[dateKey] = ensureNumber(day.value);
      });
      console.log('Production Plan cellValues:', planCellValues);
      
      rows.push({
        id: detail.planRowId,
        productCode: detail.materialCode,
        componentName: detail.partName,
        customerId: detail.customerId,
        planType: 'PRODUCTION',
        totalValue: ensureNumber(detail.planTotal),
        cellValues: planCellValues
      });
      
      // Clamp Count row
      const clampCellValues: { [key: string]: number } = {};
      detail.clampCount.forEach(day => {
        const dateKey = format(day.date, 'yyyy-MM-dd');
        clampCellValues[dateKey] = ensureNumber(day.value);
      });
      console.log('Clamp cellValues:', clampCellValues);
      
      rows.push({
        id: detail.clampRowId,
        productCode: detail.materialCode,
        componentName: detail.partName,
        customerId: detail.customerId,
        planType: 'CLAMP',
        totalValue: ensureNumber(detail.clampTotal),
        cellValues: clampCellValues
      });
    });
    
    const request: SavePMCWeekRequest = {
      pmcWeekId: this.currentWeek.id,
      rows: rows
    };
    
    console.log('\n=== Final Save Request ===');
    console.log('PMC Week ID:', request.pmcWeekId);
    console.log('Total rows to save:', rows.length);
    console.log('Full request:', JSON.stringify(request, null, 2));
    
    this.pmcService.savePMCWeek(request).subscribe({
      next: (data) => {
        console.log('PMC saved successfully:', data);
        this.currentWeek = data;
        this.loadPMCData(data); // Reload UI with saved data
        this.hasChanges = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã lưu kế hoạch sản xuất'
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error saving PMC:', error);
        console.error('Error details:', error.error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || error.message || 'Không thể lưu kế hoạch'
        });
        this.loading = false;
      }
    });
  }
  
  /**
   * Handle change in workers available value
   */
  onWorkersAvailableChange(): void {
    // Recalculate summary data when workers available changes
    this.initializeSummaryData();
    this.hasChanges = true;
  }

  /**
   * Get cell background color based on value
   */
  getCellColor(value: number, isPercentage: boolean = false): string {
    if (isPercentage) {
      if (value === 0) return '';
      if (value >= 90) return '#ff0000'; // Red
      if (value >= 85) return '#ffff00'; // Yellow
      return '#92d050'; // Green
    }
    return '';
  }
  
  /**
   * Calculate totals
   */
  recalculateTotals(): void {
    // Get unique customers
    const uniqueCustomers = [...new Set(this.productionDetails.map(d => d.customer).filter(c => c))];
    
    // Recalculate for each customer
    const customerWorkers: { [customer: string]: number[] } = {};
    uniqueCustomers.forEach(customer => {
      customerWorkers[customer] = this.dateColumns.map((_, index) => 
        this.productionDetails
          .filter(d => d.customer === customer)
          .reduce((sum, d) => sum + (d.clampCount[index]?.value || 0), 0)
      );
    });
    
    // Update summary data for each customer
    uniqueCustomers.forEach((customer, idx) => {
      if (this.summaryData[idx]) {
        this.summaryData[idx].values = customerWorkers[customer];
      }
    });
    
    // Update total (should be right after customer rows)
    const totalIdx = uniqueCustomers.length;
    if (this.summaryData[totalIdx]?.label === 'TỔNG CẦN CN') {
      this.summaryData[totalIdx].values = this.dateColumns.map((_, index) => 
        Object.values(customerWorkers).reduce((sum, values) => sum + values[index], 0)
      );
    }
  }
  
  /**
   * Handle cell value change
   */
  onCellValueChange(): void {
    this.hasChanges = true;
    console.log('Cell value changed, hasChanges:', this.hasChanges);
    console.log('Current production details sample:', this.productionDetails[0]);
    
    // DO NOT recalculate totals - let users enter them manually
    // this.recalculateRowTotals();
    
    // Recalculate all summary data including revenue
    this.initializeSummaryData();
  }
  
  /**
   * Recalculate totals for each production detail row (NOT USED - totals are entered manually)
   */
  recalculateRowTotals(): void {
    // DO NOTHING - totals should be entered manually by users
    // Users can enter any value they want in the Total column
    console.log('recalculateRowTotals called but doing nothing - totals are manual entry');
  }
}
