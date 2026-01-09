import { Component, OnInit } from '@angular/core';
import { MaterialService } from '../../../services/material.service';
import { CustomerService } from '../../../services/customer.service';
import { UnitOfMeasureService } from '../../../services/unit-of-measure.service';
import { Material } from '../../../models/material.interface';
import { Customer } from '../../../models/customer.interface';
import { UnitOfMeasure } from '../../../models/unit-of-measure.interface';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

@Component({
  selector: 'app-materials',
  templateUrl: './materials.component.html',
  styleUrls: ['./materials.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class MaterialsComponent implements OnInit {
  materials: Material[] = [];
  customers: Customer[] = [];
  units: UnitOfMeasure[] = [];
  loading = false;
  showDialog = false;
  isEdit = false;
  selectedMaterial: Material | null = null;
  
  // Filter theo chủ hàng (optional)
  selectedCustomerId: string | null = null;

  materialForm: any = {
    code: '',
    name: '',
    type: '',
    colorCode: '',
    unit: 'kg',
    description: '',
    isActive: true,
    customerId: null // Optional - Material có thể không gắn với chủ hàng
  };

  materialTypes = [
    { label: 'Nhựa nguyên sinh', value: 'PLASTIC' },
    { label: 'Mực in', value: 'INK' },
    { label: 'Vật tư phụ', value: 'AUXILIARY' },
    { label: 'Khác', value: 'OTHER' }
  ];

  constructor(
    private materialService: MaterialService,
    private customerService: CustomerService,
    private unitOfMeasureService: UnitOfMeasureService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadUnits();
    this.loadMaterials();
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

  loadUnits(): void {
    this.unitOfMeasureService.getAll().subscribe({
      next: (units) => {
        this.units = units.filter(u => u.isActive);
      },
      error: (error) => {
        console.error('Error loading units:', error);
      }
    });
  }

  loadMaterials(): void {
    this.loading = true;
    this.materialService.getAll(undefined, this.selectedCustomerId || undefined).subscribe({
      next: (materials) => {
        this.materials = materials;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading materials:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách vật tư'
        });
      }
    });
  }

  onCustomerFilterChange(): void {
    this.loadMaterials();
  }

  openCreateDialog(): void {
    this.isEdit = false;
    this.materialForm = {
      code: '',
      name: '',
      type: 'PLASTIC',
      colorCode: '',
      unit: this.units.length > 0 ? this.units[0].code : 'kg',
      description: '',
      isActive: true,
      customerId: this.selectedCustomerId || null // Optional - có thể null
    };
    this.showDialog = true;
  }

  openEditDialog(material: Material): void {
    this.isEdit = true;
    this.selectedMaterial = material;
    this.materialForm = {
      code: material.code,
      name: material.name,
      type: material.type,
      colorCode: material.colorCode || '',
      unit: material.unit,
      description: material.description || '',
      isActive: material.isActive,
      customerId: material.customerId // Giữ nguyên customerId
    };
    this.showDialog = true;
  }

  saveMaterial(): void {
    if (!this.materialForm.code || !this.materialForm.name) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập đầy đủ thông tin bắt buộc (Mã NVL và Tên NVL)'
      });
      return;
    }

    if (this.isEdit && this.selectedMaterial) {
      this.materialService.update(this.selectedMaterial.id, this.materialForm).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Cập nhật vật tư thành công'
          });
          this.showDialog = false;
          this.loadMaterials();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: error.error?.message || 'Không thể cập nhật vật tư'
          });
        }
      });
    } else {
      this.materialService.create(this.materialForm).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Tạo vật tư mới thành công'
          });
          this.showDialog = false;
          this.loadMaterials();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: error.error?.message || 'Không thể tạo vật tư'
          });
        }
      });
    }
  }

  getStatusSeverity(isActive: boolean): string {
    return isActive ? 'success' : 'danger';
  }

  getStockSeverity(current: number, min: number): string {
    if (current <= 0) return 'danger';
    if (current <= min) return 'warning';
    return 'success';
  }
}

