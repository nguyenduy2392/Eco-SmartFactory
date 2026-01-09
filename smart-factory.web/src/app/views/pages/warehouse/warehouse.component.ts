import { Component, OnInit } from '@angular/core';
import { WarehouseService } from '../../../services/warehouse.service';
import { MaterialService } from '../../../services/material.service';
import { CustomerService } from '../../../services/customer.service';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';
import { StockInComponent } from '../../../components/stock-in/stock-in/stock-in.component';
import {
  Warehouse,
  MaterialReceipt,
  MaterialIssue,
  MaterialAdjustment,
  MaterialTransactionHistory,
  MaterialStock,
  CreateMaterialReceiptRequest,
  CreateMaterialIssueRequest,
  CreateMaterialAdjustmentRequest
} from '../../../models/warehouse.interface';
import { Material, CreateMaterialRequest } from '../../../models/material.interface';
import { Customer } from '../../../models/customer.interface';

@Component({
  selector: 'app-warehouse',
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule, StockInComponent]
})
export class WarehouseComponent implements OnInit {
  // Tab management
  activeTabIndex = 0;

  // Common data
  warehouses: Warehouse[] = [];
  materials: Material[] = [];
  customers: Customer[] = [];
  selectedCustomerId: string | null = null;

  // Receipt tab
  receipts: MaterialReceipt[] = [];

  // Issue tab
  issues: MaterialIssue[] = [];
  showIssueDialog = false;
  issueForm: Partial<CreateMaterialIssueRequest> = {
    issueDate: new Date(),
    quantity: 0
  };

  // Adjustment tab
  adjustments: MaterialAdjustment[] = [];
  showAdjustmentDialog = false;
  adjustmentForm: Partial<CreateMaterialAdjustmentRequest> = {
    adjustmentDate: new Date(),
    adjustmentQuantity: 0
  };

  // History tab
  history: MaterialTransactionHistory[] = [];
  historyFilters = {
    materialId: null as string | null,
    customerId: null as string | null,
    warehouseId: null as string | null,
    batchNumber: null as string | null,
    transactionType: null as string | null,
    fromDate: null as Date | null,
    toDate: null as Date | null
  };

  // Stock view
  selectedMaterialForStock: Material | null = null;
  materialStock: MaterialStock | null = null;
  showStockDialog = false;

  // Create material dialog
  showCreateMaterialDialog = false;
  currentMaterialDialogSource: 'receipt' | 'issue' | 'adjustment' | null = null;
  newMaterialForm: Partial<CreateMaterialRequest> = {
    code: '',
    name: '',
    type: '',
    unit: 'kg',
    customerId: ''
  };

  loading = false;

  transactionTypes = [
    { label: 'Tất cả', value: null },
    { label: 'Nhập kho', value: 'RECEIPT' },
    { label: 'Xuất kho', value: 'ISSUE' },
    { label: 'Điều chỉnh', value: 'ADJUSTMENT' }
  ];

  // Danh sách đơn vị tính
  units = [
    { label: 'kg', value: 'kg' },
    { label: 'lít', value: 'l' },
    { label: 'cái', value: 'pcs' },
    { label: 'bộ', value: 'set' },
    { label: 'm', value: 'm' },
    { label: 'm²', value: 'm2' },
    { label: 'm³', value: 'm3' },
    { label: 'g', value: 'g' },
    { label: 'tấn', value: 'ton' }
  ];

  // Danh sách loại vật tư
  materialTypes = [
    { label: 'Nhựa nguyên sinh', value: 'PLASTIC' },
    { label: 'Mực in', value: 'INK' },
    { label: 'Vật tư phụ', value: 'AUXILIARY' },
    { label: 'Khác', value: 'OTHER' }
  ];

  constructor(
    private warehouseService: WarehouseService,
    private materialService: MaterialService,
    private customerService: CustomerService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadCustomers();
    this.loadMaterials();
  }

  loadWarehouses(): void {
    this.warehouseService.getAllWarehouses(true).subscribe({
      next: (warehouses) => {
        this.warehouses = warehouses;
        // Set default warehouse if available
        if (warehouses.length > 0) {
          if (!this.issueForm.warehouseId) {
            this.issueForm.warehouseId = warehouses[0].id;
          }
          if (!this.adjustmentForm.warehouseId) {
            this.adjustmentForm.warehouseId = warehouses[0].id;
          }
        }
      },
      error: (error) => {
        console.error('Error loading warehouses:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách kho'
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
      }
    });
  }

  loadMaterials(): void {
    this.materialService.getAll(true, this.selectedCustomerId || undefined).subscribe({
      next: (materials) => {
        this.materials = materials;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading materials:', error);
        this.loading = false;
      }
    });
  }

  onCustomerFilterChange(): void {
    this.loadMaterials();
    this.historyFilters.customerId = this.selectedCustomerId;
    if (this.activeTabIndex === 3) {
      this.loadHistory();
    }
  }

  // Event handler khi nhập kho thành công từ StockInComponent
  onStockInSuccess(): void {
    this.loadMaterials(); // Refresh danh sách NVL và tồn kho
    this.loadReceipts(); // Refresh danh sách phiếu nhập (nếu cần hiển thị)
    this.messageService.add({
      severity: 'info',
      summary: 'Cập nhật',
      detail: 'Dữ liệu kho đã được cập nhật'
    });
  }

  // Issue methods
  openIssueDialog(): void {
    this.issueForm = {
      customerId: this.selectedCustomerId || '',
      warehouseId: this.warehouses[0]?.id || '',
      quantity: 0,
      unit: 'kg',
      batchNumber: '',
      issueDate: new Date(),
      reason: '',
      issueNumber: `PXK-${new Date().getFullYear()}-${String(Date.now()).slice(-6)}`,
      notes: ''
    };
    this.showIssueDialog = true;
  }

  onIssueMaterialChange(): void {
    const material = this.materials.find(m => m.id === this.issueForm.materialId);
    if (material) {
      this.issueForm.unit = material.unit;
      this.issueForm.customerId = material.customerId;
    }
  }

  saveIssue(): void {
    if (!this.issueForm.customerId || !this.issueForm.materialId || 
        !this.issueForm.warehouseId || !this.issueForm.batchNumber ||
        !this.issueForm.quantity || !this.issueForm.reason || !this.issueForm.issueNumber) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập đầy đủ thông tin bắt buộc'
      });
      return;
    }

    this.loading = true;
    this.warehouseService.createMaterialIssue(this.issueForm as CreateMaterialIssueRequest).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Xuất kho thành công'
        });
        this.showIssueDialog = false;
        this.loadMaterials(); // Reload to update stock - will set loading = false
        // Reload issues list if on issue tab
        if (this.activeTabIndex === 1) {
          this.loadIssues();
        }
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.error || 'Không thể xuất kho'
        });
      }
    });
  }

  // Adjustment methods
  openAdjustmentDialog(): void {
    this.adjustmentForm = {
      customerId: this.selectedCustomerId || '',
      warehouseId: this.warehouses[0]?.id || '',
      adjustmentQuantity: 0,
      unit: 'kg',
      batchNumber: '',
      adjustmentDate: new Date(),
      reason: '',
      responsiblePerson: '',
      adjustmentNumber: `DC-${new Date().getFullYear()}-${String(Date.now()).slice(-6)}`,
      notes: ''
    };
    this.showAdjustmentDialog = true;
  }

  onAdjustmentMaterialChange(): void {
    const material = this.materials.find(m => m.id === this.adjustmentForm.materialId);
    if (material) {
      this.adjustmentForm.unit = material.unit;
      this.adjustmentForm.customerId = material.customerId;
    }
  }

  saveAdjustment(): void {
    if (!this.adjustmentForm.customerId || !this.adjustmentForm.materialId || 
        !this.adjustmentForm.warehouseId || !this.adjustmentForm.batchNumber ||
        !this.adjustmentForm.reason || !this.adjustmentForm.responsiblePerson ||
        !this.adjustmentForm.adjustmentNumber) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập đầy đủ thông tin bắt buộc (bao gồm lý do và người chịu trách nhiệm)'
      });
      return;
    }

    if (this.adjustmentForm.adjustmentQuantity === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Số lượng điều chỉnh không thể bằng 0'
      });
      return;
    }

    this.loading = true;
    this.warehouseService.createMaterialAdjustment(this.adjustmentForm as CreateMaterialAdjustmentRequest).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Điều chỉnh kho thành công'
        });
        this.showAdjustmentDialog = false;
        this.loadMaterials(); // Reload to update stock - will set loading = false
        // Reload adjustments list if on adjustment tab
        if (this.activeTabIndex === 2) {
          this.loadAdjustments();
        }
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.error || 'Không thể điều chỉnh kho'
        });
      }
    });
  }

  // History methods
  loadHistory(): void {
    this.loading = true;
    this.warehouseService.getTransactionHistory({
      materialId: this.historyFilters.materialId || undefined,
      customerId: this.historyFilters.customerId || undefined,
      warehouseId: this.historyFilters.warehouseId || undefined,
      batchNumber: this.historyFilters.batchNumber || undefined,
      transactionType: this.historyFilters.transactionType || undefined,
      fromDate: this.historyFilters.fromDate || undefined,
      toDate: this.historyFilters.toDate || undefined
    }).subscribe({
      next: (history) => {
        this.history = history;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading history:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải lịch sử giao dịch'
        });
      }
    });
  }

  onHistoryTabChange(): void {
    if (this.activeTabIndex === 3) {
      this.historyFilters.customerId = this.selectedCustomerId;
      this.loadHistory();
    }
  }

  // Load methods for receipts, issues, adjustments (stub functions - can be implemented if needed)
  loadReceipts(): void {
    // TODO: Implement if needed to reload receipts list
    // For now, receipts are loaded on demand or can be refreshed manually
  }

  loadIssues(): void {
    // TODO: Implement if needed to reload issues list
    // For now, issues are loaded on demand or can be refreshed manually
  }

  loadAdjustments(): void {
    // TODO: Implement if needed to reload adjustments list
    // For now, adjustments are loaded on demand or can be refreshed manually
  }

  // Stock methods
  viewMaterialStock(material: Material): void {
    this.selectedMaterialForStock = material;
    this.loading = true;
    this.warehouseService.getMaterialStock(material.id).subscribe({
      next: (stock) => {
        this.materialStock = stock;
        this.showStockDialog = true;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading stock:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải thông tin tồn kho'
        });
      }
    });
  }

  getTransactionTypeLabel(type: string): string {
    switch (type) {
      case 'RECEIPT': return 'Nhập kho';
      case 'ISSUE': return 'Xuất kho';
      case 'ADJUSTMENT': return 'Điều chỉnh';
      default: return type;
    }
  }

  getTransactionTypeSeverity(type: string): string {
    switch (type) {
      case 'RECEIPT': return 'success';
      case 'ISSUE': return 'warning';
      case 'ADJUSTMENT': return 'info';
      default: return '';
    }
  }

  getStockSeverity(current: number, min: number): string {
    if (current <= 0) return 'danger';
    if (current <= min) return 'warning';
    return 'success';
  }

  // Excel Import/Export methods
  onFileSelected(event: any): void {
    const file = event.files[0];
    if (!file) return;

    if (!this.selectedCustomerId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn chủ hàng trước khi import'
      });
      return;
    }

    this.loading = true;
    this.warehouseService.importMaterialReceipts(file, this.selectedCustomerId).subscribe({
      next: (result) => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: result.message || `Đã import thành công ${result.receipts?.length || 0} phiếu nhập kho`
        });
        this.loadMaterials(); // Reload to update stock
        if (result.errors && result.errors.length > 0) {
          console.warn('Import errors:', result.errors);
        }
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.error || 'Không thể import file Excel'
        });
      }
    });
  }

  exportHistory(): void {
    this.loading = true;
    this.warehouseService.exportTransactionHistory({
      materialId: this.historyFilters.materialId || undefined,
      customerId: this.historyFilters.customerId || undefined,
      warehouseId: this.historyFilters.warehouseId || undefined,
      batchNumber: this.historyFilters.batchNumber || undefined,
      transactionType: this.historyFilters.transactionType || undefined,
      fromDate: this.historyFilters.fromDate || undefined,
      toDate: this.historyFilters.toDate || undefined
    }).subscribe({
      next: (blob) => {
        this.loading = false;
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Lich_su_kho_${new Date().toISOString().slice(0, 10)}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã xuất file Excel thành công'
        });
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.error || 'Không thể xuất file Excel'
        });
      }
    });
  }

  // Material creation methods
  openCreateMaterialDialog(source: 'receipt' | 'issue' | 'adjustment'): void {
    this.currentMaterialDialogSource = source;
    
    // Lấy customerId từ form hiện tại
    let customerId = '';
    if (source === 'issue' && this.issueForm.customerId) {
      customerId = this.issueForm.customerId;
    } else if (source === 'adjustment' && this.adjustmentForm.customerId) {
      customerId = this.adjustmentForm.customerId;
    }

    // Khởi tạo form tạo mới
    this.newMaterialForm = {
      code: '',
      name: '',
      type: '',
      unit: 'kg',
      customerId: customerId
    };

    this.showCreateMaterialDialog = true;
  }

  closeCreateMaterialDialog(): void {
    this.showCreateMaterialDialog = false;
    this.currentMaterialDialogSource = null;
    this.newMaterialForm = {
      code: '',
      name: '',
      type: '',
      unit: 'kg',
      customerId: ''
    };
  }

  saveNewMaterial(): void {
    // Validate form
    if (!this.newMaterialForm.code || !this.newMaterialForm.name) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập đầy đủ thông tin bắt buộc (Mã vật tư, Tên vật tư)'
      });
      return;
    }

    // Kiểm tra customerId (nên luôn có vì nút chỉ hiện khi đã chọn chủ hàng)
    if (!this.newMaterialForm.customerId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Không tìm thấy thông tin chủ hàng. Vui lòng chọn chủ hàng trước khi tạo nguyên vật liệu.'
      });
      return;
    }

    // Tự động gán tên chủ hàng vào supplier (mặc định chủ hàng là nhà cung cấp)
    const selectedCustomer = this.customers.find(c => c.id === this.newMaterialForm.customerId);

    // Gán giá trị mặc định cho các trường bắt buộc
    const materialRequest: CreateMaterialRequest = {
      code: this.newMaterialForm.code!,
      name: this.newMaterialForm.name!,
      type: this.newMaterialForm.type || '',
      unit: this.newMaterialForm.unit || 'kg',
      currentStock: 0,
      minStock: 0,
      customerId: this.newMaterialForm.customerId!,
      supplier: selectedCustomer?.name // Mặc định supplier = tên chủ hàng
    };

    this.loading = true;
    this.materialService.create(materialRequest).subscribe({
      next: (newMaterial) => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Tạo nguyên vật liệu mới thành công'
        });
        
        // Reload danh sách materials
        this.loadMaterials();
        
        // Tự động chọn nguyên vật liệu vừa tạo vào form hiện tại
        if (this.currentMaterialDialogSource === 'issue') {
          this.issueForm.materialId = newMaterial.id;
          this.onIssueMaterialChange();
        } else if (this.currentMaterialDialogSource === 'adjustment') {
          this.adjustmentForm.materialId = newMaterial.id;
          this.onAdjustmentMaterialChange();
        }

        // Đóng dialog
        this.closeCreateMaterialDialog();
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || error.error?.error || 'Không thể tạo nguyên vật liệu'
        });
      }
    });
  }
}

