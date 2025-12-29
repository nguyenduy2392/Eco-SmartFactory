import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { PurchaseOrderService } from '../../../services/purchase-order.service';
import { AvailabilityCheckService } from '../../../services/availability-check.service';
import { MaterialReceiptService } from '../../../services/material-receipt.service';
import {
  PurchaseOrder,
  POOperation,
  POMaterialBaseline,
  AvailabilityCheckRequest,
  AvailabilityCheckResult
} from '../../../models/purchase-order.interface';
import { MaterialReceipt } from '../../../models/material-receipt.interface';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

@Component({
  selector: 'app-po-detail',
  templateUrl: './po-detail.component.html',
  styleUrls: ['./po-detail.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class PODetailComponent implements OnInit {
  purchaseOrder: PurchaseOrder | null = null;
  loading = false;
  poId: string | null = null;

  // Version management
  availableVersions: PurchaseOrder[] = [];
  selectedVersion: PurchaseOrder | null = null;

  // Tab state
  activeTabIndex = 0;

  // Availability Check Dialog
  showAvailabilityDialog = false;
  availabilityLoading = false;
  plannedQuantity: number = 0;
  availabilityResult: AvailabilityCheckResult | null = null;

  // Material Receipts
  materialReceipts: MaterialReceipt[] = [];
  materialReceiptsLoading = false;

  // Clone Version Dialog
  showCloneDialog = false;
  cloneLoading = false;
  cloneNotes: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private poService: PurchaseOrderService,
    private availabilityService: AvailabilityCheckService,
    private materialReceiptService: MaterialReceiptService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.poId = params['id'];
      if (this.poId) {
        this.loadPODetail();
        this.loadPOVersions();
      }
    });
  }

  loadPODetail(): void {
    if (!this.poId) return;

    this.loading = true;
    this.poService.getById(this.poId).subscribe({
      next: (po) => {
        this.purchaseOrder = po;
        this.selectedVersion = po;
        this.loading = false;
        // Load material receipts for this customer
        if (po.customerId) {
          this.loadMaterialReceipts(po.customerId);
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

  loadMaterialReceipts(customerId: string): void {
    this.materialReceiptsLoading = true;
    this.materialReceiptService.getByCustomer(customerId, 'RECEIVED').subscribe({
      next: (receipts) => {
        this.materialReceipts = receipts;
        this.materialReceiptsLoading = false;
      },
      error: (error) => {
        console.error('Error loading material receipts:', error);
        this.materialReceiptsLoading = false;
      }
    });
  }

  loadPOVersions(): void {
    if (!this.purchaseOrder) return;

    this.poService.getVersions(this.purchaseOrder.poNumber).subscribe({
      next: (versions) => {
        this.availableVersions = versions;
      },
      error: (error) => {
        console.error('Error loading versions:', error);
      }
    });
  }

  onVersionChange(version: PurchaseOrder): void {
    this.selectedVersion = version;
    this.purchaseOrder = version;
  }

  goBack(): void {
    this.location.back();
  }

  // Status and type helpers
  getStatusLabel(status: string): string {
    const statusMap: { [key: string]: string } = {
      'DRAFT': 'Nháp',
      'APPROVED_FOR_PMC': 'Đã phê duyệt cho PMC',
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

  get isLocked(): boolean {
    return this.purchaseOrder?.status === 'LOCKED';
  }

  get isApproved(): boolean {
    return this.purchaseOrder?.status === 'APPROVED_FOR_PMC';
  }

  get isDraft(): boolean {
    return this.purchaseOrder?.status === 'DRAFT';
  }

  get canApprove(): boolean {
    return this.isDraft && !this.isLocked;
  }

  get canClone(): boolean {
    return this.purchaseOrder !== null;
  }

  // Clone Version
  openCloneDialog(): void {
    this.showCloneDialog = true;
    this.cloneNotes = '';
  }

  closeCloneDialog(): void {
    this.showCloneDialog = false;
    this.cloneNotes = '';
  }

  cloneVersion(): void {
    if (!this.purchaseOrder) return;

    this.cloneLoading = true;
    this.poService.cloneVersion({
      originalPOId: this.purchaseOrder.id,
      notes: this.cloneNotes
    }).subscribe({
      next: (newVersion) => {
        this.cloneLoading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: `Tạo version mới thành công: ${newVersion.version}`,
          life: 5000
        });
        this.closeCloneDialog();
        this.loadPOVersions();
        // Navigate to new version
        this.router.navigate(['/processing-po', newVersion.id]);
      },
      error: (error) => {
        console.error('Clone error:', error);
        this.cloneLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể tạo version mới'
        });
      }
    });
  }

  // Approve for PMC
  approveForPMC(): void {
    if (!this.purchaseOrder) return;

    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn phê duyệt PO ${this.purchaseOrder.poNumber} (${this.purchaseOrder.version}) cho PMC?<br><br>
                <strong>Lưu ý:</strong> Sau khi phê duyệt, version này sẽ bị khóa và không thể chỉnh sửa.`,
      header: 'Xác nhận phê duyệt',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Phê duyệt',
      rejectLabel: 'Hủy',
      accept: () => {
        this.poService.approveForPMC({
          purchaseOrderId: this.purchaseOrder!.id
        }).subscribe({
          next: (response) => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: response.message || 'Phê duyệt PO thành công'
            });
            this.loadPODetail();
            this.loadPOVersions();
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

  // Availability Check
  openAvailabilityDialog(): void {
    if (!this.isApproved) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Chỉ có thể kiểm tra khả dụng với PO đã phê duyệt'
      });
      return;
    }
    this.showAvailabilityDialog = true;
    this.plannedQuantity = 0;
    this.availabilityResult = null;
  }

  closeAvailabilityDialog(): void {
    this.showAvailabilityDialog = false;
    this.plannedQuantity = 0;
    this.availabilityResult = null;
  }

  checkAvailability(): void {
    if (!this.purchaseOrder) return;

    if (!this.plannedQuantity || this.plannedQuantity <= 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập số lượng kế hoạch sản xuất'
      });
      return;
    }

    this.availabilityLoading = true;
    const request: AvailabilityCheckRequest = {
      purchaseOrderId: this.purchaseOrder.id,
      plannedQuantity: this.plannedQuantity
    };

    this.availabilityService.checkAvailability(request).subscribe({
      next: (result) => {
        this.availabilityResult = result;
        this.availabilityLoading = false;
      },
      error: (error) => {
        console.error('Availability check error:', error);
        this.availabilityLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể kiểm tra khả dụng NVL'
        });
      }
    });
  }

  getOverallStatusLabel(status: string): string {
    const statusMap: { [key: string]: string } = {
      'PASS': 'ĐỦ NGUYÊN VẬT LIỆU',
      'FAIL': 'THIẾU NGUYÊN VẬT LIỆU',
      'WARNING': 'CẢN BÁO'
    };
    return statusMap[status] || status;
  }

  getOverallStatusSeverity(status: string): string {
    const severityMap: { [key: string]: string } = {
      'PASS': 'success',
      'FAIL': 'danger',
      'WARNING': 'warning'
    };
    return severityMap[status] || 'info';
  }

  getMaterialSeverityLabel(severity: string): string {
    const severityMap: { [key: string]: string } = {
      'OK': 'OK',
      'WARNING': 'Cảnh báo',
      'CRITICAL': 'Thiếu'
    };
    return severityMap[severity] || severity;
  }

  getMaterialSeveritySeverity(severity: string): string {
    const severityMap: { [key: string]: string } = {
      'OK': 'success',
      'WARNING': 'warning',
      'CRITICAL': 'danger'
    };
    return severityMap[severity] || 'info';
  }

  get operations(): POOperation[] {
    return this.purchaseOrder?.operations || [];
  }

  get materialBaseline(): POMaterialBaseline[] {
    return this.purchaseOrder?.materialBaseline || [];
  }

  // Format date for display
  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString('vi-VN');
  }
}


