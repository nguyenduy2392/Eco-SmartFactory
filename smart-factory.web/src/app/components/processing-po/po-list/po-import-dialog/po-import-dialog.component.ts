import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PurchaseOrderService } from '../../../../services/purchase-order.service';
import { Customer } from '../../../../models/customer.interface';
import { ImportPOResponse, ImportError } from '../../../../models/purchase-order.interface';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../../shared.module';
import { PrimengModule } from '../../../../primeng.module';
import { UiModalService } from '../../../../services/shared/ui-modal.service';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';

@Component({
  selector: 'app-po-import-dialog',
  templateUrl: './po-import-dialog.component.html',
  styleUrls: ['./po-import-dialog.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, SharedModule, PrimengModule, CKEditorModule]
})
export class POImportDialogComponent implements OnInit, OnChanges {
  @Input() customers: Customer[] = [];
  @Input() importProcessingTypeOptions: { label: string; value: string }[] = [];
  @Input() customerId?: string;
  @Input() onImport?: () => void;

  importLoading = false;
  selectedFile: File | null = null;
  importErrors: ImportError[] = [];
  showImportErrorDialog = false;
  public Editor = ClassicEditor;
  importForm = {
    poNumber: '',
    customerId: '',
    processingType: 'EP_NHUA',
    poDate: new Date(),
    expectedDeliveryDate: null as Date | null,
    notes: ''
  };

  constructor(
    private poService: PurchaseOrderService,
    private messageService: MessageService,
    private uiModalService: UiModalService
  ) { }

  ngOnInit(): void {
    this.resetImportForm();
    // Nếu có customerId từ input, tự động set vào form
    if (this.customerId) {
      this.importForm.customerId = this.customerId;
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['customerId'] && this.customerId) {
      this.importForm.customerId = this.customerId;
    }

    if (changes['importProcessingTypeOptions'] && this.importProcessingTypeOptions.length > 0) {
      if (!this.importForm.processingType) {
        this.importForm.processingType = this.importProcessingTypeOptions[0].value;
      }
    }
  }

  handleClose(): void {
    this.uiModalService.closeModal();
    this.resetImportForm();
  }

  resetImportForm(): void {
    const defaultProcessingType = this.importProcessingTypeOptions.length > 0 
      ? this.importProcessingTypeOptions[0].value 
      : 'EP_NHUA';
    // Ưu tiên sử dụng customerId từ input, nếu không thì giữ giá trị hiện tại
    const customerId = this.customerId || this.importForm.customerId || '';
    this.importForm = {
      poNumber: '',
      customerId: customerId,
      processingType: defaultProcessingType,
      poDate: new Date(),
      expectedDeliveryDate: null,
      notes: ''
    };
    this.selectedFile = null;
    this.importErrors = [];
  }

  onFileSelect(event: any): void {
    const file = event.files[0];
    if (file) {
      // Validate file type
      if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.xls')) {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Vui lòng chọn file Excel (.xlsx hoặc .xls)'
        });
        return;
      }
      this.selectedFile = file;
    }
  }

  onFileRemove(): void {
    this.selectedFile = null;
  }

  importPO(): void {
    if (!this.validateImportForm()) {
      return;
    }

    this.importLoading = true;
    this.poService.importFromExcel(
      this.selectedFile!,
      this.importForm.poNumber,
      this.importForm.customerId,
      this.importForm.processingType as 'EP_NHUA' | 'PHUN_IN' | 'LAP_RAP',
      this.importForm.poDate,
      this.importForm.expectedDeliveryDate,
      this.importForm.notes
    ).subscribe({
      next: (response: ImportPOResponse) => {
        this.importLoading = false;
        
        if (response.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: `Import PO thành công!\nPO ID: ${response.purchaseOrderId}\nVersion: ${response.version}\nStatus: ${response.status}`,
            life: 5000
          });
          this.uiModalService.closeModal();
          this.resetImportForm();
          if (this.onImport) {
            this.onImport();
          }
        } else {
          // Có lỗi validation
          this.importErrors = response.errors || [];
          this.showImportErrorDialog = true;
        }
      },
      error: (error) => {
        console.error('Import error:', error);
        this.importLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể import PO'
        });
      }
    });
  }

  validateImportForm(): boolean {
    if (!this.selectedFile) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn file Excel'
      });
      return false;
    }

    if (!this.importForm.poNumber.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập số PO'
      });
      return false;
    }

    if (!this.importForm.customerId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn chủ hàng'
      });
      return false;
    }

    if (!this.importForm.processingType) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn loại gia công'
      });
      return false;
    }

    return true;
  }

  closeImportErrorDialog(): void {
    this.showImportErrorDialog = false;
    this.importErrors = [];
  }
}
