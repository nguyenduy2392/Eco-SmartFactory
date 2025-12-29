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
      next: (result: any) => {
        // Map BE response to frontend interface
        this.availabilityResult = {
          overallStatus: result.overallStatus,
          purchaseOrderId: result.purchaseOrderId,
          plannedQuantity: result.plannedQuantity,
          checkDate: result.checkedAt ? new Date(result.checkedAt) : new Date(),
          partResults: result.partDetails?.map((p: any) => ({
            partId: p.partId,
            partCode: p.partCode,
            partName: p.partName,
            processingType: p.processingType,
            processingTypeName: p.processingTypeName,
            requiredQty: p.requiredQuantity,
            canProduce: p.canProduce,
            severity: p.severity,
            bomVersion: p.bomVersion,
            hasActiveBOM: p.hasActiveBOM
          })) || []
        };
        this.checkLoading = false;
        
        // Show notification based on result
        if (result.overallStatus === 'PASS') {
          this.messageService.add({
            severity: 'success',
            summary: 'Kiểm tra thành công',
            detail: 'Tất cả linh kiện có thể sản xuất',
            life: 3000
          });
        } else if (result.overallStatus === 'FAIL') {
          this.messageService.add({
            severity: 'error',
            summary: 'Kiểm tra thất bại',
            detail: 'Có linh kiện không thể sản xuất. Vui lòng kiểm tra BOM.',
            life: 5000
          });
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: 'Cảnh báo',
            detail: 'Có linh kiện cần kiểm tra.',
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
          detail: error.error?.message || 'Không thể kiểm tra khả dụng linh kiện'
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
      'PASS': 'CÓ THỂ SẢN XUẤT',
      'FAIL': 'KHÔNG THỂ SẢN XUẤT',
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

  getPartSeverityLabel(severity: string): string {
    const severityMap: { [key: string]: string } = {
      'OK': 'Có thể sản xuất',
      'WARNING': 'Cảnh báo',
      'CRITICAL': 'Không có BOM'
    };
    return severityMap[severity] || severity;
  }

  getPartSeveritySeverity(severity: string): string {
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

  getStatusClass(status: string): string {
    return 'status-' + status.toLowerCase();
  }

  getStatusColor(status: string): string {
    return status === 'PASS' ? 'text-green-500' : 'text-red-500';
  }

  getRowClass(severity: string): { [key: string]: boolean } {
    return {
      'material-row-critical': severity === 'CRITICAL',
      'material-row-warning': severity === 'WARNING'
    };
  }

  getCanProduceIcon(canProduce: boolean): string {
    return canProduce ? 'pi pi-check-circle text-green-500' : 'pi pi-times-circle text-red-500';
  }

  getCanProduceCount(): number {
    return this.availabilityResult?.partResults.filter(p => p.canProduce).length || 0;
  }
}


