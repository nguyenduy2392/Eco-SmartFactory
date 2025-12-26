import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PurchaseOrderService } from '../../../services/purchase-order.service';
import { CustomerService } from '../../../services/customer.service';
import { PurchaseOrderList } from '../../../models/purchase-order.interface';
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
    private messageService: MessageService,
    private confirmationService: ConfirmationService
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
      // Thêm BOM UTF-8 để Excel nhận diện đúng encoding
      const BOM = '\uFEFF';
      const blob = new Blob([BOM + template], { 
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;' 
      });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', `Template_${templateType}.xlsx`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      // Hiển thị thông báo thành công
      this.messageService.add({
        severity: 'success',
        summary: 'Thành công',
        detail: `Đã tải xuống template ${templateType}`
      });
    }
  }

  /**
   * Tạo template ÉP NHỰA với đầy đủ các cột và dữ liệu mẫu
   */
  private createEpNhuaTemplate(): string {
    const header = 'Mã sản phẩm,Tên sản phẩm,Mã khuôn / Model,Mã linh kiện,Tên linh kiện,Vật liệu,Mã màu,Màu sắc,Số lòng khuôn,Chu kỳ (s),Trọng lượng (g),Số lượng (PCS),Đơn giá (VND),Thành tiền (VND)\n';
    
    // Dữ liệu mẫu từ template thực tế (theo đúng giá trị trong ảnh)
    // Thành tiền = Số lượng × Đơn giá
    const samples = [
      'PKW4180,VM-TMTR-00002,JAZ-PKW4180-T01-01,H02-0PKW4180-001,Nửa đầu trái,ABS 15A1-H,3-25962,PMS 340C,2,33,3.15,35000,1080,37800000',
      'PKW4180,VM-TMTR-00002,JAZ-PKW4180-T01-01,H02-0PKW4180-002,Nửa đầu phải,ABS 15A1-H,3-25962,PMS 340C,2,33,3.1,35000,1080,37800000',
      'PKW4180,VM-TMTR-00002,JAZ-PKW4180-T02-01,H02-0PKW4180-006,Thân dưới sau,ABS 15A1-H,3-25963,PMS 7541C,2,33,3.85,35000,1080,37800000',
      'PKW4180,VM-TMTR-00002,JAZ-PKW4180-T03-01,H02-0PKW4180-009,Vảy đầu,PVC 95°,3-25964,PMS 7458C,4,35,3.65,35000,1155,40425000',
      'PKW4180,VM-TMTR-00002,JAZ-PKW4180-T03-01,H02-0PKW4180-010,Vảy ngực,PVC 95°,3-25965,PMS 198C,4,35,0.6,35000,1155,40425000'
    ];
    
    return header + samples.join('\n');
  }

  /**
   * Tạo template LẮP RÁP với đầy đủ các cột và dữ liệu mẫu
   */
  private createLapRapTemplate(): string {
    const header = 'Mã sản phẩm,Nội dung gia công,Số lần gia công,Đơn giá (VND),Thành tiền (VND),Số lượng hợp đồng (PCS),Tổng tiền (VND),Ngày hoàn thành,Ghi chú\n';
    
    // Dữ liệu mẫu từ template thực tế
    const samples = [
      'PKW4180-0002,Lắp ráp,,31,150,4650,35000,162750000,2025-01-15'
    ];
    
    // Thêm dòng tổng (có thể để trống)
    const totalRow = ',,,162750000,,,,"Dòng tổng – để trống nếu không dùng"\n';
    
    return header + samples.join('\n') + totalRow;
  }

  /**
   * Tạo template PHUN IN với đầy đủ các cột và dữ liệu mẫu
   */
  private createPhunInTemplate(): string {
    const header = 'Tên sản phẩm,Mã sản phẩm,Mã linh kiện,Mô tả linh kiện,Vị trí gia công,Công đoạn,Số lần gia công,Đơn giá (VND),Đơn giá chuẩn (VND),Số lượng,Đơn giá hợp đồng (PCS),Thành tiền (VND),Ngày hoàn thành,Ghi chú\n';
    
    // Dữ liệu mẫu từ template thực tế (5 dòng như trong ảnh)
    const samples = [
      'Gallade,PKW4180-0002,H02-OPKW4180-003,Thân trên trước,Thân trên trước,Sơn,1,217,217,35000,217,7595000,',
      'Gallade,PKW4180-0002,H02-OPKW4180-003,Thân trên trước,Thân trên trước,Ép khuôn,1,217,217,35000,217,7595000,',
      'Gallade,PKW4180-0002,H02-OPKW4180-003,Thân trên trước,Thân trên trước,Cắt biên,0,217,217,35000,217,7595000,',
      'Gallade,PKW4180-0002,H02-OPKW4180-003,Thân trên trước,Thân trên trước,In chuyển,0,122,122,35000,217,7595000,',
      'Gallade,PKW4180-0002,H02-OPKW4180-003,Thân trên trước,Thân trên trước,Thủ công,0,238,238,35000,217,7595000,'
    ];
    
    return header + samples.join('\n');
  }

  /**
   * Xác nhận xóa PO
   */
  confirmDelete(po: PurchaseOrderList): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa PO "${po.poNumber}"? Hành động này không thể hoàn tác.`,
      header: 'Xác nhận xóa PO',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      accept: () => {
        this.deletePO(po);
      }
    });
  }

  /**
   * Xóa PO sau khi xác nhận
   */
  deletePO(po: PurchaseOrderList): void {
    this.loading = true;
    this.poService.delete(po.id).subscribe({
      next: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: `Đã xóa PO ${po.poNumber} thành công`
        });
        this.loadPurchaseOrders();
      },
      error: (error) => {
        console.error('Error deleting PO:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.error || 'Không thể xóa PO'
        });
      }
    });
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

