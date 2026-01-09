import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { StockInService } from '../../../services/stock-in.service';
import { MaterialService } from '../../../services/material.service';
import { WarehouseService } from '../../../services/warehouse.service';
import { CustomerService } from '../../../services/customer.service';
import {
  StockInRequest,
  StockInMaterialItem,
  POForSelection
} from '../../../models/stock-in.interface';
import { Material } from '../../../models/material.interface';
import { Warehouse } from '../../../models/warehouse.interface';
import { Customer } from '../../../models/customer.interface';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

@Component({
  selector: 'app-stock-in',
  templateUrl: './stock-in.component.html',
  styleUrls: ['./stock-in.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class StockInComponent implements OnInit {
  @Output() stockInSuccess = new EventEmitter<void>();
  
  stockInForm!: FormGroup;
  loading = false;
  submitting = false;

  // Dropdowns data
  customers: Customer[] = [];
  warehouses: Warehouse[] = [];
  materials: Material[] = [];
  filteredPOs: POForSelection[] = [];

  // Selected data
  selectedPO: POForSelection | null = null;

  constructor(
    private fb: FormBuilder,
    private stockInService: StockInService,
    private materialService: MaterialService,
    private warehouseService: WarehouseService,
    private customerService: CustomerService,
    private messageService: MessageService
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadWarehouses();
    this.loadMaterials();
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
      batchNumber: ['', Validators.required],
      supplierCode: [''],
      purchasePOCode: [''],
      notes: ['']
    });
  }

  addMaterialRow(): void {
    this.materialsArray.push(this.createMaterialRow());
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

  loadMaterials(): void {
    this.materialService.getAll().subscribe({
      next: (data) => {
        this.materials = data;
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

  onCustomerChange(event: any): void {
    // Reset PO khi thay đổi khách hàng
    this.selectedPO = null;
    this.stockInForm.patchValue({
      purchaseOrderId: null,
      poNumber: ''
    });
    this.filteredPOs = [];
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
    const request: StockInRequest = {
      purchaseOrderId: formValue.purchaseOrderId || null,
      customerId: formValue.customerId,
      warehouseId: formValue.warehouseId,
      receiptDate: formValue.receiptDate,
      receiptNumber: formValue.receiptNumber,
      notes: formValue.notes,
      materials: formValue.materials
    };

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
    this.stockInForm.reset({
      receiptDate: new Date()
    });
    this.selectedPO = null;
    
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
