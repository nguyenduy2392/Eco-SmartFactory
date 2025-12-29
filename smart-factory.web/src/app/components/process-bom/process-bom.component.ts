import { Component, OnInit } from '@angular/core';
import { ProcessBOMService } from '../../services/process-bom.service';
import { PartService, PartDetail } from '../../services/part.service';
import {
  ProcessBOM,
  ProcessBOMList,
  CreateBOMRequest,
  CreateBOMDetailRequest,
  BOMDetail
} from '../../models/process-bom.interface';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SharedModule } from '../../shared.module';
import { PrimengModule } from '../../primeng.module';

interface BOMDetailForm {
  materialCode: string;
  materialName?: string;
  qtyPerUnit: number;
  scrapRate: number;
  uom: string;
  processStep?: string;
  notes?: string;
}

@Component({
  selector: 'app-process-bom',
  templateUrl: './process-bom.component.html',
  styleUrls: ['./process-bom.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class ProcessBOMComponent implements OnInit {
  bomList: ProcessBOMList[] = [];
  parts: PartDetail[] = [];
  loading = false;

  // Filters
  selectedPartId: string | undefined;
  selectedProcessingType: string | undefined;
  selectedStatus: string | undefined;

  // Selected BOM for viewing
  selectedBOM: ProcessBOM | null = null;
  showBOMDetailDialog = false;

  // Create BOM Dialog
  showCreateBOMDialog = false;
  createBOMLoading = false;
  createBOMForm: {
    partId: string;
    processingType: string;
    effectiveDate: Date;
    notes: string;
  } = {
    partId: '',
    processingType: 'EP_NHUA',
    effectiveDate: new Date(),
    notes: ''
  };
  bomDetailsForm: BOMDetailForm[] = [];

  // Options
  processingTypeOptions = [
    { label: 'ÉP NHỰA', value: 'EP_NHUA' },
    { label: 'PHUN IN', value: 'PHUN_IN' },
    { label: 'LẮP RÁP', value: 'LAP_RAP' }
  ];

  statusOptions = [
    { label: 'Tất cả', value: undefined },
    { label: 'Đang hoạt động', value: 'ACTIVE' },
    { label: 'Không hoạt động', value: 'INACTIVE' }
  ];

  uomOptions = [
    { label: 'KG', value: 'KG' },
    { label: 'PCS', value: 'PCS' },
    { label: 'M', value: 'M' },
    { label: 'M2', value: 'M2' },
    { label: 'L', value: 'L' }
  ];

  constructor(
    private bomService: ProcessBOMService,
    private partService: PartService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit(): void {
    this.loadParts();
    this.loadBOMList();
  }

  loadParts(): void {
    this.partService.getAll().subscribe({
      next: (parts) => {
        this.parts = parts;
      },
      error: (error) => {
        console.error('Error loading parts:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách linh kiện'
        });
      }
    });
  }

  loadBOMList(): void {
    this.loading = true;
    this.bomService.getAll(
      this.selectedPartId,
      this.selectedProcessingType,
      this.selectedStatus
    ).subscribe({
      next: (list) => {
        this.bomList = list;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading BOM list:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách BOM'
        });
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.loadBOMList();
  }

  resetFilters(): void {
    this.selectedPartId = undefined;
    this.selectedProcessingType = undefined;
    this.selectedStatus = undefined;
    this.loadBOMList();
  }

  // View BOM Detail
  viewBOMDetail(bom: ProcessBOMList): void {
    this.bomService.getById(bom.id).subscribe({
      next: (detail) => {
        this.selectedBOM = detail;
        this.showBOMDetailDialog = true;
      },
      error: (error) => {
        console.error('Error loading BOM detail:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải chi tiết BOM'
        });
      }
    });
  }

  closeBOMDetailDialog(): void {
    this.showBOMDetailDialog = false;
    this.selectedBOM = null;
  }

  // Create BOM Dialog
  openCreateBOMDialog(): void {
    this.showCreateBOMDialog = true;
    this.resetCreateBOMForm();
  }

  closeCreateBOMDialog(): void {
    this.showCreateBOMDialog = false;
    this.resetCreateBOMForm();
  }

  resetCreateBOMForm(): void {
    this.createBOMForm = {
      partId: '',
      processingType: 'EP_NHUA',
      effectiveDate: new Date(),
      notes: ''
    };
    this.bomDetailsForm = [];
    this.addBOMDetailRow();
  }

  addBOMDetailRow(): void {
    this.bomDetailsForm.push({
      materialCode: '',
      materialName: '',
      qtyPerUnit: 0,
      scrapRate: 0,
      uom: 'KG',
      processStep: '',
      notes: ''
    });
  }

  removeBOMDetailRow(index: number): void {
    if (this.bomDetailsForm.length > 1) {
      this.bomDetailsForm.splice(index, 1);
    } else {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'BOM phải có ít nhất một dòng nguyên vật liệu'
      });
    }
  }

  validateCreateBOMForm(): boolean {
    if (!this.createBOMForm.partId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn linh kiện'
      });
      return false;
    }

    if (!this.createBOMForm.processingType) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn loại gia công'
      });
      return false;
    }

    if (this.bomDetailsForm.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'BOM phải có ít nhất một dòng nguyên vật liệu'
      });
      return false;
    }

    for (let i = 0; i < this.bomDetailsForm.length; i++) {
      const detail = this.bomDetailsForm[i];
      
      if (!detail.materialCode.trim()) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: `Dòng ${i + 1}: Vui lòng nhập mã nguyên vật liệu`
        });
        return false;
      }

      if (detail.qtyPerUnit <= 0) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: `Dòng ${i + 1}: Số lượng phải lớn hơn 0`
        });
        return false;
      }

      if (detail.scrapRate < 0) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: `Dòng ${i + 1}: Tỷ lệ hao hụt phải >= 0`
        });
        return false;
      }

      if (!detail.uom.trim()) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: `Dòng ${i + 1}: Vui lòng chọn đơn vị tính`
        });
        return false;
      }
    }

    return true;
  }

  createBOM(): void {
    if (!this.validateCreateBOMForm()) {
      return;
    }

    this.createBOMLoading = true;

    const request: CreateBOMRequest = {
      partId: this.createBOMForm.partId,
      processingType: this.createBOMForm.processingType as 'EP_NHUA' | 'PHUN_IN' | 'LAP_RAP',
      effectiveDate: this.createBOMForm.effectiveDate,
      notes: this.createBOMForm.notes,
      bomDetails: this.bomDetailsForm.map(detail => ({
        materialCode: detail.materialCode.trim(),
        qtyPerUnit: detail.qtyPerUnit,
        scrapRate: detail.scrapRate,
        uom: detail.uom,
        processStep: detail.processStep?.trim() || undefined,
        notes: detail.notes?.trim() || undefined
      }))
    };

    this.bomService.create(request).subscribe({
      next: (bom) => {
        this.createBOMLoading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: `Tạo BOM thành công: ${bom.version}`,
          life: 5000
        });
        this.closeCreateBOMDialog();
        this.loadBOMList();
      },
      error: (error) => {
        console.error('Create BOM error:', error);
        this.createBOMLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể tạo BOM'
        });
      }
    });
  }

  // Delete BOM
  deleteBOM(bom: ProcessBOMList): void {
    if (bom.status === 'ACTIVE') {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Không thể xóa BOM đang hoạt động'
      });
      return;
    }

    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa BOM ${bom.version} của linh kiện ${bom.partCode}?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      accept: () => {
        this.bomService.delete(bom.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Xóa BOM thành công'
            });
            this.loadBOMList();
          },
          error: (error) => {
            console.error('Delete BOM error:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi',
              detail: error.error?.message || 'Không thể xóa BOM'
            });
          }
        });
      }
    });
  }

  // Helpers
  getStatusLabel(status: string): string {
    return status === 'ACTIVE' ? 'Đang hoạt động' : 'Không hoạt động';
  }

  getStatusSeverity(status: string): string {
    return status === 'ACTIVE' ? 'success' : 'secondary';
  }

  getProcessingTypeLabel(type: string): string {
    const typeMap: { [key: string]: string } = {
      'EP_NHUA': 'ÉP NHỰA',
      'PHUN_IN': 'PHUN IN',
      'LAP_RAP': 'LẮP RÁP'
    };
    return typeMap[type] || type;
  }

  get bomDetails(): BOMDetail[] {
    return this.selectedBOM?.bomDetails || [];
  }
}

