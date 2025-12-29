import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { PurchaseOrderService } from '../../../services/purchase-order.service';
import { CustomerService } from '../../../services/customer.service';
import {
  PurchaseOrderList,
  ImportPOResponse,
  ImportError
} from '../../../models/purchase-order.interface';
import { Customer } from '../../../models/customer.interface';
import { MessageService, ConfirmationService } from 'primeng/api';
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
  selectedProcessingType: string | undefined;
  selectedCustomerId: string | undefined;
  searchText = '';

  // Import Dialog
  showImportDialog = false;
  importLoading = false;
  selectedFile: File | null = null;
  importErrors: ImportError[] = [];
  showImportErrorDialog = false;
  importForm = {
    poNumber: '',
    customerId: '',
    processingType: 'EP_NHUA',
    poDate: new Date(),
    expectedDeliveryDate: null as Date | null,
    notes: ''
  };

  // Options
  statusOptions = [
    { label: 'Tất cả', value: undefined },
    { label: 'Nháp', value: 'DRAFT' },
    { label: 'Đã phê duyệt cho PMC', value: 'APPROVED_FOR_PMC' },
    { label: 'Đã khóa', value: 'LOCKED' }
  ];

  processingTypeOptions = [
    { label: 'Tất cả', value: undefined },
    { label: 'ÉP NHỰA', value: 'EP_NHUA' },
    { label: 'PHUN IN', value: 'PHUN_IN' },
    { label: 'LẮP RÁP', value: 'LAP_RAP' }
  ];

  importProcessingTypeOptions = [
    { label: 'ÉP NHỰA', value: 'EP_NHUA' },
    { label: 'PHUN IN', value: 'PHUN_IN' },
    { label: 'LẮP RÁP', value: 'LAP_RAP' }
  ];

  constructor(
    private poService: PurchaseOrderService,
    private customerService: CustomerService,
    private router: Router,
    private route: ActivatedRoute,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit(): void {
    // Đọc queryParams từ URL (nếu có customerId từ trang customers)
    this.route.queryParams.subscribe(params => {
      if (params['customerId']) {
        this.selectedCustomerId = params['customerId'];
      }
      this.loadCustomers();
      this.loadPurchaseOrders();
    });
  }

  loadCustomers(): void {
    this.customerService.getAll().subscribe({
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

  loadPurchaseOrders(): void {
    this.loading = true;
    this.poService.getAll(
      this.selectedStatus,
      this.selectedProcessingType,
      this.selectedCustomerId
    ).subscribe({
      next: (orders) => {
        this.purchaseOrders = orders;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading purchase orders:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách PO'
        });
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.loadPurchaseOrders();
  }

  resetFilters(): void {
    this.selectedStatus = undefined;
    this.selectedProcessingType = undefined;
    this.selectedCustomerId = undefined;
    this.searchText = '';
    this.loadPurchaseOrders();
  }

  get filteredPurchaseOrders(): PurchaseOrderList[] {
    if (!this.searchText) {
      return this.purchaseOrders;
    }
    const search = this.searchText.toLowerCase();
    return this.purchaseOrders.filter(po =>
      po.poNumber.toLowerCase().includes(search) ||
      po.customerName.toLowerCase().includes(search)
    );
  }

  // Import Dialog
  openImportDialog(): void {
    this.showImportDialog = true;
    this.resetImportForm();
  }

  closeImportDialog(): void {
    this.showImportDialog = false;
    this.resetImportForm();
  }

  resetImportForm(): void {
    this.importForm = {
      poNumber: '',
      customerId: '',
      processingType: 'EP_NHUA',
      poDate: new Date(),
      expectedDeliveryDate: null,
      notes: ''
    };
    this.selectedFile = null;
    this.importErrors = [];
  }

  onFileSelect(event: any): void {
    const file = event.files[0];
    if (file) {
      // Validate file type
      if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.xls')) {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Vui lòng chọn file Excel (.xlsx hoặc .xls)'
        });
        return;
      }
      this.selectedFile = file;
    }
  }

  onFileRemove(): void {
    this.selectedFile = null;
  }

  importPO(): void {
    if (!this.validateImportForm()) {
      return;
    }

    this.importLoading = true;
    this.poService.importFromExcel(
      this.selectedFile!,
      this.importForm.poNumber,
      this.importForm.customerId,
      this.importForm.processingType as 'EP_NHUA' | 'PHUN_IN' | 'LAP_RAP',
      this.importForm.poDate,
      this.importForm.expectedDeliveryDate,
      this.importForm.notes
    ).subscribe({
      next: (response: ImportPOResponse) => {
        this.importLoading = false;
        
        if (response.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: `Import PO thành công!\nPO ID: ${response.purchaseOrderId}\nVersion: ${response.version}\nStatus: ${response.status}`,
            life: 5000
          });
          this.closeImportDialog();
          this.loadPurchaseOrders();
        } else {
          // Có lỗi validation
          this.importErrors = response.errors || [];
          this.showImportErrorDialog = true;
        }
      },
      error: (error) => {
        console.error('Import error:', error);
        this.importLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể import PO'
        });
      }
    });
  }

  validateImportForm(): boolean {
    if (!this.selectedFile) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn file Excel'
      });
      return false;
    }

    if (!this.importForm.poNumber.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập số PO'
      });
      return false;
    }

    if (!this.importForm.customerId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn chủ hàng'
      });
      return false;
    }

    if (!this.importForm.processingType) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn loại gia công'
      });
      return false;
    }

    return true;
  }

  closeImportErrorDialog(): void {
    this.showImportErrorDialog = false;
    this.importErrors = [];
  }

  viewPODetail(po: PurchaseOrderList): void {
    this.router.navigate(['/processing-po', po.id]);
  }

  deletePO(po: PurchaseOrderList): void {
    if (po.status !== 'DRAFT') {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Chỉ có thể xóa PO ở trạng thái Nháp'
      });
      return;
    }

    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa PO ${po.poNumber}?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      accept: () => {
        this.poService.delete(po.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Xóa PO thành công'
            });
            this.loadPurchaseOrders();
          },
          error: (error) => {
            console.error('Delete error:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi',
              detail: error.error?.message || 'Không thể xóa PO'
            });
          }
        });
      }
    });
  }

  getStatusLabel(status: string): string {
    const statusMap: { [key: string]: string } = {
      'DRAFT': 'Nháp',
      'APPROVED_FOR_PMC': 'Đã phê duyệt',
      'LOCKED': 'Đã khóa'
    };
    return statusMap[status] || status;
  }

  getStatusSeverity(status: string): string {
    const severityMap: { [key: string]: string } = {
      'DRAFT': 'warning',
      'APPROVED_FOR_PMC': 'success',
      'LOCKED': 'info'
    };
    return severityMap[status] || 'info';
  }

  getProcessingTypeLabel(type: string): string {
    const typeMap: { [key: string]: string } = {
      'EP_NHUA': 'ÉP NHỰA',
      'PHUN_IN': 'PHUN IN',
      'LAP_RAP': 'LẮP RÁP'
    };
    return typeMap[type] || type;
  }
}


