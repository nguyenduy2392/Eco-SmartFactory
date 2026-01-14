import { Component, OnInit, Output, EventEmitter, Input, SimpleChanges, OnChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { StockInService } from '../../../services/stock-in.service';
import { MaterialService } from '../../../services/material.service';
import { WarehouseService } from '../../../services/warehouse.service';
import { CustomerService } from '../../../services/customer.service';
import { UnitOfMeasureService } from '../../../services/unit-of-measure.service';
import {
  StockInRequest,
  StockInMaterialItem,
  POForSelection
} from '../../../models/stock-in.interface';
import { Material } from '../../../models/material.interface';
import { Warehouse } from '../../../models/warehouse.interface';
import { Customer } from '../../../models/customer.interface';
import { UnitOfMeasure } from '../../../models/unit-of-measure.interface';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

@Component({
  selector: 'app-stock-in',
  templateUrl: './stock-in.component.html',
  styleUrls: ['./stock-in.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class StockInComponent implements OnInit, OnChanges {
  @Output() stockInSuccess = new EventEmitter<void>();
  @Input() prefillCustomerId?: string;
  @Input() prefillPOId?: string;
  @Input() prefillPONumber?: string;
  
  stockInForm!: FormGroup;
  loading = false;
  submitting = false;

  // Dropdowns data
  customers: Customer[] = [];
  warehouses: Warehouse[] = [];
  materials: Material[] = [];
  filteredMaterials: Material[] = []; // Materials filtered by selected customer
  unitsOfMeasure: UnitOfMeasure[] = [];
  filteredPOs: POForSelection[] = [];

  // Selected data
  selectedPO: POForSelection | null = null;

  constructor(
    private fb: FormBuilder,
    private stockInService: StockInService,
    private materialService: MaterialService,
    private warehouseService: WarehouseService,
    private customerService: CustomerService,
    private unitOfMeasureService: UnitOfMeasureService,
    private messageService: MessageService
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadWarehouses();
    this.loadMaterials();
    this.loadUnitsOfMeasure();
    
    // Apply prefill data after a short delay to ensure form is ready
    setTimeout(() => {
      this.applyPrefillData();
    }, 800);
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Handle prefill data from parent component
    if (changes['prefillCustomerId'] || changes['prefillPOId'] || changes['prefillPONumber']) {
      if (this.stockInForm) {
        setTimeout(() => {
          this.applyPrefillData();
        }, 100);
      }
    }
  }

  private applyPrefillData(): void {
    if (!this.prefillCustomerId || !this.stockInForm) {
      return;
    }

    console.log('Applying prefill data:', {
      customerId: this.prefillCustomerId,
      poId: this.prefillPOId,
      poNumber: this.prefillPONumber
    });

    this.stockInForm.patchValue({
      customerId: this.prefillCustomerId
    });
    
    // If PO info is provided, load and select it
    if (this.prefillPOId && this.prefillPONumber) {
      console.log('Prefilling PO:', { poId: this.prefillPOId, poNumber: this.prefillPONumber });
      
      this.stockInService.getPOById(this.prefillPOId).subscribe({
        next: (po) => {
          console.log('Loaded PO:', po);
          this.selectedPO = po;
          this.stockInForm.patchValue({
            purchaseOrderId: po.id,
            poNumber: po.poNumber || this.prefillPONumber
          });
          
          console.log('Form after prefill:', this.stockInForm.value);
          
          // Auto-fill purchasePOCode for all rows
          this.materialsArray.controls.forEach(control => {
            control.patchValue({ purchasePOCode: po.poNumber || this.prefillPONumber });
          });
          
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Đã tự động điền thông tin từ PO'
          });
        },
        error: (error) => {
          console.error('Error loading PO:', error);
          // Fallback: manually set both purchaseOrderId and poNumber
          this.stockInForm.patchValue({
            purchaseOrderId: this.prefillPOId,
            poNumber: this.prefillPONumber
          });
          
          console.log('Form after fallback:', this.stockInForm.value);
          
          this.messageService.add({
            severity: 'info',
            summary: 'Thông tin',
            detail: 'Đã điền thông tin PO (không load được chi tiết)'
          });
        }
      });
    }
  }

  initForm(): void {
    this.stockInForm = this.fb.group({
      purchaseOrderId: [null],
      poNumber: [''],
      customerId: ['', Validators.required],
      warehouseId: ['', Validators.required],
      receiptDate: [new Date(), Validators.required],
      receiptNumber: ['', Validators.required],
      notes: [''],
      materials: this.fb.array([])
    });

    // Add first material row
    this.addMaterialRow();
  }

  get materialsArray(): FormArray {
    return this.stockInForm.get('materials') as FormArray;
  }

  createMaterialRow(): FormGroup {
    return this.fb.group({
      materialId: ['', Validators.required],
      quantity: [0, [Validators.required, Validators.min(0.001)]],
      unit: ['', Validators.required],
      batchNumber: [''],
      supplierCode: [''],
      purchasePOCode: [''],
      notes: ['']
    });
  }

  addMaterialRow(): void {
    const newRow = this.createMaterialRow();
    
    // Auto-fill purchasePOCode từ PO Liên Quan nếu có
    if (this.selectedPO && this.selectedPO.poNumber) {
      newRow.patchValue({ purchasePOCode: this.selectedPO.poNumber });
    }
    
    this.materialsArray.push(newRow);
  }

  removeMaterialRow(index: number): void {
    if (this.materialsArray.length > 1) {
      this.materialsArray.removeAt(index);
    } else {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Phải có ít nhất 1 nguyên vật liệu'
      });
    }
  }

  loadCustomers(): void {
    this.loading = true;
    this.customerService.getAll().subscribe({
      next: (data) => {
        this.customers = data;
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách khách hàng'
        });
        this.loading = false;
      }
    });
  }

  loadWarehouses(): void {
    this.warehouseService.getAllWarehouses(true).subscribe({
      next: (data) => {
        this.warehouses = data;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách kho'
        });
      }
    });
  }

  loadMaterials(customerId?: string): void {
    // Always load all materials (materials are shared across customers)
    // Don't filter by customer - user can select any material for any customer
    this.materialService.getAll(true, undefined).subscribe({
      next: (data) => {
        this.materials = data;
        this.filteredMaterials = data;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách nguyên vật liệu'
        });
      }
    });
  }

  loadUnitsOfMeasure(): void {
    this.unitOfMeasureService.getAll().subscribe({
      next: (data) => {
        this.unitsOfMeasure = data.filter(u => u.isActive);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách đơn vị'
        });
      }
    });
  }

  onCustomerChange(event: any): void {
    // Reset PO khi thay đổi khách hàng
    this.selectedPO = null;
    this.stockInForm.patchValue({
      purchaseOrderId: null,
      poNumber: ''
    });
    this.filteredPOs = [];
    
    // Materials are shared across customers, no need to reload
  }

  onSearchPO(event: any): void {
    const searchTerm = event.query;
    const customerId = this.stockInForm.get('customerId')?.value;
    
    // Không tìm kiếm nếu chưa chọn khách hàng
    if (!customerId) {
      this.filteredPOs = [];
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn khách hàng trước'
      });
      return;
    }

    this.stockInService.getPOsForSelection(searchTerm, customerId).subscribe({
      next: (data) => {
        this.filteredPOs = data;
        if (data.length === 0) {
          this.messageService.add({
            severity: 'info',
            summary: 'Thông tin',
            detail: 'Không tìm thấy PO nào của khách hàng này'
          });
        }
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tìm kiếm PO'
        });
      }
    });
  }

  onSelectPO(event: any): void {
    // PrimeNG autocomplete có thể truyền event hoặc object trực tiếp
    const po = event?.value || event;
    
    // Kiểm tra nếu po không có giá trị hoặc không đầy đủ
    if (!po || !po.id) {
      console.warn('Invalid PO selection:', event);
      return;
    }

    this.selectedPO = po;
    this.stockInForm.patchValue({
      purchaseOrderId: po.id,
      poNumber: po.poNumber || ''
      // Giữ nguyên customerId đã chọn
    });
    
    // Auto-fill purchasePOCode cho tất cả các dòng hiện có
    this.materialsArray.controls.forEach(control => {
      control.patchValue({ purchasePOCode: po.poNumber || '' });
    });
    
    // Load materials by PO's customer if needed
    this.messageService.add({
      severity: 'info',
      summary: 'Đã chọn PO',
      detail: `PO: ${po.poNumber || 'N/A'} - ${po.customerName || 'N/A'}`
    });
  }

  onClearPO(): void {
    this.selectedPO = null;
    this.stockInForm.patchValue({
      purchaseOrderId: null,
      poNumber: ''
    });
    // Clear purchasePOCode cho tất cả các dòng
    this.materialsArray.controls.forEach(control => {
      control.patchValue({ purchasePOCode: '' });
    });
  }

  onMaterialChange(index: number): void {
    const materialControl = this.materialsArray.at(index).get('materialId');
    const unitControl = this.materialsArray.at(index).get('unit');
    
    if (materialControl && materialControl.value) {
      const material = this.materials.find(m => m.id === materialControl.value);
      if (material && unitControl) {
        unitControl.setValue(material.unit);
      }
    }
  }

  onSubmit(): void {
    if (this.stockInForm.invalid) {
      this.markFormGroupTouched(this.stockInForm);
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng điền đầy đủ thông tin bắt buộc'
      });
      return;
    }

    const formValue = this.stockInForm.value;
    console.log('Form value before submit:', formValue);
    console.log('purchaseOrderId:', formValue.purchaseOrderId);
    
    const request: StockInRequest = {
      purchaseOrderId: formValue.purchaseOrderId || null,
      customerId: formValue.customerId,
      warehouseId: formValue.warehouseId,
      receiptDate: formValue.receiptDate,
      receiptNumber: formValue.receiptNumber,
      notes: formValue.notes,
      materials: formValue.materials
    };

    console.log('Request to send:', request);
    
    this.submitting = true;
    this.stockInService.stockIn(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: response.message || 'Nhập kho thành công'
          });
          this.resetForm();
          this.stockInSuccess.emit(); // Emit event để parent component refresh data
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: response.errors?.join(', ') || 'Có lỗi xảy ra'
          });
        }
        this.submitting = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.error || 'Không thể nhập kho'
        });
        this.submitting = false;
      }
    });
  }

  resetForm(): void {
    // Preserve prefill data if exists
    const preservedData: any = {
      receiptDate: new Date()
    };
    
    if (this.prefillCustomerId) {
      preservedData.customerId = this.prefillCustomerId;
    }
    
    if (this.prefillPOId && this.prefillPONumber) {
      preservedData.purchaseOrderId = this.prefillPOId;
      preservedData.poNumber = this.prefillPONumber;
      // Don't clear selectedPO if we have prefill data
    } else {
      this.selectedPO = null;
    }
    
    this.stockInForm.reset(preservedData);
    
    // Clear materials array and add one row
    while (this.materialsArray.length > 0) {
      this.materialsArray.removeAt(0);
    }
    this.addMaterialRow();
  }

  private markFormGroupTouched(formGroup: FormGroup | FormArray): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup || control instanceof FormArray) {
        this.markFormGroupTouched(control);
      }
    });
  }
}
