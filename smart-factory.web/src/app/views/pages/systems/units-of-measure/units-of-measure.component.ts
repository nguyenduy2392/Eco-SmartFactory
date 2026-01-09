import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../../shared.module';
import { PrimengModule } from '../../../../primeng.module';
import { UnitOfMeasureService } from '../../../../services/unit-of-measure.service';
import { UnitOfMeasure, CreateUnitOfMeasureRequest } from '../../../../models/unit-of-measure.interface';

@Component({
  selector: 'app-units-of-measure',
  standalone: true,
  imports: [SharedModule, PrimengModule],
  templateUrl: './units-of-measure.component.html',
  styleUrls: ['./units-of-measure.component.scss']
})
export class UnitsOfMeasureComponent implements OnInit {
  units: UnitOfMeasure[] = [];
  loading = false;
  showDialog = false;
  isEdit = false;
  selectedUnit: UnitOfMeasure | null = null;

  unitForm: CreateUnitOfMeasureRequest = {
    code: '',
    name: '',
    description: '',
    displayOrder: 0,
    isActive: true
  };

  constructor(
    private unitService: UnitOfMeasureService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadUnits();
  }

  loadUnits(): void {
    this.loading = true;
    this.unitService.getAll().subscribe({
      next: (units) => {
        this.units = units;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading units:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách đơn vị tính'
        });
      }
    });
  }

  openCreateDialog(): void {
    this.isEdit = false;
    this.unitForm = {
      code: '',
      name: '',
      description: '',
      displayOrder: 0,
      isActive: true
    };
    this.showDialog = true;
  }

  openEditDialog(unit: UnitOfMeasure): void {
    this.isEdit = true;
    this.selectedUnit = unit;
    this.unitForm = {
      code: unit.code,
      name: unit.name,
      description: unit.description,
      displayOrder: unit.displayOrder,
      isActive: unit.isActive
    };
    this.showDialog = true;
  }

  saveUnit(): void {
    if (!this.unitForm.code || !this.unitForm.name) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng điền đầy đủ thông tin bắt buộc'
      });
      return;
    }

    if (this.isEdit && this.selectedUnit) {
      this.unitService.update(this.selectedUnit.id, this.unitForm).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Cập nhật đơn vị tính thành công'
          });
          this.showDialog = false;
          this.loadUnits();
        },
        error: (error) => {
          console.error('Error updating unit:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: error.error?.error || 'Không thể cập nhật đơn vị tính'
          });
        }
      });
    } else {
      this.unitService.create(this.unitForm).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Thêm đơn vị tính thành công'
          });
          this.showDialog = false;
          this.loadUnits();
        },
        error: (error) => {
          console.error('Error creating unit:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: error.error?.error || 'Không thể thêm đơn vị tính'
          });
        }
      });
    }
  }

  deleteUnit(unit: UnitOfMeasure): void {
    if (confirm(`Bạn có chắc chắn muốn xóa đơn vị "${unit.name}"?`)) {
      this.unitService.delete(unit.id).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Xóa đơn vị tính thành công'
          });
          this.loadUnits();
        },
        error: (error) => {
          console.error('Error deleting unit:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: 'Không thể xóa đơn vị tính'
          });
        }
      });
    }
  }

  getStatusSeverity(isActive: boolean): string {
    return isActive ? 'success' : 'danger';
  }
}
