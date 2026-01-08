import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { POOperation, PurchaseOrder } from '../../../models/purchase-order.interface';
import { Tabulator, EditModule, FormatModule, InteractionModule, SortModule, ResizeColumnsModule } from 'tabulator-tables';
import { PurchaseOrderService } from '../../../services/purchase-order.service';
import { CustomerService } from '../../../services/customer.service';
import { ProcessingTypeService } from '../../../services/processing-type.service';
import { ProductService } from '../../../services/system/product.service';
import { PartService, PartDetail } from '../../../services/part.service';
import { MessageService } from 'primeng/api';
import { Customer } from '../../../models/customer.interface';
import { ProcessingType } from '../../../models/processing-type.interface';
import { Product } from '../../../models/interface/product.interface';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

// Register modules for inline editing, formatting, interaction, sorting, and resizing
Tabulator.registerModule([EditModule, FormatModule, InteractionModule, SortModule, ResizeColumnsModule]);

@Component({
  selector: 'app-po-excel-table',
  templateUrl: './po-excel-table.component.html',
  styleUrls: ['./po-excel-table.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class POExcelTableComponent implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  @Input() purchaseOrder!: PurchaseOrder;
  @Input() processingType!: 'EP_NHUA' | 'PHUN_IN' | 'LAP_RAP';
  @Input() readonly = false; // If true, disable all editing
  @Output() dataChanged = new EventEmitter<void>();
  @Output() saved = new EventEmitter<PurchaseOrder>();

  @ViewChild('tableContainer', { static: false }) tableContainer!: ElementRef;

  tabulatorInstance!: Tabulator;
  tableData: any[] = [];
  tableColumns: any[] = [];

  // PO Form data
  poFormData: any = {
    poNumber: '',
    poDate: null,
    expectedDeliveryDate: null,
    customerId: '',
    processingType: '',
    notes: ''
  };

  customers: Customer[] = [];
  processingTypes: ProcessingType[] = [];
  products: Product[] = [];
  parts: PartDetail[] = [];
  loadingCustomers = false;
  loadingProcessingTypes = false;
  loadingProducts = false;
  loadingParts = false;
  saving = false;

  private isInitialized = false;
  private pendingChanges: Map<string, any> = new Map();
  private saveTimeout: any;
  private isSaving = false;

  constructor(
    private poService: PurchaseOrderService,
    private customerService: CustomerService,
    private processingTypeService: ProcessingTypeService,
    private productService: ProductService,
    private partService: PartService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadCustomers();
    this.loadProcessingTypes();
    this.loadProducts();
    this.loadParts();
    this.initializeFormData();
  }

  initializeFormData(): void {
    if (this.purchaseOrder) {
      this.poFormData = {
        poNumber: this.purchaseOrder.poNumber || '',
        poDate: this.purchaseOrder.poDate ? new Date(this.purchaseOrder.poDate) : null,
        expectedDeliveryDate: this.purchaseOrder.expectedDeliveryDate ? new Date(this.purchaseOrder.expectedDeliveryDate) : null,
        customerId: this.purchaseOrder.customerId || '',
        processingType: this.purchaseOrder.processingType || '',
        notes: this.purchaseOrder.notes || ''
      };
    }
  }

  loadCustomers(): void {
    this.loadingCustomers = true;
    this.customerService.getAll().subscribe({
      next: (customers) => {
        this.customers = customers;
        this.loadingCustomers = false;
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        this.loadingCustomers = false;
      }
    });
  }

  loadProcessingTypes(): void {
    this.loadingProcessingTypes = true;
    this.processingTypeService.getAll().subscribe({
      next: (types) => {
        this.processingTypes = types;
        this.loadingProcessingTypes = false;
      },
      error: (error) => {
        console.error('Error loading processing types:', error);
        this.loadingProcessingTypes = false;
      }
    });
  }

  loadProducts(): void {
    this.loadingProducts = true;
    this.productService.getAll().subscribe({
      next: (products) => {
        this.products = products;
        this.loadingProducts = false;
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.loadingProducts = false;
      }
    });
  }

  loadParts(): void {
    this.loadingParts = true;
    this.partService.getAll().subscribe({
      next: (parts) => {
        this.parts = parts;
        this.loadingParts = false;
      },
      error: (error) => {
        console.error('Error loading parts:', error);
        this.loadingParts = false;
      }
    });
  }

  getProcessingTypeLabel(code: string): string {
    const type = this.processingTypes.find(t => t.code === code);
    return type ? type.name : code;
  }

  ngAfterViewInit(): void {
    if (this.purchaseOrder && this.processingType && this.tableContainer) {
      this.initializeTable();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['purchaseOrder']) {
      this.initializeFormData();
      if (this.isInitialized && this.tabulatorInstance) {
        this.updateTableData();
      } else if (this.tableContainer) {
        this.initializeTable();
      }
    }
    if (changes['processingType']) {
      if (this.isInitialized && this.tabulatorInstance) {
        this.updateTableData();
      } else if (this.tableContainer) {
        this.initializeTable();
      }
    }
  }

  initializeTable(): void {
    if (!this.tableContainer || !this.purchaseOrder || !this.processingType) {
      return;
    }

    // Destroy existing instance if any
    if (this.tabulatorInstance) {
      try {
        this.tabulatorInstance.destroy();
      } catch (e) {
        console.warn('Error destroying tabulator instance:', e);
      }
      this.tabulatorInstance = null as any;
    }

    this.prepareData();
    this.prepareColumns();

    const container = this.tableContainer.nativeElement;

    this.tabulatorInstance = new Tabulator(container, {
      data: this.tableData,
      columns: this.tableColumns,
      layout: 'fitDataFill',
      height: 'auto',
      editableTitleCancel: true,
      cellEdited: (cell: any) => {
        this.onCellEdit(cell);
      },
      cellEditCancelled: (cell: any) => {
        // Handle edit cancellation if needed
      }
    });

    this.isInitialized = true;
  }

  prepareData(): void {
    if (!this.purchaseOrder?.operations) {
      this.tableData = [];
      return;
    }

    switch (this.processingType) {
      case 'EP_NHUA':
        this.prepareEpNhuaData();
        break;
      case 'PHUN_IN':
        this.preparePhunInData();
        break;
      case 'LAP_RAP':
        this.prepareLapRapData();
        break;
    }
  }

  prepareEpNhuaData(): void {
    const grouped = this.groupOperationsByProductAndModel();
    this.tableData = [];

    grouped.forEach((group, groupIndex) => {
      group.operations.forEach((op: POOperation, opIndex: number) => {
        this.tableData.push({
          id: op.id,
          operationId: op.id,
          rowIndex: this.tableData.length,
          productNumber: group.productCode || '',
          modelNumber: op.modelNumber || group.modelNumber || '',
          partNumber: op.partCode || '',
          partName: op.partName || '',
          material: op.material || group.material || '',
          colorCode: op.colorCode || group.colorCode || '',
          color: op.color || group.color || '',
          cavityQuantity: this.toNumberOrNull(op.cavityQuantity ?? group.cavityQuantity),
          set: this.toNumberOrNull(op.set ?? group.set),
          cycle: this.toNumberOrNull(op.cycleTime ?? group.cycle),
          netWeight: this.toNumberOrNull(op.netWeight),
          totalWeight: this.toNumberOrNull(op.totalWeight ?? group.totalWeight),
          machineType: op.machineType || group.machineType || '',
          requiredMaterial: op.requiredMaterial || group.requiredMaterial || '',
          requiredColor: op.requiredColor || group.requiredColor || '',
          quantity: op.quantity || 0,
          chargeCount: op.chargeCount || 0,
          unitPrice: op.unitPrice || 0,
          totalPrice: op.totalAmount || 0,
          _groupKey: `${group.productCode}_${group.modelNumber}` // For grouping
        });
      });
    });
  }

  preparePhunInData(): void {
    const grouped = this.groupOperationsByProductAndPart();
    this.tableData = [];

    grouped.forEach((group) => {
      const processingMethods = this.getProcessingMethodsForPart(group.partId);

      processingMethods.forEach((method: any) => {
        const op = group.operations.find((o: POOperation) =>
          o.printContent === method.name || o.operationName.includes(method.name)
        );

        // Get values from operation if available, otherwise use method defaults
        const processingCount = op?.chargeCount ?? method.count ?? 0;
        const unitPrice = op?.unitPrice ?? method.unitPrice ?? 0;
        const quantity = op?.quantity ?? group.quantity ?? 0;
        
        // Calculate correctly: Tổng đơn giá = Số lần gia công × Giá mỗi lần
        const totalUnitPrice = processingCount * unitPrice;
        // Calculate correctly: Thành tiền = Tổng đơn giá × Số lượng hợp đồng
        const amount = totalUnitPrice * quantity;

        this.tableData.push({
          id: `${group.partId}_${method.name}`,
          operationId: op?.id || '',
          rowIndex: this.tableData.length,
          productNumber: group.productCode || '',
          sequenceNumber: '',
          partNumber: group.partCode || '',
          sprayPosition: op?.sprayPosition || group.partName || '',
          processingContent: method.name || '',
          processingCount: processingCount,
          unitPrice: unitPrice,
          totalUnitPrice: totalUnitPrice,
          quantity: quantity,
          amount: amount,
          completionDate: op?.completionDate ? this.formatDate(op.completionDate) : '',
          notes: op?.notes || '',
          _groupKey: `${group.productCode}_${group.partCode}`
        });
      });
    });
  }

  prepareLapRapData(): void {
    const grouped = this.groupOperationsByProduct();
    this.tableData = [];

    grouped.forEach((group) => {
      const processingCount = this.toNumberOrNull(group.chargeCount) ?? 0;
      const unitPrice = this.toNumberOrNull(group.unitPrice) ?? 0;
      const quantity = this.toNumberOrNull(group.quantity) ?? 0;

      // Tổng số tiền = Số lần gia công * Đơn giá
      const totalAmount1 = processingCount * unitPrice;
      // Tổng tất số tiền = Tổng số tiền * Số lượng hợp đồng
      const totalAmount2 = totalAmount1 * quantity;

      this.tableData.push({
        id: group.productCode || '',
        operationId: this.purchaseOrder.operations?.find(op => op.productCode === group.productCode)?.id || '',
        rowIndex: this.tableData.length,
        productNumber: group.productCode || '',
        partNumber: group.partCode || '',
        processingContent: group.assemblyContent || group.operationName || '',
        processingCount: processingCount,
        unitPrice: unitPrice,
        totalAmount1: totalAmount1,
        quantity: quantity,
        totalAmount2: totalAmount2,
        completionDate: group.completionDate ? this.formatDate(group.completionDate) : '',
        notes: group.notes || ''
      });
    });
  }

  prepareColumns(): void {
    // Add action column for add and delete buttons (only if not readonly)
    const actionColumn = this.readonly ? null : {
      title: 'Thao tác',
      minWidth: 100,
      formatter: (cell: any) => {
        const row = cell.getRow();
        return `
          <button class="tabulator-add-btn" style="padding: 4px 8px; background: #28a745; color: white; border: none; border-radius: 3px; cursor: pointer; margin-right: 4px;" data-row-id="${row.getData().id}">
            <i class="pi pi-plus" style="font-size: 10px;"></i>
          </button>
          <button class="tabulator-delete-btn" style="padding: 4px 8px; background: #dc3545; color: white; border: none; border-radius: 3px; cursor: pointer;" data-row-id="${row.getData().id}">
            <i class="pi pi-trash" style="font-size: 10px;"></i>
          </button>
        `;
      },
      cellClick: (e: any, cell: any) => {
        const target = e.target.closest('button');
        if (!target) return;

        const rowId = target.getAttribute('data-row-id');
        const row = this.tabulatorInstance.getRow(rowId);
        if (!row) return;

        const rowData = row.getData();

        if (target.classList.contains('tabulator-add-btn')) {
          // Insert row after current row
          this.insertRowAfter(rowData);
        } else if (target.classList.contains('tabulator-delete-btn')) {
          this.deleteRow(rowData);
        }
      },
      headerSort: false,
      resizable: true
    };

    // Helper function to get editor based on readonly flag
    const getEditor = (editorType: string) => this.readonly ? false : editorType;

    switch (this.processingType) {
      case 'EP_NHUA':
        this.tableColumns = [
          ...(actionColumn ? [actionColumn] : []),
          { title: 'Mã sản phẩm', field: 'productNumber', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Số khuôn/Mẫu', field: 'modelNumber', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Mã linh kiện', field: 'partNumber', minWidth: 120, resizable: true, editor: getEditor('input') },
          { title: 'Tên sản phẩm & Chi tiết', field: 'partName', minWidth: 150, resizable: true, editor: getEditor('input') },
          { title: 'Vật liệu', field: 'material', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Mã màu', field: 'colorCode', minWidth: 80, resizable: true, editor: getEditor('input') },
          { title: 'Màu sắc', field: 'color', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Số lỗ khuôn', field: 'cavityQuantity', minWidth: 90, resizable: true, editor: getEditor('number'), editorParams: { min: 0 } },
          { title: 'Bộ', field: 'set', minWidth: 60, resizable: true, editor: getEditor('number'), editorParams: { min: 0 } },
          { title: 'Chu kỳ', field: 'cycle', minWidth: 80, resizable: true, editor: getEditor('number'), editorParams: { min: 0 } },
          { title: 'Trọng lượng tịnh', field: 'netWeight', minWidth: 100, resizable: true, editor: getEditor('number'), editorParams: { min: 0, step: 0.01 } },
          { title: 'Tổng trọng lượng', field: 'totalWeight', minWidth: 120, resizable: true, editor: getEditor('number'), editorParams: { min: 0, step: 0.01 } },
          { title: 'Số máy ép', field: 'machineType', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Vật liệu cần', field: 'requiredMaterial', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Màu cần', field: 'requiredColor', minWidth: 80, resizable: true, editor: getEditor('input') },
          { title: 'Số lượng hợp đồng (PCS)', field: 'quantity', minWidth: 120, resizable: true, editor: getEditor('number'), editorParams: { min: 0 } },
          { title: 'Số lần ép', field: 'chargeCount', minWidth: 80, resizable: true, editor: getEditor('number'), editorParams: { min: 0 } },
          { title: 'Đơn giá (VND)', field: 'unitPrice', minWidth: 100, resizable: true, editor: getEditor('number'), editorParams: { min: 0, step: 0.01 }, formatter: (cell: any) => {
            const value = cell.getValue();
            return value ? this.formatCurrency(value) : '';
          }},
          { title: 'Tổng đơn giá (VND)', field: 'totalPrice', minWidth: 120, resizable: true, editor: false, formatter: (cell: any) => {
            const value = cell.getValue();
            return value ? this.formatCurrency(value) : '';
          }}
        ];
        break;
      case 'PHUN_IN':
        this.tableColumns = [
          ...(actionColumn ? [actionColumn] : []),
          { title: 'Mã sản phẩm', field: 'productNumber', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Mã linh kiện', field: 'partNumber', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Vị trí phun sơn', field: 'sprayPosition', minWidth: 120, resizable: true, editor: getEditor('input') },
          { title: 'Công đoạn', field: 'processingContent', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Số lần gia công', field: 'processingCount', minWidth: 100, resizable: true, editor: getEditor('number'), editorParams: { min: 0 } },
          { title: 'Giá mỗi lần (VND)', field: 'unitPrice', minWidth: 110, resizable: true, editor: getEditor('number'), editorParams: { min: 0, step: 0.01 }, formatter: (cell: any) => {
            const value = cell.getValue();
            return value ? this.formatCurrency(value) : '';
          }},
          { title: 'Tổng đơn giá (VND)', field: 'totalUnitPrice', minWidth: 120, resizable: true, editor: false, formatter: (cell: any) => {
            const value = cell.getValue();
            return value ? this.formatCurrency(value) : '';
          }},
          { title: 'Số lượng hợp đồng (PCS)', field: 'quantity', minWidth: 120, resizable: true, editor: getEditor('number'), editorParams: { min: 0 } },
          { title: 'Thành tiền (VND)', field: 'amount', minWidth: 120, resizable: true, editor: false, formatter: (cell: any) => {
            const value = cell.getValue();
            return value ? this.formatCurrency(value) : '';
          }},
          { title: 'Ngày hoàn thành', field: 'completionDate', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Ghi chú', field: 'notes', minWidth: 150, resizable: true, editor: getEditor('input') }
        ];
        break;
      case 'LAP_RAP':
        this.tableColumns = [
          ...(actionColumn ? [actionColumn] : []),
          { title: 'Mã sản phẩm', field: 'productNumber', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Nội dung gia công', field: 'processingContent', minWidth: 150, resizable: true, editor: getEditor('input') },
          { title: 'Số lần gia công', field: 'processingCount', minWidth: 100, resizable: true, editor: getEditor('number'), editorParams: { min: 0 } },
          { title: 'Đơn giá (VND)', field: 'unitPrice', minWidth: 100, resizable: true, editor: getEditor('number'), editorParams: { min: 0, step: 0.01 }, formatter: (cell: any) => {
            const value = cell.getValue();
            return value ? this.formatCurrency(value) : '';
          }},
          { title: 'Tổng số tiền (VND)', field: 'totalAmount1', minWidth: 120, resizable: true, editor: false, formatter: (cell: any) => {
            const value = cell.getValue();
            return value ? this.formatCurrency(value) : '';
          }},
          { title: 'Số lượng hợp đồng (PCS)', field: 'quantity', minWidth: 120, resizable: true, editor: getEditor('number'), editorParams: { min: 0 } },
          { title: 'Tổng tất số tiền (VND)', field: 'totalAmount2', minWidth: 120, resizable: true, editor: false, formatter: (cell: any) => {
            const value = cell.getValue();
            return value ? this.formatCurrency(value) : '';
          }},
          { title: 'Ngày hoàn thành', field: 'completionDate', minWidth: 100, resizable: true, editor: getEditor('input') },
          { title: 'Ghi chú', field: 'notes', minWidth: 150, resizable: true, editor: getEditor('input') }
        ];
        break;
    }
  }

  onCellEdit(cell: any): void {
    if (this.isSaving || this.readonly) return; // Disable editing if readonly

    try {
      const field = cell.getField();
      const rowData = cell.getRow().getData();
      let newValue = cell.getValue();
      const oldValue = cell.getOldValue();

      console.log('Cell edited:', { field, newValue, oldValue, rowData });

      // Skip if value hasn't changed
      if (oldValue === newValue) {
        console.log('Value unchanged, skipping');
        return;
      }

      // Skip calculated fields (they shouldn't be editable anyway)
      if (this.isCalculatedField(field)) {
        console.log('Calculated field, skipping');
        return;
      }

      // Convert numeric fields to numbers
      if (this.isNumericField(field)) {
        newValue = this.toNumberOrNull(newValue) ?? 0;
      }

      // Update row data
      rowData[field] = newValue;

      // Mark for save
      this.markForSave(rowData.operationId, field, newValue, rowData);

      // Recalculate totals
      this.recalculateRow(rowData);

      console.log('Pending changes count:', this.pendingChanges.size);
      // Don't auto-save, wait for user to click Save button
      // this.debounceSave();
    } catch (error) {
      console.error('Error in cell edit:', error);
    }
  }

  recalculateRow(row: any): void {
    if (!this.tabulatorInstance) return;

    try {
      const rowComponent = this.tabulatorInstance.getRow(row.id);
      if (!rowComponent) return;

      switch (this.processingType) {
        case 'EP_NHUA':
          const quantity = this.toNumberOrNull(row.quantity) ?? 0;
          const chargeCount = this.toNumberOrNull(row.chargeCount) ?? 0;
          const unitPrice = this.toNumberOrNull(row.unitPrice) ?? 0;
          const totalPrice = quantity * chargeCount * unitPrice;
          row.totalPrice = totalPrice;
          rowComponent.update({ totalPrice });
          break;
        case 'PHUN_IN':
          const processingCount = this.toNumberOrNull(row.processingCount) ?? 0;
          const unitPricePhun = this.toNumberOrNull(row.unitPrice) ?? 0;
          const quantityPhun = this.toNumberOrNull(row.quantity) ?? 0;
          const totalUnitPrice = processingCount * unitPricePhun;
          const amount = totalUnitPrice * quantityPhun;
          row.totalUnitPrice = totalUnitPrice;
          row.amount = amount;
          rowComponent.update({ totalUnitPrice, amount });
          break;
        case 'LAP_RAP':
          // Tổng số tiền = Số lần gia công * Đơn giá
          const processingCountLap = this.toNumberOrNull(row.processingCount) ?? 0;
          const unitPriceLap = this.toNumberOrNull(row.unitPrice) ?? 0;
          const totalAmount1 = processingCountLap * unitPriceLap;
          // Tổng tất số tiền = Tổng số tiền * Số lượng hợp đồng
          const quantityLap = this.toNumberOrNull(row.quantity) ?? 0;
          const totalAmount2 = totalAmount1 * quantityLap;
          row.totalAmount1 = totalAmount1;
          row.totalAmount2 = totalAmount2;
          rowComponent.update({ totalAmount1, totalAmount2 });
          break;
      }
    } catch (error) {
      console.error('Error recalculating row:', error);
    }
  }

  markForSave(operationId: string, field: string, value: any, row: any): void {
    // For new rows, use row id as key
    const key = operationId ? `${operationId}_${field}` : `${row.id}_${field}`;
    const isNew = !operationId || row.id.startsWith('new_');
    this.pendingChanges.set(key, { operationId, field, value, row, isNew });
    console.log('Marked for save:', { key, operationId, field, value, isNew, pendingCount: this.pendingChanges.size });
  }

  debounceSave(): void {
    if (this.saveTimeout) {
      clearTimeout(this.saveTimeout);
    }

    this.saveTimeout = setTimeout(() => {
      this.saveChanges();
    }, 2000);
  }

  async saveChanges(): Promise<void> {
    if (this.pendingChanges.size === 0) {
      console.log('No pending changes to save');
      return;
    }

    if (this.isSaving) {
      console.log('Already saving, skipping...');
      return;
    }

    console.log('Saving changes, count:', this.pendingChanges.size);
    this.isSaving = true;
    const changesToSave = new Map(this.pendingChanges);
    this.pendingChanges.clear();

    const changesByOperation = new Map<string, any>();
    const newRowsToCreate = new Map<string, any>();

    changesToSave.forEach((change, key) => {
      const { operationId, field, value, row, isNew } = change;

      console.log('Processing change:', { key, operationId, field, value, isNew, rowId: row.id });

      // Handle new rows (row starts with 'new_' or no operationId)
      // Note: operationId can be empty string for existing rows that don't have an operation yet
      // So we check row.id to determine if it's truly a new row
      if (isNew || row.id.startsWith('new_')) {
        const rowKey = row.id;
        if (!newRowsToCreate.has(rowKey)) {
          newRowsToCreate.set(rowKey, {
            row,
            data: {}
          });
        }
        const update = this.mapFieldToUpdate(field, value);
        if (Object.keys(update).length > 0) {
          Object.assign(newRowsToCreate.get(rowKey)!.data, update);
          console.log('Added to new rows:', { rowKey, update, mergedData: newRowsToCreate.get(rowKey)!.data });
        }
        return;
      }

      // If operationId is empty but row is not new, skip it (this shouldn't happen for existing operations)
      if (!operationId || operationId === '') {
        console.warn('Skipping change with empty operationId for existing row:', { rowId: row.id, field, value });
        return;
      }

      // Handle existing operations
      if (!changesByOperation.has(operationId)) {
        changesByOperation.set(operationId, {
          operationId,
          updates: {}
        });
      }

      const update = this.mapFieldToUpdate(field, value);
      if (Object.keys(update).length > 0) {
        Object.assign(changesByOperation.get(operationId)!.updates, update);
        console.log('Added to existing operation:', { operationId, update, mergedUpdates: changesByOperation.get(operationId)!.updates });
      }
    });

    console.log('Changes grouped:', {
      newRows: newRowsToCreate.size,
      existingOps: changesByOperation.size,
      newRowKeys: Array.from(newRowsToCreate.keys()),
      existingOpIds: Array.from(changesByOperation.keys())
    });

    const savePromises: Promise<any>[] = [];

    // Update existing operations
    savePromises.push(...Array.from(changesByOperation.values()).map(change => {
      const operation = this.findOperationById(change.operationId);
      if (!operation) return Promise.resolve();

      return new Promise((resolve, reject) => {
        this.poService.updateOperation(
          this.purchaseOrder.id,
          change.operationId,
          {
            ...this.buildUpdateData(operation, change.updates)
          }
        ).subscribe({
          next: () => resolve(undefined),
          error: (err) => reject(err)
        });
      });
    }));

    // Create new operations
    savePromises.push(...Array.from(newRowsToCreate.values()).map(({ row, data }) => {
      return new Promise((resolve, reject) => {
        // Build operation data for creation
        const operationData = this.buildCreateOperationData(row, data);

        this.poService.createOperation(this.purchaseOrder.id, operationData).subscribe({
          next: (createdOperation: any) => {
            // Update row with new operation ID
            if (this.tabulatorInstance) {
              const rowComponent = this.tabulatorInstance.getRow(row.id);
              if (rowComponent) {
                rowComponent.update({
                  id: createdOperation.id,
                  operationId: createdOperation.id
                });
                row.id = createdOperation.id;
                row.operationId = createdOperation.id;
              }
            }
            resolve(createdOperation);
          },
          error: (err) => reject(err)
        });
      });
    }));

    try {
      console.log('Executing save promises. Count:', savePromises.length);
      await Promise.all(savePromises);
      console.log('All changes saved successfully');
      // Don't show message here, let saveAll() handle it
      this.dataChanged.emit();
    } catch (error: any) {
      console.error('Error saving changes:', error);
      // Restore pending changes on error
      changesToSave.forEach((change, key) => {
        this.pendingChanges.set(key, change);
      });
      const errorMessage = error?.error?.message || error?.message || 'Không thể lưu thay đổi';
      throw new Error(errorMessage);
    } finally {
      this.isSaving = false;
    }
  }

  buildCreateOperationData(row: any, updates: any): any {
    // Get first operation to get required IDs (partId, processingTypeId)
    const firstOperation = this.purchaseOrder.operations?.[0];
    if (!firstOperation) {
      throw new Error('Không tìm thấy operation để lấy thông tin cần thiết');
    }

    const baseData: any = {
      partId: firstOperation.partId || '',
      processingTypeId: firstOperation.processingTypeId || '',
      sequenceOrder: (this.purchaseOrder.operations?.length || 0) + 1
    };

    switch (this.processingType) {
      case 'EP_NHUA':
        return {
          ...baseData,
          operationName: updates.operationName || 'EP_NHUA',
          modelNumber: updates.modelNumber || row.modelNumber || '',
          material: updates.material || row.material || '',
          colorCode: updates.colorCode || row.colorCode || '',
          color: updates.color || row.color || '',
          cavityQuantity: this.toNumberOrNull(updates.cavityQuantity ?? row.cavityQuantity),
          set: this.toNumberOrNull(updates.set ?? row.set),
          cycleTime: this.toNumberOrNull(updates.cycleTime ?? row.cycle),
          netWeight: this.toNumberOrNull(updates.netWeight ?? row.netWeight),
          totalWeight: this.toNumberOrNull(updates.totalWeight ?? row.totalWeight),
          machineType: updates.machineType || row.machineType || '',
          chargeCount: (this.toNumberOrNull(updates.chargeCount ?? row.chargeCount) ?? 0),
          unitPrice: (this.toNumberOrNull(updates.unitPrice ?? row.unitPrice) ?? 0),
          quantity: (this.toNumberOrNull(updates.quantity ?? row.quantity) ?? 0)
        };
      case 'PHUN_IN':
        return {
          ...baseData,
          operationName: updates.operationName || 'PHUN_IN',
          printContent: updates.printContent || row.processingContent || '',
          sprayPosition: updates.sprayPosition || row.sprayPosition || '',
          chargeCount: (this.toNumberOrNull(updates.chargeCount ?? row.processingCount) ?? 0),
          unitPrice: (this.toNumberOrNull(updates.unitPrice ?? row.unitPrice) ?? 0),
          quantity: (this.toNumberOrNull(updates.quantity ?? row.quantity) ?? 0),
          completionDate: updates.completionDate || row.completionDate || null,
          notes: updates.notes || row.notes || ''
        };
      case 'LAP_RAP':
        return {
          ...baseData,
          operationName: updates.operationName || 'LAP_RAP',
          assemblyContent: updates.assemblyContent || row.processingContent || '',
          chargeCount: (this.toNumberOrNull(updates.chargeCount ?? row.processingCount) ?? 0),
          unitPrice: (this.toNumberOrNull(updates.unitPrice ?? row.unitPrice) ?? 0),
          quantity: (this.toNumberOrNull(updates.quantity ?? row.quantity) ?? 0),
          completionDate: updates.completionDate || row.completionDate || null,
          notes: updates.notes || row.notes || ''
        };
      default:
        return baseData;
    }
  }

  isCalculatedField(field: string): boolean {
    switch (this.processingType) {
      case 'EP_NHUA':
        return field === 'totalPrice';
      case 'PHUN_IN':
        return field === 'totalUnitPrice' || field === 'amount';
      case 'LAP_RAP':
        return field === 'totalAmount1' || field === 'totalAmount2';
      default:
        return false;
    }
  }

  isNumericField(field: string): boolean {
    switch (this.processingType) {
      case 'EP_NHUA':
        return ['quantity', 'chargeCount', 'unitPrice', 'cavityQuantity', 'set', 'cycle', 'netWeight', 'totalWeight'].includes(field);
      case 'PHUN_IN':
        return ['processingCount', 'unitPrice', 'quantity'].includes(field);
      case 'LAP_RAP':
        return ['processingCount', 'unitPrice', 'quantity'].includes(field);
      default:
        return false;
    }
  }

  mapFieldToUpdate(field: string, value: any): any {
    const updates: any = {};

    // Handle ProductCode and PartCode for all processing types
    if (field === 'productNumber') updates.productCode = value;
    else if (field === 'partNumber') updates.partCode = value;

    switch (this.processingType) {
      case 'EP_NHUA':
        if (field === 'modelNumber') updates.modelNumber = value;
        else if (field === 'material') updates.material = value;
        else if (field === 'colorCode') updates.colorCode = value;
        else if (field === 'color') updates.color = value;
        else if (field === 'cavityQuantity') updates.cavityQuantity = this.toNumberOrNull(value);
        else if (field === 'set') updates.set = this.toNumberOrNull(value);
        else if (field === 'cycle') updates.cycleTime = this.toNumberOrNull(value);
        else if (field === 'netWeight') updates.netWeight = this.toNumberOrNull(value);
        else if (field === 'totalWeight') updates.totalWeight = this.toNumberOrNull(value);
        else if (field === 'machineType') updates.machineType = value;
        else if (field === 'requiredMaterial') updates.requiredMaterial = value;
        else if (field === 'requiredColor') updates.requiredColor = value;
        else if (field === 'quantity') updates.quantity = this.toNumberOrNull(value) ?? 0;
        else if (field === 'chargeCount') updates.chargeCount = this.toNumberOrNull(value) ?? 0;
        else if (field === 'unitPrice') updates.unitPrice = this.toNumberOrNull(value) ?? 0;
        break;
      case 'PHUN_IN':
        if (field === 'sprayPosition') updates.sprayPosition = value;
        else if (field === 'processingContent') updates.printContent = value;
        else if (field === 'processingCount') updates.chargeCount = this.toNumberOrNull(value) ?? 0;
        else if (field === 'unitPrice') updates.unitPrice = this.toNumberOrNull(value) ?? 0;
        else if (field === 'quantity') updates.quantity = this.toNumberOrNull(value) ?? 0;
        else if (field === 'completionDate') updates.completionDate = value;
        else if (field === 'notes') updates.notes = value;
        break;
      case 'LAP_RAP':
        if (field === 'processingContent') updates.assemblyContent = value;
        else if (field === 'processingCount') updates.chargeCount = this.toNumberOrNull(value) ?? 0;
        else if (field === 'unitPrice') updates.unitPrice = this.toNumberOrNull(value) ?? 0;
        else if (field === 'quantity') updates.quantity = this.toNumberOrNull(value) ?? 0;
        else if (field === 'completionDate') updates.completionDate = value;
        else if (field === 'notes') updates.notes = value;
        break;
    }

    return updates;
  }

  buildUpdateData(operation: POOperation, updates: any): any {
    // For nullable numeric fields, convert empty strings to null
    // For required numeric fields, ensure they're numbers
    const result: any = {
      operationName: updates.printContent || updates.assemblyContent || operation.operationName,
      chargeCount: updates.chargeCount !== undefined ? (this.toNumberOrNull(updates.chargeCount) ?? operation.chargeCount ?? 0) : operation.chargeCount,
      unitPrice: updates.unitPrice !== undefined ? (this.toNumberOrNull(updates.unitPrice) ?? operation.unitPrice ?? 0) : operation.unitPrice,
      quantity: updates.quantity !== undefined ? (this.toNumberOrNull(updates.quantity) ?? operation.quantity ?? 0) : operation.quantity,
      sprayPosition: updates.sprayPosition ?? operation.sprayPosition,
      printContent: updates.printContent ?? operation.printContent,
      cycleTime: updates.cycleTime !== undefined ? this.toNumberOrNull(updates.cycleTime) : operation.cycleTime,
      assemblyContent: updates.assemblyContent ?? operation.assemblyContent,
      modelNumber: updates.modelNumber ?? operation.modelNumber,
      material: updates.material ?? operation.material,
      colorCode: updates.colorCode ?? operation.colorCode,
      color: updates.color ?? operation.color,
      cavityQuantity: updates.cavityQuantity !== undefined ? this.toNumberOrNull(updates.cavityQuantity) : operation.cavityQuantity,
      set: updates.set !== undefined ? this.toNumberOrNull(updates.set) : operation.set,
      netWeight: updates.netWeight !== undefined ? this.toNumberOrNull(updates.netWeight) : operation.netWeight,
      totalWeight: updates.totalWeight !== undefined ? this.toNumberOrNull(updates.totalWeight) : operation.totalWeight,
      machineType: updates.machineType ?? operation.machineType,
      requiredMaterial: updates.requiredMaterial ?? operation.requiredMaterial,
      requiredColor: updates.requiredColor ?? operation.requiredColor,
      completionDate: updates.completionDate ?? operation.completionDate,
      notes: updates.notes ?? operation.notes,
      // ProductCode and PartCode for updating relationships
      productCode: updates.productCode ?? operation.productCode,
      partCode: updates.partCode ?? operation.partCode
    };
    return result;
  }

  findOperationById(id: string): POOperation | null {
    return this.purchaseOrder.operations?.find(op => op.id === id) || null;
  }

  updateTableData(): void {
    if (!this.tabulatorInstance) {
      this.initializeTable();
      return;
    }

    this.prepareData();
    this.prepareColumns();
    this.tabulatorInstance.setColumns(this.tableColumns);
    this.tabulatorInstance.setData(this.tableData);
  }

  formatDate(date: Date | string): string {
    if (!date) return '';
    const d = new Date(date);
    return `${d.getDate().toString().padStart(2, '0')}/${(d.getMonth() + 1).toString().padStart(2, '0')}/${d.getFullYear()}`;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  }

  // Helper methods for grouping
  groupOperationsByProductAndModel(): any[] {
    const groups = new Map<string, any>();

    this.purchaseOrder.operations?.forEach((op: POOperation) => {
      const key = `${op.productCode || ''}_${op.modelNumber || ''}`;

      if (!groups.has(key)) {
        groups.set(key, {
          productCode: op.productCode,
          productName: op.productName,
          modelNumber: op.modelNumber || '',
          material: op.material || '',
          colorCode: op.colorCode || '',
          color: op.color || '',
          cavityQuantity: op.cavityQuantity ?? null,
          set: op.set ?? null,
          cycle: op.cycleTime ?? null,
          totalWeight: op.totalWeight ?? null,
          machineType: op.machineType || '',
          requiredMaterial: op.requiredMaterial || '',
          requiredColor: op.requiredColor || '',
          operations: []
        });
      }

      groups.get(key)!.operations.push(op);
    });

    return Array.from(groups.values());
  }

  groupOperationsByProductAndPart(): any[] {
    const groups = new Map<string, any>();

    this.purchaseOrder.operations?.forEach((op: POOperation) => {
      const key = `${op.productCode || ''}_${op.partCode || ''}`;

      if (!groups.has(key)) {
        groups.set(key, {
          productCode: op.productCode,
          productName: op.productName,
          partCode: op.partCode,
          partName: op.partName,
          partId: op.partId,
          quantity: op.quantity,
          operations: []
        });
      }

      groups.get(key)!.operations.push(op);
    });

    return Array.from(groups.values());
  }

  groupOperationsByProduct(): any[] {
    const groups = new Map<string, any>();

    this.purchaseOrder.operations?.forEach((op: POOperation) => {
      const key = op.productCode || '';

      if (!groups.has(key)) {
        groups.set(key, {
          productCode: op.productCode,
          productName: op.productName,
          partCode: op.partCode,
          assemblyContent: op.assemblyContent || op.operationName,
          chargeCount: op.chargeCount,
          unitPrice: op.unitPrice,
          totalAmount: op.totalAmount,
          quantity: op.quantity,
          completionDate: op.completionDate,
          notes: op.notes
        });
      } else {
        const group = groups.get(key)!;
        group.totalAmount = (group.totalAmount || 0) + (op.totalAmount || 0);
      }
    });

    return Array.from(groups.values());
  }

  getProcessingMethodsForPart(partId: string): any[] {
    return [
      { name: '散枪', count: 0, unitPrice: 217, totalPrice: 0, totalAmount: 0 },
      { name: '夹模', count: 0, unitPrice: 217, totalPrice: 0, totalAmount: 0 },
      { name: '边摸', count: 0, unitPrice: 217, totalPrice: 0, totalAmount: 0 },
      { name: '移印', count: 0, unitPrice: 122, totalPrice: 0, totalAmount: 0 },
      { name: '手油', count: 0, unitPrice: 238, totalPrice: 0, totalAmount: 0 }
    ];
  }

  addRow(): void {
    if (!this.tabulatorInstance || !this.purchaseOrder) return;

    // Create a new empty row based on processing type
    const newRow: any = {
      id: `new_${Date.now()}`,
      operationId: '',
      rowIndex: this.tableData.length
    };

    switch (this.processingType) {
      case 'EP_NHUA':
        Object.assign(newRow, {
          productNumber: '',
          modelNumber: '',
          partNumber: '',
          partName: '',
          material: '',
          colorCode: '',
          color: '',
          cavityQuantity: 0,
          set: 0,
          cycle: 0,
          netWeight: 0,
          totalWeight: 0,
          machineType: '',
          requiredMaterial: '',
          requiredColor: '',
          quantity: 0,
          chargeCount: 0,
          unitPrice: 0,
          totalPrice: 0
        });
        break;
      case 'PHUN_IN':
        Object.assign(newRow, {
          productNumber: '',
          sequenceNumber: '',
          partNumber: '',
          sprayPosition: '',
          processingContent: '',
          processingCount: 0,
          unitPrice: 0,
          totalUnitPrice: 0,
          quantity: 0,
          amount: 0,
          completionDate: '',
          notes: ''
        });
        break;
      case 'LAP_RAP':
        Object.assign(newRow, {
          productNumber: '',
          partNumber: '',
          processingContent: '',
          processingCount: 0,
          unitPrice: 0,
          totalAmount1: 0,
          quantity: 0,
          totalAmount2: 0,
          completionDate: '',
          notes: ''
        });
        break;
    }

    // Add row to table
    this.tabulatorInstance.addRow(newRow);
    this.tableData.push(newRow);
  }

  insertRowAfter(afterRowData: any): void {
    if (!this.tabulatorInstance || !this.purchaseOrder) return;

    // Calculate sequenceOrder based on the position
    // This ensures that when a row is inserted after another row, it maintains its position after save
    let calculatedSequenceOrder: number;
    
    // Find the index of the row to insert after
    const afterRowIndex = this.tableData.findIndex((r: any) => r.id === afterRowData.id);
    
    if (afterRowIndex >= 0) {
      // Find the sequenceOrder of the row we're inserting after
      let afterRowSequenceOrder: number | undefined;
      
      if (afterRowData.operationId) {
        // Row has an existing operation - get its sequenceOrder
        const afterOperation = this.purchaseOrder.operations?.find(op => op.id === afterRowData.operationId);
        afterRowSequenceOrder = afterOperation?.sequenceOrder;
      } else if (afterRowData._sequenceOrder !== undefined) {
        // Row is a new row with pre-calculated sequenceOrder
        afterRowSequenceOrder = afterRowData._sequenceOrder;
      }
      
      // If we have sequenceOrder from the after row, use it
      if (afterRowSequenceOrder !== undefined) {
        // Find the next row's sequenceOrder to check if we're inserting between rows
        const nextRowIndex = afterRowIndex + 1;
        let nextRowSequenceOrder: number | undefined;
        
        if (nextRowIndex < this.tableData.length) {
          const nextRow = this.tableData[nextRowIndex];
          if (nextRow.operationId) {
            const nextOperation = this.purchaseOrder.operations?.find(op => op.id === nextRow.operationId);
            nextRowSequenceOrder = nextOperation?.sequenceOrder;
          } else if (nextRow._sequenceOrder !== undefined) {
            // Next row is also a new row with calculated sequenceOrder
            nextRowSequenceOrder = nextRow._sequenceOrder;
          }
        }
        
        // Calculate new sequenceOrder
        // If next row exists and has a higher sequenceOrder, we insert between them
        // Otherwise, we insert after the last row
        if (nextRowSequenceOrder !== undefined && nextRowSequenceOrder > afterRowSequenceOrder) {
          // Insert between two rows - use afterRowSequenceOrder + 1
          // Note: If there are conflicts, backend will handle sorting
          calculatedSequenceOrder = afterRowSequenceOrder + 1;
        } else {
          // Insert after the last row or no next row - increment by 1
          calculatedSequenceOrder = afterRowSequenceOrder + 1;
        }
      } else {
        // Fallback: calculate based on position in tableData
        // Get all operations sorted by sequenceOrder
        const sortedOperations = [...(this.purchaseOrder.operations || [])].sort((a, b) => a.sequenceOrder - b.sequenceOrder);
        
        // Count how many existing operations are before this position
        let operationsBeforePosition = 0;
        for (let i = 0; i < afterRowIndex && i < this.tableData.length; i++) {
          if (this.tableData[i].operationId && !this.tableData[i].id.startsWith('new_')) {
            operationsBeforePosition++;
          }
        }
        
        if (sortedOperations.length > 0 && operationsBeforePosition < sortedOperations.length) {
          // Use sequenceOrder of operation at the calculated position
          calculatedSequenceOrder = sortedOperations[operationsBeforePosition].sequenceOrder + 1;
        } else {
          // Default: use length + 1
          calculatedSequenceOrder = (this.purchaseOrder.operations?.length || 0) + 1;
        }
      }
    } else {
      // If row not found, add at end
      calculatedSequenceOrder = (this.purchaseOrder.operations?.length || 0) + 1;
    }

    // Create a new empty row based on processing type
    const newRow: any = {
      id: `new_${Date.now()}`,
      operationId: '',
      rowIndex: this.tableData.length,
      _sequenceOrder: calculatedSequenceOrder // Store calculated sequenceOrder
    };

    // Create empty row with all fields empty/zero (don't copy from afterRowData)
    switch (this.processingType) {
      case 'EP_NHUA':
        Object.assign(newRow, {
          productNumber: '',
          modelNumber: '',
          partNumber: '',
          partName: '',
          material: '',
          colorCode: '',
          color: '',
          cavityQuantity: 0,
          set: 0,
          cycle: 0,
          netWeight: 0,
          totalWeight: 0,
          machineType: '',
          requiredMaterial: '',
          requiredColor: '',
          quantity: 0,
          chargeCount: 0,
          unitPrice: 0,
          totalPrice: 0
        });
        break;
      case 'PHUN_IN':
        Object.assign(newRow, {
          productNumber: '',
          sequenceNumber: '',
          partNumber: '',
          sprayPosition: '',
          processingContent: '',
          processingCount: 0,
          unitPrice: 0,
          totalUnitPrice: 0,
          quantity: 0,
          amount: 0,
          completionDate: '',
          notes: ''
        });
        break;
      case 'LAP_RAP':
        Object.assign(newRow, {
          productNumber: '',
          partNumber: '',
          processingContent: '',
          processingCount: 0,
          unitPrice: 0,
          totalAmount1: 0,
          quantity: 0,
          totalAmount2: 0,
          completionDate: '',
          notes: ''
        });
        break;
    }

    if (afterRowIndex >= 0) {
      // Insert after the found row
      const insertIndex = afterRowIndex + 1;

      // Insert into tableData array
      this.tableData.splice(insertIndex, 0, newRow);

      // Get the row component to insert after
      const allRows = this.tabulatorInstance.getRows();
      if (allRows.length > afterRowIndex) {
        const afterRowComponent = allRows[afterRowIndex];

        // Use addRow with position parameter (addRow(data, addToTop, referenceRow))
        this.tabulatorInstance.addRow(newRow, false, afterRowComponent);
      } else {
        // Fallback: reload all data
        this.tabulatorInstance.setData(this.tableData);
      }

      // Scroll to the new row after a short delay
      setTimeout(() => {
        const newRowComponent = this.tabulatorInstance.getRow(newRow.id);
        if (newRowComponent) {
          newRowComponent.scrollTo();
        }
      }, 100);
    } else {
      // If row not found, add at end
      this.tabulatorInstance.addRow(newRow);
      this.tableData.push(newRow);
    }
  }

  async deleteRow(rowData: any): Promise<void> {
    if (!this.tabulatorInstance || !this.purchaseOrder) return;

    // If it's a new row (not saved yet), just remove from table
    if (rowData.id.startsWith('new_') || !rowData.operationId) {
      this.tabulatorInstance.deleteRow(rowData.id);
      this.tableData = this.tableData.filter((r: any) => r.id !== rowData.id);
      return;
    }

    // Confirm deletion
    if (!confirm('Bạn có chắc chắn muốn xóa dòng này?')) {
      return;
    }

    try {
      // Delete from server
      await new Promise((resolve, reject) => {
        this.poService.deleteOperation(this.purchaseOrder.id, rowData.operationId).subscribe({
          next: () => resolve(undefined),
          error: (err) => reject(err)
        });
      });

      // Remove from table
      this.tabulatorInstance.deleteRow(rowData.id);
      this.tableData = this.tableData.filter((r: any) => r.id !== rowData.id);

      this.messageService.add({
        severity: 'success',
        summary: 'Thành công',
        detail: 'Đã xóa dòng'
      });

      this.dataChanged.emit();
    } catch (error) {
      console.error('Error deleting row:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Không thể xóa dòng'
      });
    }
  }

  async saveAll(): Promise<void> {
    if (!this.purchaseOrder) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Không có thông tin PO để lưu'
      });
      return;
    }

    this.saving = true;

    try {
      console.log('Saving all table data...');

      // Get all current table data from Tabulator
      const allTableData = this.tabulatorInstance?.getData() || [];
      console.log('Total rows in table:', allTableData.length);

      // Process all rows: update existing and create new ones
      await this.saveAllTableData(allTableData);

      // Then save PO general info
      console.log('Saving PO general info...', this.poFormData);
      const updatedPO = await new Promise<PurchaseOrder>((resolve, reject) => {
        this.poService.updateGeneralInfo(this.purchaseOrder.id, {
          poNumber: this.poFormData.poNumber,
          poDate: this.poFormData.poDate,
          expectedDeliveryDate: this.poFormData.expectedDeliveryDate,
          customerId: this.poFormData.customerId,
          processingType: this.poFormData.processingType,
          notes: this.poFormData.notes
        }).subscribe({
          next: (updated) => {
            console.log('PO general info saved:', updated);
            this.purchaseOrder = updated;
            resolve(updated);
          },
          error: (err) => {
            console.error('Error saving PO general info:', err);
            reject(err);
          }
        });
      });

      // Clear pending changes after successful save
      this.pendingChanges.clear();

      this.messageService.add({
        severity: 'success',
        summary: 'Thành công',
        detail: 'Đã lưu tất cả thay đổi'
      });

      this.saved.emit(updatedPO);
      this.dataChanged.emit();
    } catch (error: any) {
      console.error('Error saving:', error);
      const errorMessage = error?.error?.message || error?.message || 'Không thể lưu thay đổi';
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: errorMessage
      });
    } finally {
      this.saving = false;
    }
  }

  isRowEmpty(row: any): boolean {
    // Check if all editable fields in the row are empty/zero
    switch (this.processingType) {
      case 'EP_NHUA':
        return (
          (!row.productNumber || row.productNumber.trim() === '') &&
          (!row.modelNumber || row.modelNumber.trim() === '') &&
          (!row.partNumber || row.partNumber.trim() === '') &&
          (!row.partName || row.partName.trim() === '') &&
          (!row.material || row.material.trim() === '') &&
          (!row.colorCode || row.colorCode.trim() === '') &&
          (!row.color || row.color.trim() === '') &&
          (!row.machineType || row.machineType.trim() === '') &&
          (!row.requiredMaterial || row.requiredMaterial.trim() === '') &&
          (!row.requiredColor || row.requiredColor.trim() === '') &&
          (this.toNumberOrNull(row.cavityQuantity) ?? 0) === 0 &&
          (this.toNumberOrNull(row.set) ?? 0) === 0 &&
          (this.toNumberOrNull(row.cycle) ?? 0) === 0 &&
          (this.toNumberOrNull(row.netWeight) ?? 0) === 0 &&
          (this.toNumberOrNull(row.totalWeight) ?? 0) === 0 &&
          (this.toNumberOrNull(row.quantity) ?? 0) === 0 &&
          (this.toNumberOrNull(row.chargeCount) ?? 0) === 0 &&
          (this.toNumberOrNull(row.unitPrice) ?? 0) === 0
        );
      case 'PHUN_IN':
        return (
          (!row.productNumber || row.productNumber.trim() === '') &&
          (!row.partNumber || row.partNumber.trim() === '') &&
          (!row.sprayPosition || row.sprayPosition.trim() === '') &&
          (!row.processingContent || row.processingContent.trim() === '') &&
          (this.toNumberOrNull(row.processingCount) ?? 0) === 0 &&
          (this.toNumberOrNull(row.unitPrice) ?? 0) === 0 &&
          (this.toNumberOrNull(row.quantity) ?? 0) === 0 &&
          (!row.completionDate || row.completionDate.trim() === '') &&
          (!row.notes || row.notes.trim() === '')
        );
      case 'LAP_RAP':
        return (
          (!row.productNumber || row.productNumber.trim() === '') &&
          (!row.partNumber || row.partNumber.trim() === '') &&
          (!row.processingContent || row.processingContent.trim() === '') &&
          (this.toNumberOrNull(row.processingCount) ?? 0) === 0 &&
          (this.toNumberOrNull(row.unitPrice) ?? 0) === 0 &&
          (this.toNumberOrNull(row.quantity) ?? 0) === 0 &&
          (!row.completionDate || row.completionDate.trim() === '') &&
          (!row.notes || row.notes.trim() === '')
        );
      default:
        return false;
    }
  }

  async saveAllTableData(allTableData: any[]): Promise<void> {
    if (!allTableData || allTableData.length === 0) {
      console.log('No table data to save');
      return;
    }

    if (this.isSaving) {
      console.log('Already saving, skipping...');
      return;
    }

    this.isSaving = true;

    try {
      const savePromises: Promise<any>[] = [];
      const existingOperationIds = new Set(
        (this.purchaseOrder.operations || []).map((op: POOperation) => op.id)
      );

      const rowsToDelete: string[] = []; // Track rows to delete after successful save

      for (const row of allTableData) {
        const isNewRow = !row.operationId || row.id.startsWith('new_') || !existingOperationIds.has(row.operationId);
        
        // Handle empty rows
        if (this.isRowEmpty(row)) {
          if (isNewRow) {
            // Skip empty new rows (don't create them)
            console.log('Skipping empty new row:', row.id);
            continue;
          } else {
            // Delete empty existing rows
            console.log('Deleting empty existing row:', row.id, row.operationId);
            rowsToDelete.push(row.id);
            savePromises.push(
              new Promise((resolve, reject) => {
                this.poService.deleteOperation(this.purchaseOrder.id, row.operationId).subscribe({
                  next: () => resolve(undefined),
                  error: (err) => {
                    console.error('Error deleting empty row:', err);
                    reject(err);
                  }
                });
              })
            );
            continue;
          }
        }

        // Recalculate totals before saving to ensure data consistency
        this.recalculateRow(row);

        if (isNewRow) {
          // Validate required fields for new rows
          if (this.processingType === 'LAP_RAP') {
            if (!row.productNumber || row.productNumber.trim() === '') {
              throw new Error(`Dòng mới cần có Mã sản phẩm. Vui lòng nhập Mã sản phẩm trước khi lưu.`);
            }
          }

          // Create new operation
          const operationData = this.buildCreateOperationDataFromRow(row);
          savePromises.push(
            new Promise((resolve, reject) => {
              this.poService.createOperation(this.purchaseOrder.id, operationData).subscribe({
                next: (createdOperation: any) => {
                  // Update row with new operation ID
                  if (this.tabulatorInstance) {
                    const rowComponent = this.tabulatorInstance.getRow(row.id);
                    if (rowComponent) {
                      rowComponent.update({
                        id: createdOperation.id,
                        operationId: createdOperation.id
                      });
                      row.id = createdOperation.id;
                      row.operationId = createdOperation.id;
                    }
                  }
                  resolve(createdOperation);
                },
                error: (err) => {
                  console.error('Error creating operation:', err);
                  reject(err);
                }
              });
            })
          );
        } else if (row.operationId) {
          // Update existing operation
          const operation = this.findOperationById(row.operationId);
          if (operation) {
            const updateData = this.buildUpdateDataFromRow(row, operation);
            savePromises.push(
              new Promise((resolve, reject) => {
                this.poService.updateOperation(
                  this.purchaseOrder.id,
                  row.operationId,
                  updateData
                ).subscribe({
                  next: () => resolve(undefined),
                  error: (err) => reject(err)
                });
              })
            );
          }
        }
      }

      console.log('Executing save promises. Count:', savePromises.length);
      await Promise.all(savePromises);
      console.log('All table data saved successfully');
      
      // Delete empty rows from table after successful save
      if (rowsToDelete.length > 0 && this.tabulatorInstance) {
        rowsToDelete.forEach(rowId => {
          try {
            this.tabulatorInstance.deleteRow(rowId);
            // Also remove from tableData
            const index = this.tableData.findIndex((r: any) => r.id === rowId);
            if (index >= 0) {
              this.tableData.splice(index, 1);
            }
          } catch (error) {
            console.warn('Error deleting row from table:', rowId, error);
          }
        });
      }
      
      this.dataChanged.emit();
    } catch (error: any) {
      console.error('Error saving table data:', error);
      const errorMessage = error?.error?.message || error?.message || 'Không thể lưu dữ liệu bảng';
      throw new Error(errorMessage);
    } finally {
      this.isSaving = false;
    }
  }

  buildCreateOperationDataFromRow(row: any): any {
    // Get first operation to get required IDs (partId, processingTypeId)
    const firstOperation = this.purchaseOrder.operations?.[0];
    if (!firstOperation) {
      throw new Error('Không tìm thấy operation để lấy thông tin cần thiết');
    }

    // Try to find Part based on productCode if provided (fallback for backward compatibility)
    let partId: string | undefined = undefined;
    if (row.productNumber && this.parts && this.parts.length > 0) {
      const matchingPart = this.parts.find(p =>
        p.productCode === row.productNumber ||
        (p.productId && this.products.find(pr => pr.id === p.productId && pr.code === row.productNumber))
      );
      if (matchingPart) {
        partId = matchingPart.id;
      }
    }

    // Use calculated sequenceOrder from row if available, otherwise calculate
    let sequenceOrder: number;
    if (row._sequenceOrder !== undefined) {
      // Use the pre-calculated sequenceOrder from insertRowAfter
      sequenceOrder = row._sequenceOrder;
    } else {
      // Fallback: use length + 1 (for rows added via addRow() at the end)
      sequenceOrder = (this.purchaseOrder.operations?.length || 0) + 1;
    }

    const baseData: any = {
      partId: partId, // Optional - can use ProductCode and PartCode instead
      processingTypeId: firstOperation.processingTypeId || '',
      sequenceOrder: sequenceOrder,
      // Send ProductCode and PartCode to let backend find or create Product/Part
      productCode: row.productNumber || undefined,
      partCode: row.partNumber || undefined
    };

    switch (this.processingType) {
      case 'EP_NHUA':
        return {
          ...baseData,
          operationName: 'EP_NHUA',
          modelNumber: row.modelNumber || '',
          material: row.material || '',
          colorCode: row.colorCode || '',
          color: row.color || '',
          cavityQuantity: this.toNumberOrNull(row.cavityQuantity),
          set: this.toNumberOrNull(row.set),
          cycleTime: this.toNumberOrNull(row.cycle),
          netWeight: this.toNumberOrNull(row.netWeight),
          totalWeight: this.toNumberOrNull(row.totalWeight),
          machineType: row.machineType || '',
          chargeCount: (this.toNumberOrNull(row.chargeCount) ?? 0),
          unitPrice: (this.toNumberOrNull(row.unitPrice) ?? 0),
          quantity: (this.toNumberOrNull(row.quantity) ?? 0)
        };
      case 'PHUN_IN':
        return {
          ...baseData,
          operationName: 'PHUN_IN',
          printContent: row.processingContent || '',
          sprayPosition: row.sprayPosition || '',
          chargeCount: (this.toNumberOrNull(row.processingCount) ?? 0),
          unitPrice: (this.toNumberOrNull(row.unitPrice) ?? 0),
          quantity: (this.toNumberOrNull(row.quantity) ?? 0),
          completionDate: row.completionDate ? this.parseDate(row.completionDate) : null,
          notes: row.notes || ''
        };
      case 'LAP_RAP':
        return {
          ...baseData,
          operationName: 'LAP_RAP',
          assemblyContent: row.processingContent || '',
          chargeCount: (this.toNumberOrNull(row.processingCount) ?? 0),
          unitPrice: (this.toNumberOrNull(row.unitPrice) ?? 0),
          quantity: (this.toNumberOrNull(row.quantity) ?? 0),
          completionDate: row.completionDate ? this.parseDate(row.completionDate) : null,
          notes: row.notes || ''
        };
      default:
        return baseData;
    }
  }

  buildUpdateDataFromRow(row: any, operation: POOperation): any {
    // Base data with ProductCode and PartCode
    // For LAP_RAP, always send productCode if it exists in row (even if empty, send it to allow clearing)
    const baseData: any = {
      productCode: row.productNumber !== undefined ? (row.productNumber || undefined) : operation.productCode,
      partCode: row.partNumber !== undefined ? (row.partNumber || undefined) : operation.partCode
    };

    switch (this.processingType) {
      case 'EP_NHUA':
        return {
          ...baseData,
          operationName: operation.operationName || 'EP_NHUA',
          modelNumber: row.modelNumber !== undefined ? row.modelNumber : operation.modelNumber,
          material: row.material !== undefined ? row.material : operation.material,
          colorCode: row.colorCode !== undefined ? row.colorCode : operation.colorCode,
          color: row.color !== undefined ? row.color : operation.color,
          cavityQuantity: row.cavityQuantity !== undefined ? this.toNumberOrNull(row.cavityQuantity) : operation.cavityQuantity,
          set: row.set !== undefined ? this.toNumberOrNull(row.set) : operation.set,
          cycleTime: row.cycle !== undefined ? this.toNumberOrNull(row.cycle) : operation.cycleTime,
          netWeight: row.netWeight !== undefined ? this.toNumberOrNull(row.netWeight) : operation.netWeight,
          totalWeight: row.totalWeight !== undefined ? this.toNumberOrNull(row.totalWeight) : operation.totalWeight,
          machineType: row.machineType !== undefined ? row.machineType : operation.machineType,
          chargeCount: row.chargeCount !== undefined ? (this.toNumberOrNull(row.chargeCount) ?? 0) : operation.chargeCount,
          unitPrice: row.unitPrice !== undefined ? (this.toNumberOrNull(row.unitPrice) ?? 0) : operation.unitPrice,
          quantity: row.quantity !== undefined ? (this.toNumberOrNull(row.quantity) ?? 0) : operation.quantity
        };
      case 'PHUN_IN':
        return {
          ...baseData,
          operationName: operation.operationName || 'PHUN_IN',
          printContent: row.processingContent !== undefined ? row.processingContent : operation.printContent,
          sprayPosition: row.sprayPosition !== undefined ? row.sprayPosition : operation.sprayPosition,
          chargeCount: row.processingCount !== undefined ? (this.toNumberOrNull(row.processingCount) ?? 0) : operation.chargeCount,
          unitPrice: row.unitPrice !== undefined ? (this.toNumberOrNull(row.unitPrice) ?? 0) : operation.unitPrice,
          quantity: row.quantity !== undefined ? (this.toNumberOrNull(row.quantity) ?? 0) : operation.quantity,
          completionDate: row.completionDate !== undefined && row.completionDate !== ''
            ? this.parseDate(row.completionDate)
            : operation.completionDate,
          notes: row.notes !== undefined ? row.notes : operation.notes
        };
      case 'LAP_RAP':
        return {
          ...baseData,
          operationName: operation.operationName || 'LAP_RAP',
          assemblyContent: row.processingContent !== undefined ? row.processingContent : operation.assemblyContent,
          chargeCount: row.processingCount !== undefined ? (this.toNumberOrNull(row.processingCount) ?? 0) : operation.chargeCount,
          unitPrice: row.unitPrice !== undefined ? (this.toNumberOrNull(row.unitPrice) ?? 0) : operation.unitPrice,
          quantity: row.quantity !== undefined ? (this.toNumberOrNull(row.quantity) ?? 0) : operation.quantity,
          completionDate: row.completionDate !== undefined && row.completionDate !== ''
            ? this.parseDate(row.completionDate)
            : operation.completionDate,
          notes: row.notes !== undefined ? row.notes : operation.notes
        };
      default:
        return baseData;
    }
  }

  parseDate(dateString: string): Date | null {
    if (!dateString) return null;
    // Try to parse DD/MM/YYYY format
    const parts = dateString.split('/');
    if (parts.length === 3) {
      const day = parseInt(parts[0], 10);
      const month = parseInt(parts[1], 10) - 1;
      const year = parseInt(parts[2], 10);
      return new Date(year, month, day);
    }
    // Try standard date parsing
    const date = new Date(dateString);
    return isNaN(date.getTime()) ? null : date;
  }

  toNumberOrNull(value: any): number | null {
    if (value === null || value === undefined) return null;
    if (value === '' || value === ' ') return null;
    const num = typeof value === 'number' ? value : Number(value);
    return isNaN(num) ? null : num;
  }

  cancel(): void {
    // Reset form data to original values
    this.initializeFormData();

    // Clear pending changes
    this.pendingChanges.clear();
    if (this.saveTimeout) {
      clearTimeout(this.saveTimeout);
    }

    // Reload table data
    if (this.tabulatorInstance) {
      this.updateTableData();
    }

    this.messageService.add({
      severity: 'info',
      summary: 'Đã hủy',
      detail: 'Đã khôi phục dữ liệu ban đầu'
    });
  }

  ngOnDestroy(): void {
    if (this.tabulatorInstance) {
      this.tabulatorInstance.destroy();
    }
    if (this.saveTimeout) {
      clearTimeout(this.saveTimeout);
    }
  }
}

