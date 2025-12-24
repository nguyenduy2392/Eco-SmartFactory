import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PurchaseOrderService } from '../../../services/purchase-order.service';
import { CustomerService } from '../../../services/customer.service';
import { PurchaseOrderList } from '../../../models/purchase-order.interface';
import { Customer } from '../../../models/customer.interface';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

@Component({
  selector: 'app-po-list',
  templateUrl: './po-list.component.html',
  styleUrls: ['./po-list.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class POListComponent implements OnInit {
  purchaseOrders: PurchaseOrderList[] = [];
  customers: Customer[] = [];
  loading = false;

  // Filters
  selectedStatus: string | undefined;
  selectedVersionType: string | undefined;
  selectedCustomerId: string | undefined;
  searchText = '';

  // Import Dialog
  showImportDialog = false;
  importLoading = false;
  selectedFile: File | null = null;
  importForm = {
    poNumber: '',
    customerId: '',
    templateType: 'EP_NHUA',
    poDate: new Date(),
    expectedDeliveryDate: null as Date | null,
    notes: ''
  };

  // Options
  statusOptions = [
    { label: 'Tất cả', value: undefined },
    { label: 'Mới', value: 'New' },
    { label: 'Đang xử lý', value: 'InProgress' },
    { label: 'Hoàn thành', value: 'Completed' },
    { label: 'Hủy', value: 'Cancelled' }
  ];

  versionTypeOptions = [
    { label: 'Tất cả', value: undefined },
    { label: 'Bản gốc', value: 'ORIGINAL' },
    { label: 'Bản chốt PM', value: 'FINAL' },
    { label: 'Bản sản xuất PMC', value: 'PRODUCTION' }
  ];

  templateTypeOptions = [
    { label: 'ÉP NHỰA', value: 'EP_NHUA' },
    { label: 'LẮP RÁP', value: 'LAP_RAP' },
    { label: 'PHUN IN', value: 'PHUN_IN' }
  ];

  constructor(
    private poService: PurchaseOrderService,
    private customerService: CustomerService,
    private router: Router,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadPurchaseOrders();
  }

  /**
   * Load danh sách chủ hàng
   */
  loadCustomers(): void {
    this.customerService.getAll(true).subscribe({
      next: (customers) => {
        this.customers = customers;
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách chủ hàng'
        });
      }
    });
  }

  /**
   * Load danh sách PO
   */
  loadPurchaseOrders(): void {
    this.loading = true;
    this.poService.getAll(this.selectedStatus, this.selectedVersionType, this.selectedCustomerId).subscribe({
      next: (pos) => {
        this.purchaseOrders = pos;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading purchase orders:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách PO'
        });
      }
    });
  }

  /**
   * Filter khi thay đổi điều kiện
   */
  onFilterChange(): void {
    this.loadPurchaseOrders();
  }

  /**
   * Xem chi tiết PO
   */
  viewPO(id: string): void {
    this.router.navigate(['/purchase-orders', id]);
  }

  /**
   * Tạo PO mới
   */
  createNewPO(): void {
    this.router.navigate(['/purchase-orders/new']);
  }

  /**
   * Clone version PO
   */
  cloneVersion(po: PurchaseOrderList): void {
    const newVersionType = po.versionType === 'ORIGINAL' ? 'FINAL' : 'PRODUCTION';
    
    this.poService.cloneVersion({
      originalPOId: po.id,
      newVersionType: newVersionType,
      notes: `Cloned from ${po.poNumber}`
    }).subscribe({
      next: (newPO) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: `Đã tạo bản ${newVersionType} từ ${po.poNumber}`
        });
        this.loadPurchaseOrders();
      },
      error: (error) => {
        console.error('Error cloning PO:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể clone PO'
        });
      }
    });
  }

  /**
   * Get badge severity cho status
   */
  getStatusSeverity(status: string): string {
    switch (status) {
      case 'New': return 'info';
      case 'InProgress': return 'warning';
      case 'Completed': return 'success';
      case 'Cancelled': return 'danger';
      default: return 'secondary';
    }
  }

  /**
   * Get badge severity cho version type
   */
  getVersionTypeSeverity(versionType: string): string {
    switch (versionType) {
      case 'ORIGINAL': return 'info';
      case 'FINAL': return 'warning';
      case 'PRODUCTION': return 'success';
      default: return 'secondary';
    }
  }

  /**
   * Get display text cho status
   */
  getStatusText(status: string): string {
    const option = this.statusOptions.find(opt => opt.value === status);
    return option ? option.label : status;
  }

  /**
   * Get display text cho version type
   */
  getVersionTypeText(versionType: string): string {
    const option = this.versionTypeOptions.find(opt => opt.value === versionType);
    return option ? option.label : versionType;
  }

  /**
   * Mở dialog import Excel
   */
  openImportDialog(): void {
    this.showImportDialog = true;
    this.resetImportForm();
  }

  /**
   * Đóng dialog import Excel
   */
  closeImportDialog(): void {
    this.showImportDialog = false;
    this.resetImportForm();
  }

  /**
   * Reset form import
   */
  resetImportForm(): void {
    this.selectedFile = null;
    this.importForm = {
      poNumber: '',
      customerId: '',
      templateType: 'EP_NHUA',
      poDate: new Date(),
      expectedDeliveryDate: null,
      notes: ''
    };
  }

  /**
   * Xử lý khi chọn file
   */
  onFileSelect(event: any): void {
    const file = event.files?.[0] || event.currentFiles?.[0];
    if (file) {
      // Kiểm tra định dạng file
      if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.xls')) {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Chỉ chấp nhận file Excel (.xlsx, .xls)'
        });
        return;
      }
      this.selectedFile = file;
    }
  }

  /**
   * Xóa file đã chọn
   */
  clearFile(): void {
    this.selectedFile = null;
  }

  /**
   * Download template Excel mẫu
   */
  downloadTemplate(templateType: string): void {
    const templates = {
      'EP_NHUA': this.createEpNhuaTemplate(),
      'LAP_RAP': this.createLapRapTemplate(),
      'PHUN_IN': this.createPhunInTemplate()
    };

    const template = templates[templateType as keyof typeof templates];
    if (template) {
      const blob = new Blob([template], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', `Template_${templateType}.csv`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }

  /**
   * Tạo template ÉP NHỰA
   */
  private createEpNhuaTemplate(): string {
    const header = 'STT,Mã linh kiện,Tên linh kiện,Vật liệu,Màu,Trọng lượng (g),Chu kỳ (s),Số lượng,Đơn giá,Thành tiền\n';
    const sample = '1,LK001,Linh kiện mẫu 1,ABS,Đen,50,30,1000,5000,5000000\n';
    return header + sample;
  }

  /**
   * Tạo template LẮP RÁP
   */
  private createLapRapTemplate(): string {
    const header = 'STT,Mã linh kiện,Tên linh kiện,Nội dung lắp ráp,Số lượng,Đơn giá,Thành tiền\n';
    const sample = '1,LK001,Linh kiện mẫu 1,Lắp ráp chi tiết A với B,1000,3000,3000000\n';
    return header + sample;
  }

  /**
   * Tạo template PHUN IN
   */
  private createPhunInTemplate(): string {
    const header = 'STT,Mã linh kiện,Tên linh kiện,Vị trí phun,Nội dung in,Màu sơn,Số lượng,Đơn giá,Thành tiền\n';
    const sample = '1,LK001,Linh kiện mẫu 1,Mặt trước,Logo công ty,Đỏ,1000,4000,4000000\n';
    return header + sample;
  }

  /**
   * Import PO từ Excel
   */
  importFromExcel(): void {
    // Validate
    if (!this.selectedFile) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Vui lòng chọn file Excel'
      });
      return;
    }

    if (!this.importForm.poNumber) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Vui lòng nhập mã PO'
      });
      return;
    }

    if (!this.importForm.customerId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Vui lòng chọn chủ hàng'
      });
      return;
    }

    // Call API
    this.importLoading = true;
    this.poService.importFromExcel(
      this.selectedFile,
      this.importForm.poNumber,
      this.importForm.customerId,
      this.importForm.templateType,
      this.importForm.poDate,
      this.importForm.expectedDeliveryDate,
      this.importForm.notes
    ).subscribe({
      next: (result) => {
        this.importLoading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: `Đã import PO ${result.poNumber} thành công`
        });
        this.closeImportDialog();
        this.loadPurchaseOrders();
      },
      error: (error) => {
        console.error('Error importing PO:', error);
        this.importLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.error || 'Không thể import PO từ Excel'
        });
      }
    });
  }
}

