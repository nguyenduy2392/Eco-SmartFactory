import { Component, OnInit } from '@angular/core';
import { MaterialService } from '../../../services/material.service';
import { Material } from '../../../models/material.interface';
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
  loading = false;
  showDialog = false;
  isEdit = false;
  selectedMaterial: Material | null = null;

  materialForm: any = {
    code: '',
    name: '',
    type: '',
    colorCode: '',
    supplier: '',
    unit: 'kg',
    currentStock: 0,
    minStock: 0,
    unitCost: 0,
    description: '',
    isActive: true
  };

  materialTypes = [
    { label: 'Nhựa nguyên sinh', value: 'PLASTIC' },
    { label: 'Mực in', value: 'INK' },
    { label: 'Vật tư phụ', value: 'AUXILIARY' },
    { label: 'Khác', value: 'OTHER' }
  ];

  units = [
    { label: 'kg', value: 'kg' },
    { label: 'lít', value: 'l' },
    { label: 'cái', value: 'pcs' },
    { label: 'bộ', value: 'set' }
  ];

  constructor(
    private materialService: MaterialService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadMaterials();
  }

  loadMaterials(): void {
    this.loading = true;
    this.materialService.getAll().subscribe({
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

  openCreateDialog(): void {
    this.isEdit = false;
    this.materialForm = {
      code: '',
      name: '',
      type: 'PLASTIC',
      colorCode: '',
      supplier: '',
      unit: 'kg',
      currentStock: 0,
      minStock: 0,
      unitCost: 0,
      description: '',
      isActive: true
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
      supplier: material.supplier || '',
      unit: material.unit,
      currentStock: material.currentStock,
      minStock: material.minStock,
      unitCost: material.unitCost || 0,
      description: material.description || '',
      isActive: material.isActive
    };
    this.showDialog = true;
  }

  saveMaterial(): void {
    if (!this.materialForm.code || !this.materialForm.name) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập đầy đủ thông tin bắt buộc'
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

