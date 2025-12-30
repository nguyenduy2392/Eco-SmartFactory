import { Component, OnInit } from '@angular/core';
import { WarehouseService } from '../../../services/warehouse.service';
import { MaterialService } from '../../../services/material.service';
import { CustomerService } from '../../../services/customer.service';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';
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
import { Material } from '../../../models/material.interface';
import { Customer } from '../../../models/customer.interface';

@Component({
  selector: 'app-warehouse',
  templateUrl: './warehouse.component.html',
  styleUrls: ['./warehouse.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
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
  showReceiptDialog = false;
  receiptForm: Partial<CreateMaterialReceiptRequest> = {
    receiptDate: new Date(),
    quantity: 0
  };

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
        if (warehouses.length > 0 && !this.receiptForm.warehouseId) {
          this.receiptForm.warehouseId = warehouses[0].id;
          this.issueForm.warehouseId = warehouses[0].id;
          this.adjustmentForm.warehouseId = warehouses[0].id;
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
      },
      error: (error) => {
        console.error('Error loading materials:', error);
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

  // Receipt methods
  openReceiptDialog(): void {
    this.receiptForm = {
      customerId: this.selectedCustomerId || '',
      warehouseId: this.warehouses[0]?.id || '',
      quantity: 0,
      unit: 'kg',
      batchNumber: '',
      receiptDate: new Date(),
      receiptNumber: `PNK-${new Date().getFullYear()}-${String(Date.now()).slice(-6)}`,
      notes: ''
    };
    this.showReceiptDialog = true;
  }

  onReceiptMaterialChange(): void {
    const material = this.materials.find(m => m.id === this.receiptForm.materialId);
    if (material) {
      this.receiptForm.unit = material.unit;
      this.receiptForm.customerId = material.customerId;
    }
  }

  saveReceipt(): void {
    if (!this.receiptForm.customerId || !this.receiptForm.materialId || 
        !this.receiptForm.warehouseId || !this.receiptForm.batchNumber ||
        !this.receiptForm.quantity || !this.receiptForm.receiptNumber) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập đầy đủ thông tin bắt buộc'
      });
      return;
    }

    this.loading = true;
    this.warehouseService.createMaterialReceipt(this.receiptForm as CreateMaterialReceiptRequest).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Nhập kho thành công'
        });
        this.showReceiptDialog = false;
        this.loadMaterials(); // Reload to update stock
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.error || 'Không thể nhập kho'
        });
      }
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
        this.loadMaterials(); // Reload to update stock
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
        this.loadMaterials(); // Reload to update stock
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
}

