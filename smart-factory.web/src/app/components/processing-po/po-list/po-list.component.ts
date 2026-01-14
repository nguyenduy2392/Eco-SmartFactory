import { Component, OnInit, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { PurchaseOrderService } from '../../../services/purchase-order.service';
import { CustomerService } from '../../../services/customer.service';
import { ProcessingTypeService } from '../../../services/processing-type.service';
import { PurchaseOrderList } from '../../../models/purchase-order.interface';
import { Customer } from '../../../models/customer.interface';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';
import { Menu } from 'primeng/menu';
import { UiModalService } from '../../../services/shared/ui-modal.service';
import { POImportDialogComponent } from './po-import-dialog/po-import-dialog.component';

@Component({
  selector: 'app-po-list',
  templateUrl: './po-list.component.html',
  styleUrls: ['./po-list.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class POListComponent implements OnInit, OnChanges {
  @ViewChild('actionMenu') actionMenu!: Menu;
  @Input() customerId?: string;
  @Input() hideHeader = false;

  purchaseOrders: PurchaseOrderList[] = [];
  customers: Customer[] = [];
  loading = false;
  paginatedPurchaseOrders: PurchaseOrderList[] = [];
  currentPage = 1;
  pageSize = 10;
  actionMenuItems: any[] = [];
  currentActionPO: PurchaseOrderList | null = null;

  // Filters
  selectedStatus: string | undefined;
  selectedProcessingType: string | undefined;
  selectedCustomerId: string | undefined;
  searchText = '';

  // Import Dialog - managed via UiModalService

  // Options
  statusOptions = [
    { label: 'Tất cả', value: undefined },
    { label: 'Nháp', value: 'DRAFT' },
    { label: 'Đã phê duyệt cho PMC', value: 'APPROVED_FOR_PMC' },
    { label: 'Đã khóa', value: 'LOCKED' }
  ];

  processingTypeOptions: { label: string; value: string | undefined }[] = [];
  importProcessingTypeOptions: { label: string; value: string }[] = [];

  constructor(
    private poService: PurchaseOrderService,
    private customerService: CustomerService,
    private processingTypeService: ProcessingTypeService,
    private router: Router,
    private route: ActivatedRoute,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private uiModalService: UiModalService
  ) { }

  ngOnInit(): void {
    // Ưu tiên sử dụng @Input customerId, nếu không có thì đọc từ queryParams
    if (this.customerId) {
      this.selectedCustomerId = this.customerId;
    } else {
      // Đọc queryParams từ URL (nếu có customerId từ trang customers)
      this.route.queryParams.subscribe(params => {
        if (params['customerId']) {
          this.selectedCustomerId = params['customerId'];
        }
      });
    }
    
    this.loadProcessingTypes();
    this.loadCustomers();
    this.loadPurchaseOrders();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['customerId'] && !changes['customerId'].firstChange) {
      this.selectedCustomerId = this.customerId;
      this.loadPurchaseOrders();
    }
  }

  loadProcessingTypes(): void {
    this.processingTypeService.getAll().subscribe({
      next: (types) => {
        // For filter dropdown - include "All" option
        this.processingTypeOptions = [
          { label: 'Tất cả', value: undefined },
          ...types.map(type => ({
            label: type.name,
            value: type.code
          }))
        ];
        // For import form - no "All" option
        this.importProcessingTypeOptions = types.map(type => ({
          label: type.name,
          value: type.code
        }));
      },
      error: (error) => {
        console.error('Error loading processing types:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách loại gia công'
        });
      }
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
        this.currentPage = 1;
        this.updatePaginatedPurchaseOrders();
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
    this.currentPage = 1;
    this.updatePaginatedPurchaseOrders();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.updatePaginatedPurchaseOrders();
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

  /**
   * Update paginated purchase orders
   */
  updatePaginatedPurchaseOrders(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.paginatedPurchaseOrders = this.filteredPurchaseOrders.slice(start, end);
  }

  /**
   * Pagination helpers
   */
  getFirstRecord(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getLastRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredPurchaseOrders.length);
  }

  getTotalPages(): number {
    return Math.ceil(this.filteredPurchaseOrders.length / this.pageSize);
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const maxVisible = 5;
    
    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (this.currentPage <= 3) {
        for (let i = 1; i <= maxVisible; i++) {
          pages.push(i);
        }
      } else if (this.currentPage >= totalPages - 2) {
        for (let i = totalPages - maxVisible + 1; i <= totalPages; i++) {
          pages.push(i);
        }
      } else {
        for (let i = this.currentPage - 2; i <= this.currentPage + 2; i++) {
          pages.push(i);
        }
      }
    }
    
    return pages;
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePaginatedPurchaseOrders();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.getTotalPages()) {
      this.currentPage++;
      this.updatePaginatedPurchaseOrders();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.getTotalPages()) {
      this.currentPage = page;
      this.updatePaginatedPurchaseOrders();
    }
  }

  /**
   * Get row number for display (STT)
   */
  getRowNumber(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  /**
   * Show action menu
   */
  showActionMenu(event: Event, po: PurchaseOrderList): void {
    this.currentActionPO = po;
    this.actionMenuItems = this.getActionMenuItems(po);
  }

  /**
   * Get action menu items for a PO
   */
  getActionMenuItems(po: PurchaseOrderList): any[] {
    const items: any[] = [
      {
        label: 'Xem chi tiết',
        icon: 'pi pi-eye',
        command: () => this.viewPODetail(po)
      }
    ];

    if (po.status === 'DRAFT') {
      items.push({
        separator: true
      });
      items.push({
        label: 'Xóa',
        icon: 'pi pi-trash',
        styleClass: 'p-menuitem-danger',
        command: () => this.deletePO(po)
      });
    }

    return items;
  }

  // Import Dialog
  openImportDialog(): void {
    this.uiModalService.openModal({
      title: 'Nhập khẩu PO từ Excel',
      bodyComponent: POImportDialogComponent,
      bodyData: {
        customers: this.customers,
        importProcessingTypeOptions: this.importProcessingTypeOptions,
        customerId: this.customerId,
        onImport: () => this.loadPurchaseOrders()
      },
      size: '50vw',
      showFooter: false
    });
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
    const option = this.importProcessingTypeOptions.find(opt => opt.value === type);
    return option ? option.label : type;
  }
}


