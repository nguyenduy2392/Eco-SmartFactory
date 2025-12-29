import { Component, OnInit } from '@angular/core';
import { PurchaseOrderService } from '../../services/purchase-order.service';
import { AvailabilityCheckService } from '../../services/availability-check.service';
import {
  PurchaseOrderList,
  AvailabilityCheckRequest,
  AvailabilityCheckResult
} from '../../models/purchase-order.interface';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../shared.module';
import { PrimengModule } from '../../primeng.module';

@Component({
  selector: 'app-availability-check',
  templateUrl: './availability-check.component.html',
  styleUrls: ['./availability-check.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class AvailabilityCheckComponent implements OnInit {
  purchaseOrders: PurchaseOrderList[] = [];
  loading = false;

  // Check form
  selectedPOId: string = '';
  plannedQuantity: number = 0;
  
  // Check result
  checkLoading = false;
  availabilityResult: AvailabilityCheckResult | null = null;

  constructor(
    private poService: PurchaseOrderService,
    private availabilityService: AvailabilityCheckService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadApprovedPOs();
  }

  loadApprovedPOs(): void {
    this.loading = true;
    // Load only APPROVED_FOR_PMC POs
    this.poService.getAll('APPROVED_FOR_PMC').subscribe({
      next: (orders) => {
        this.purchaseOrders = orders;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading POs:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách PO đã phê duyệt'
        });
        this.loading = false;
      }
    });
  }

  checkAvailability(): void {
    if (!this.validateForm()) {
      return;
    }

    this.checkLoading = true;
    const request: AvailabilityCheckRequest = {
      purchaseOrderId: this.selectedPOId,
      plannedQuantity: this.plannedQuantity
    };

    this.availabilityService.checkAvailability(request).subscribe({
      next: (result) => {
        this.availabilityResult = result;
        this.checkLoading = false;
        
        // Show notification based on result
        if (result.overallStatus === 'PASS') {
          this.messageService.add({
            severity: 'success',
            summary: 'Kiểm tra thành công',
            detail: 'Nguyên vật liệu đủ để sản xuất',
            life: 3000
          });
        } else if (result.overallStatus === 'FAIL') {
          this.messageService.add({
            severity: 'error',
            summary: 'Kiểm tra thất bại',
            detail: 'Thiếu nguyên vật liệu. Không thể tiến hành sản xuất.',
            life: 5000
          });
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: 'Cảnh báo',
            detail: 'Nguyên vật liệu gần hết. Nên bổ sung.',
            life: 4000
          });
        }
      },
      error: (error) => {
        console.error('Availability check error:', error);
        this.checkLoading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể kiểm tra khả dụng NVL'
        });
      }
    });
  }

  validateForm(): boolean {
    if (!this.selectedPOId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn PO'
      });
      return false;
    }

    if (!this.plannedQuantity || this.plannedQuantity <= 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập số lượng kế hoạch sản xuất'
      });
      return false;
    }

    return true;
  }

  resetForm(): void {
    this.selectedPOId = '';
    this.plannedQuantity = 0;
    this.availabilityResult = null;
  }

  // Helpers
  getOverallStatusLabel(status: string): string {
    const statusMap: { [key: string]: string } = {
      'PASS': 'ĐỦ NGUYÊN VẬT LIỆU',
      'FAIL': 'THIẾU NGUYÊN VẬT LIỆU',
      'WARNING': 'CẢNH BÁO'
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

  getOverallStatusIcon(status: string): string {
    const iconMap: { [key: string]: string } = {
      'PASS': 'pi pi-check-circle',
      'FAIL': 'pi pi-times-circle',
      'WARNING': 'pi pi-exclamation-triangle'
    };
    return iconMap[status] || 'pi pi-info-circle';
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

  get canCreateProductionPlan(): boolean {
    return this.availabilityResult?.overallStatus === 'PASS';
  }
}


