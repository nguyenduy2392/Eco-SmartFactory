import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { PurchaseOrderService } from '../../../services/purchase-order.service';
import { ProductService } from '../../../services/system/product.service';
import { PartService } from '../../../services/part.service';
import { ProcessingTypeService } from '../../../services/processing-type.service';
import {
  PurchaseOrder,
  POOperation,
  POProduct
} from '../../../models/purchase-order.interface';
import { Product } from '../../../models/interface/product.interface';
import { PartDetail } from '../../../services/part.service';
import { ProcessingType } from '../../../models/processing-type.interface';
import { MaterialReceiptHistory } from '../../../models/stock-in.interface';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';
import { POExcelTableComponent } from '../po-excel-table/po-excel-table.component';

@Component({
  selector: 'app-po-detail',
  templateUrl: './po-detail.component.html',
  styleUrls: ['./po-detail.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule, POExcelTableComponent]
})
export class PODetailComponent implements OnInit {
  purchaseOrder: PurchaseOrder | null = null;
  loading = false;
  poId: string | null = null;
  isViewingOriginal = false; // Flag to track if viewing original PO (readonly)
  operationPOId: string | null = null; // Store operation PO ID when viewing original

  // Edit mode flags
  editingGeneralInfo = false;
  editingProducts: { [key: string]: { quantity: number; unitPrice: number } } = {};
  editingOperations: { [key: string]: any } = {};

  // General info editing
  editGeneralInfo: any = {
    poNumber: '',
    poDate: null,
    expectedDeliveryDate: null,
    customerId: '',
    processingType: '',
    notes: ''
  };

  // Product management
  showAddProductDialog = false;
  availableProducts: Product[] = [];
  newProduct: any = {
    productId: '',
    quantity: 0,
    unitPrice: 0
  };

  // Operation management
  showAddOperationDialog = false;
  availableParts: PartDetail[] = [];
  availableProcessingTypes: ProcessingType[] = [];
  newOperation: any = {
    partId: '',
    processingTypeId: '',
    operationStep: '',
    chargeCount: 1,
    unitPrice: 0,
    quantity: 0,
    completionDate: null,
    notes: ''
  };

  // Expanded products
  expandedProducts: Set<string> = new Set();

  // Excel mode - always enabled
  excelMode = true;

  // Material Receipt History
  materialReceiptHistory: MaterialReceiptHistory[] = [];
  loadingHistory = false;

  // Loading states
  savingGeneralInfo = false;
  savingProduct: { [key: string]: boolean } = {};
  savingOperation: { [key: string]: boolean } = {};
  deletingProduct: { [key: string]: boolean } = {};
  deletingOperation: { [key: string]: boolean } = {};
  exporting = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private poService: PurchaseOrderService,
    private productService: ProductService,
    private partService: PartService,
    private processingTypeService: ProcessingTypeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  /**
   * Navigate to warehouse page with pre-filled customer and PO
   */
  navigateToStockIn(): void {
    if (!this.purchaseOrder) return;
    
    this.router.navigate(['/warehouse'], {
      queryParams: {
        customerId: this.purchaseOrder.customerId,
        poId: this.purchaseOrder.id,
        poNumber: this.purchaseOrder.poNumber,
        tab: 'stock-in'
      }
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.poId = params['id'];
      if (this.poId) {
        this.loadPODetail();
        this.loadAvailableData();
        this.loadMaterialReceiptHistory();
      }
    });
  }

  loadPODetail(): void {
    if (!this.poId) return;

    this.loading = true;
    this.poService.getById(this.poId).subscribe({
      next: (po) => {
        this.purchaseOrder = po;
        this.loading = false;
        // Auto-expand first product if exists
        if (po.products && po.products.length > 0) {
          this.expandedProducts.add(po.products[0].productId);
        }
      },
      error: (error) => {
        console.error('Error loading PO:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải thông tin PO'
        });
        this.loading = false;
      }
    });
  }

  loadAvailableData(): void {
    // Load products
    this.productService.getAll().subscribe({
      next: (products) => {
        this.availableProducts = products;
      },
      error: (error) => {
        console.error('Error loading products:', error);
      }
    });

    // Load parts
    this.partService.getAll().subscribe({
      next: (parts) => {
        this.availableParts = parts;
      },
      error: (error) => {
        console.error('Error loading parts:', error);
      }
    });

    // Load processing types
    this.processingTypeService.getAll().subscribe({
      next: (types) => {
        this.availableProcessingTypes = types;
      },
      error: (error) => {
        console.error('Error loading processing types:', error);
      }
    });
  }

  loadMaterialReceiptHistory(): void {
    if (!this.poId) return;

    this.loadingHistory = true;
    this.poService.getReceiptHistory(this.poId).subscribe({
      next: (history) => {
        this.materialReceiptHistory = history;
        this.loadingHistory = false;
      },
      error: (error) => {
        console.error('Error loading receipt history:', error);
        this.loadingHistory = false;
      }
    });
  }

  getMaterialsWithProgress(): any[] {
    if (!this.purchaseOrder || !this.purchaseOrder.purchaseOrderMaterials) {
      return [];
    }

    return this.purchaseOrder.purchaseOrderMaterials.map(material => {
      // Tính tổng số lượng đã nhập cho material này
      const receivedQuantity = this.materialReceiptHistory
        .filter(h => h.materialCode === material.materialCode)
        .reduce((sum, h) => sum + h.quantity, 0);

      // Tính phần trăm hoàn thành
      const progressPercent = material.plannedQuantity > 0
        ? Math.min(Math.round((receivedQuantity / material.plannedQuantity) * 100), 100)
        : 0;

      return {
        ...material,
        receivedQuantity,
        progressPercent
      };
    });
  }

  updateMaterialStatus(completed: boolean): void {
    if (!this.poId) return;

    this.poService.updateMaterialStatus(this.poId, { isMaterialFullyReceived: completed }).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Cập nhật trạng thái nhập NVL thành công'
        });
        if (this.purchaseOrder) {
          this.purchaseOrder.isMaterialFullyReceived = completed;
        }
      },
      error: (error) => {
        console.error('Error updating material status:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể cập nhật trạng thái'
        });
      }
    });
  }

  // Navigation
  goBack(): void {
    this.location.back();
  }

  // Status helpers
  getStatusLabel(status: string): string {
    const statusMap: { [key: string]: string } = {
      'DRAFT': 'Nháp',
      'APPROVED_FOR_PMC': 'Vo',
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

  // General Info Management
  startEditGeneralInfo(): void {
    if (!this.purchaseOrder) return;
    this.editGeneralInfo = {
      poNumber: this.purchaseOrder.poNumber,
      poDate: new Date(this.purchaseOrder.poDate),
      expectedDeliveryDate: this.purchaseOrder.expectedDeliveryDate ? new Date(this.purchaseOrder.expectedDeliveryDate) : null,
      notes: this.purchaseOrder.notes || ''
    };
    this.editingGeneralInfo = true;
  }

  cancelEditGeneralInfo(): void {
    this.editingGeneralInfo = false;
    this.editGeneralInfo = {};
  }

  saveGeneralInfo(): void {
    if (!this.poId || !this.purchaseOrder) return;

    this.savingGeneralInfo = true;
    this.poService.updateGeneralInfo(this.poId, {
      poNumber: this.editGeneralInfo.poNumber,
      poDate: this.editGeneralInfo.poDate,
      expectedDeliveryDate: this.editGeneralInfo.expectedDeliveryDate,
      notes: this.editGeneralInfo.notes
    }).subscribe({
      next: (updated) => {
        this.purchaseOrder = updated;
        this.savingGeneralInfo = false;
        this.editingGeneralInfo = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã cập nhật thông tin PO'
        });
      },
      error: (error) => {
        console.error('Error updating general info:', error);
        this.savingGeneralInfo = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể cập nhật thông tin PO'
        });
      }
    });
  }

  // Product Management
  toggleProduct(productId: string): void {
    if (this.expandedProducts.has(productId)) {
      this.expandedProducts.delete(productId);
    } else {
      this.expandedProducts.add(productId);
    }
  }

  isProductExpanded(productId: string): boolean {
    return this.expandedProducts.has(productId);
  }

  startEditProduct(product: POProduct): void {
    this.editingProducts[product.id] = {
      quantity: product.quantity,
      unitPrice: product.unitPrice || 0
    };
  }

  cancelEditProduct(productId: string): void {
    delete this.editingProducts[productId];
  }

  saveProduct(product: POProduct): void {
    if (!this.poId) return;

    const edited = this.editingProducts[product.id];
    if (!edited) return;

    this.savingProduct[product.id] = true;
    this.poService.updateProduct(this.poId, product.id, {
      quantity: edited.quantity,
      unitPrice: edited.unitPrice || 0
    }).subscribe({
      next: () => {
        delete this.editingProducts[product.id];
        this.savingProduct[product.id] = false;
        this.loadPODetail();
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã cập nhật sản phẩm'
        });
      },
      error: (error) => {
        console.error('Error updating product:', error);
        this.savingProduct[product.id] = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể cập nhật sản phẩm'
        });
      }
    });
  }

  deleteProduct(product: POProduct): void {
    if (!this.poId) return;

    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa sản phẩm "${product.productCode} - ${product.productName}"?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      accept: () => {
        // TODO: Implement delete product API
        this.messageService.add({
          severity: 'warn',
          summary: 'Thông báo',
          detail: 'Chức năng xóa sản phẩm đang được phát triển'
        });
      }
    });
  }

  openAddProductDialog(): void {
    this.newProduct = {
      productId: '',
      quantity: 0,
      unitPrice: 0
    };
    this.showAddProductDialog = true;
  }

  closeAddProductDialog(): void {
    this.showAddProductDialog = false;
    this.newProduct = {
      productId: '',
      quantity: 0,
      unitPrice: 0
    };
  }

  saveNewProduct(): void {
    if (!this.poId || !this.newProduct.productId || !this.newProduct.quantity) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng điền đầy đủ thông tin'
      });
      return;
    }

    // TODO: Implement create product API
    this.messageService.add({
      severity: 'warn',
      summary: 'Thông báo',
      detail: 'Chức năng thêm sản phẩm đang được phát triển'
    });
    this.closeAddProductDialog();
  }

  // Operation Management
  getOperationsForProduct(productId: string): POOperation[] {
    if (!this.purchaseOrder?.operations) return [];
    return this.purchaseOrder.operations.filter(op => op.productId === productId);
  }

  getOperationsForPart(partId: string): POOperation[] {
    if (!this.purchaseOrder?.operations) return [];
    return this.purchaseOrder.operations.filter(op => op.partId === partId);
  }

  getPartsForProduct(productId: string): any[] {
    if (!this.purchaseOrder?.operations) return [];

    const partMap = new Map<string, any>();
    this.purchaseOrder.operations
      .filter(op => op.productId === productId)
      .forEach(op => {
        if (!partMap.has(op.partId)) {
          partMap.set(op.partId, {
            partId: op.partId,
            partCode: op.partCode,
            partName: op.partName,
            operations: []
          });
        }
        partMap.get(op.partId).operations.push(op);
      });

    return Array.from(partMap.values());
  }

  startEditOperation(operation: POOperation): void {
    if (!operation || !operation.id) return;
    const operationId = String(operation.id);
    const operationStep = this.getOperationStep(operation);
    this.editingOperations[operationId] = {
      operationStep: operationStep,
      chargeCount: operation.chargeCount || 1,
      unitPrice: operation.unitPrice || 0,
      quantity: operation.quantity || 0,
      completionDate: operation.completionDate ? new Date(operation.completionDate) : null,
      notes: operation.notes || ''
    };
  }

  cancelEditOperation(operationId: string): void {
    if (!operationId) return;
    const id = String(operationId);
    delete this.editingOperations[id];
  }

  saveOperation(operation: POOperation): void {
    if (!this.poId || !operation || !operation.id) return;

    const operationId = String(operation.id);
    const edited = this.editingOperations[operationId];
    if (!edited) return;

    this.savingOperation[operationId] = true;

    const processingTypeName = operation.processingTypeName || '';
    const operationName = edited.operationStep
      ? `${processingTypeName} - ${edited.operationStep}`
      : edited.operationStep;

    const updateData = {
      operationName: operationName,
      printContent: edited.operationStep || '',
      chargeCount: edited.chargeCount,
      unitPrice: edited.unitPrice,
      quantity: edited.quantity,
      completionDate: edited.completionDate,
      notes: edited.notes
    };

    this.poService.updateOperation(this.poId, operation.id, updateData).subscribe({
      next: () => {
        delete this.editingOperations[operationId];
        this.savingOperation[operationId] = false;
        this.loadPODetail();
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã cập nhật công đoạn'
        });
      },
      error: (error) => {
        console.error('Error updating operation:', error);
        this.savingOperation[operationId] = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể cập nhật công đoạn'
        });
      }
    });
  }

  deleteOperation(operation: POOperation): void {
    if (!this.poId || !operation || !operation.id) return;

    const operationId = String(operation.id);
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa công đoạn "${this.getOperationStep(operation)}"?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      accept: () => {
        this.deletingOperation[operationId] = true;
        this.poService.deleteOperation(this.poId!, operation.id).subscribe({
          next: () => {
            this.deletingOperation[operationId] = false;
            this.loadPODetail();
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa công đoạn'
            });
          },
          error: (error) => {
            console.error('Error deleting operation:', error);
            this.deletingOperation[operationId] = false;
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi',
              detail: error.error?.message || 'Không thể xóa công đoạn'
            });
          }
        });
      }
    });
  }

  openAddOperationDialog(productId?: string, partId?: string): void {
    this.newOperation = {
      partId: partId || '',
      processingTypeId: this.purchaseOrder?.processingType ?
        this.availableProcessingTypes.find(t => t.code === this.purchaseOrder?.processingType)?.id || '' : '',
      operationStep: '',
      chargeCount: 1,
      unitPrice: 0,
      quantity: 0,
      completionDate: null,
      notes: ''
    };
    this.showAddOperationDialog = true;
  }

  closeAddOperationDialog(): void {
    this.showAddOperationDialog = false;
    this.newOperation = {
      partId: '',
      processingTypeId: '',
      operationStep: '',
      chargeCount: 1,
      unitPrice: 0,
      quantity: 0,
      completionDate: null,
      notes: ''
    };
  }

  saveNewOperation(): void {
    if (!this.poId || !this.purchaseOrder) return;

    if (!this.newOperation.partId || !this.newOperation.processingTypeId || !this.newOperation.operationStep ||
        !this.newOperation.unitPrice || !this.newOperation.quantity) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng điền đầy đủ thông tin bắt buộc'
      });
      return;
    }

    const processingType = this.availableProcessingTypes.find(t => t.id === this.newOperation.processingTypeId);
    const processingTypeName = processingType?.name || this.purchaseOrder.processingType || '';
    const operationName = `${processingTypeName} - ${this.newOperation.operationStep}`;

    const part = this.availableParts.find(p => p.id === this.newOperation.partId);
    const productId = part?.productId;

    const operationData = {
      partId: this.newOperation.partId,
      processingTypeId: this.newOperation.processingTypeId,
      operationName: operationName,
      printContent: this.newOperation.operationStep,
      chargeCount: this.newOperation.chargeCount || 1,
      unitPrice: this.newOperation.unitPrice,
      quantity: this.newOperation.quantity,
      completionDate: this.newOperation.completionDate,
      notes: this.newOperation.notes || '',
      sequenceOrder: (this.purchaseOrder.operations?.length || 0) + 1
    };

    this.poService.createOperation(this.poId, operationData).subscribe({
      next: () => {
        this.closeAddOperationDialog();
        this.loadPODetail();
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã thêm công đoạn mới'
        });
      },
      error: (error) => {
        console.error('Error creating operation:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể thêm công đoạn mới'
        });
      }
    });
  }

  // Helpers
  getOperationStep(operation: POOperation): string {
    if (operation.printContent && operation.printContent.trim() !== '') {
      return operation.printContent.trim();
    }
    if (operation.operationName && operation.processingTypeName) {
      const prefix = `${operation.processingTypeName} - `;
      if (operation.operationName.startsWith(prefix)) {
        return operation.operationName.substring(prefix.length).trim();
      }
    }
    return operation.operationName || '-';
  }

  isEditingOperation(operationId: string): boolean {
    if (!operationId) return false;
    const id = String(operationId);
    return !!this.editingOperations[id];
  }

  // Helper method to convert ID to string in template
  toString(value: any): string {
    return String(value);
  }

  isEditingProduct(productId: string): boolean {
    return !!this.editingProducts[productId];
  }

  calculateTotal(operation: any): number {
    if (!operation) return 0;
    const chargeCount = operation.chargeCount || 1;
    const unitPrice = operation.unitPrice || 0;
    const quantity = operation.quantity || 0;
    return chargeCount * unitPrice * quantity;
  }

  getProductTotal(product: POProduct): number {
    if (!this.purchaseOrder?.operations) return product.totalAmount || 0;
    return this.purchaseOrder.operations
      .filter(op => op.productId === product.productId)
      .reduce((sum, op) => {
        const opId = String(op.id);
        if (this.editingOperations[opId]) {
          return sum + this.calculateTotal(this.editingOperations[opId]);
        }
        return sum + op.totalAmount;
      }, 0);
  }

  getPartTotal(part: any): number {
    return part.operations.reduce((sum: number, op: POOperation) => {
      const opId = String(op.id);
      if (this.editingOperations[opId]) {
        return sum + this.calculateTotal(this.editingOperations[opId]);
      }
      return sum + op.totalAmount;
    }, 0);
  }

  getTotalAmount(): number {
    if (!this.purchaseOrder?.operations) return 0;
    return this.purchaseOrder.operations.reduce((sum, op) => {
      const opId = String(op.id);
      if (this.editingOperations[opId]) {
        return sum + this.calculateTotal(this.editingOperations[opId]);
      }
      return sum + op.totalAmount;
    }, 0);
  }

  // Export Excel
  exportToExcel(): void {
    if (!this.poId) return;

    this.exporting = true;
    this.poService.exportOperations(this.poId).subscribe({
      next: (blob) => {
        this.exporting = false;
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `PO_${this.purchaseOrder?.poNumber}_${new Date().toISOString().slice(0, 10)}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã xuất file Excel thành công'
        });
      },
      error: (error) => {
        console.error('Error exporting Excel:', error);
        this.exporting = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể xuất file Excel'
        });
      }
    });
  }

  // Clone Version
  openCloneDialog(): void {
    // TODO: Implement clone version dialog
    this.messageService.add({
      severity: 'info',
      summary: 'Thông báo',
      detail: 'Chức năng tạo version mới đang được phát triển'
    });
  }

  // Excel mode toggle
  toggleExcelMode(): void {
    this.excelMode = !this.excelMode;
  }

  onExcelDataChanged(): void {
    this.loadPODetail();
  }

  onPOSaved(updatedPO: PurchaseOrder): void {
    this.purchaseOrder = updatedPO;
    this.loadPODetail();
  }

  // View Original PO (readonly)
  viewOriginalPO(): void {
    if (!this.purchaseOrder?.originalPOId) return;

    // Store current operation PO ID
    this.operationPOId = this.poId;

    this.isViewingOriginal = true;
    this.loading = true;
    this.poService.getById(this.purchaseOrder.originalPOId).subscribe({
      next: (originalPO) => {
        this.purchaseOrder = originalPO;
        this.loading = false;
        this.messageService.add({
          severity: 'info',
          summary: 'Thông báo',
          detail: 'Đang xem PO gốc (chỉ xem, không thể chỉnh sửa)'
        });
      },
      error: (error) => {
        console.error('Error loading original PO:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải PO gốc'
        });
        this.loading = false;
        this.isViewingOriginal = false;
      }
    });
  }

  // Back to Operation PO
  backToOperationPO(): void {
    if (!this.operationPOId) return;

    this.isViewingOriginal = false;
    this.poId = this.operationPOId;
    this.operationPOId = null;
    this.loadPODetail();
  }

  // Approve for PMC
  approveForPMC(): void {
    if (!this.purchaseOrder) return;

    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn phê duyệt PO ${this.purchaseOrder.poNumber} cho PMC?`,
      header: 'Xác nhận phê duyệt',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Phê duyệt',
      rejectLabel: 'Hủy',
      accept: () => {
        this.poService.approveForPMC({
          purchaseOrderId: this.purchaseOrder!.id
        }).subscribe({
          next: (response) => {
            this.loadPODetail();
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: response.message || 'Phê duyệt PO thành công'
            });
          },
          error: (error) => {
            console.error('Approve error:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi',
              detail: error.error?.message || 'Không thể phê duyệt PO'
            });
          }
        });
      }
    });
  }
}

