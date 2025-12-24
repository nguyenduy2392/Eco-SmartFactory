import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { PurchaseOrderService } from '../../../services/purchase-order.service';
import { PurchaseOrder, POProduct, POOperation } from '../../../models/purchase-order.interface';
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

  // Tab state
  activeTab = 0;

  // Dialog states
  showProductDialog = false;
  showOperationDialog = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private poService: PurchaseOrderService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.poId = params['id'];
      if (this.poId) {
        this.loadPurchaseOrder();
      }
    });
  }

  /**
   * Load chi tiết PO
   */
  loadPurchaseOrder(): void {
    if (!this.poId) return;

    this.loading = true;
    this.poService.getById(this.poId).subscribe({
      next: (po) => {
        this.purchaseOrder = po;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading PO:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải thông tin PO'
        });
      }
    });
  }

  /**
   * Quay lại danh sách
   */
  goBack(): void {
    this.location.back();
  }

  /**
   * Clone version PO
   */
  cloneVersion(): void {
    if (!this.purchaseOrder) return;

    const newVersionType = this.purchaseOrder.versionType === 'ORIGINAL' ? 'FINAL' : 'PRODUCTION';
    
    this.confirmationService.confirm({
      message: `Bạn có chắc muốn tạo bản ${newVersionType} từ ${this.purchaseOrder.poNumber}?`,
      header: 'Xác nhận',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.poService.cloneVersion({
          originalPOId: this.purchaseOrder!.id,
          newVersionType: newVersionType,
          notes: `Cloned from ${this.purchaseOrder!.poNumber}`
        }).subscribe({
          next: (newPO) => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: `Đã tạo bản ${newVersionType}`
            });
            this.router.navigate(['/purchase-orders', newPO.id]);
          },
          error: (error) => {
            console.error('Error cloning PO:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi',
              detail: 'Không thể clone PO'
            });
          }
        });
      }
    });
  }

  /**
   * Export PO to Excel
   */
  exportToExcel(): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Thông báo',
      detail: 'Chức năng xuất Excel đang được phát triển'
    });
  }

  /**
   * Tính tổng tiền từ operations
   */
  getTotalOperationsAmount(): number {
    if (!this.purchaseOrder?.operations) return 0;
    return this.purchaseOrder.operations.reduce((sum, op) => sum + op.totalAmount, 0);
  }

  /**
   * Get badge severity cho status
   */
  getStatusSeverity(status: string): string {
    switch (status) {
      case 'New': return 'info';
      case 'InProgress': return 'warning';
      case 'Completed': return 'success';
      case 'Cancelled': return 'danger';
      default: return 'secondary';
    }
  }

  /**
   * Get badge severity cho version type
   */
  getVersionTypeSeverity(versionType: string): string {
    switch (versionType) {
      case 'ORIGINAL': return 'info';
      case 'FINAL': return 'warning';
      case 'PRODUCTION': return 'success';
      default: return 'secondary';
    }
  }

  /**
   * Get version type display text
   */
  getVersionTypeText(versionType: string): string {
    switch (versionType) {
      case 'ORIGINAL': return 'Bản gốc (PM Import)';
      case 'FINAL': return 'Bản chốt (PM Final)';
      case 'PRODUCTION': return 'Bản sản xuất (PMC)';
      default: return versionType;
    }
  }

  /**
   * Group operations by processing type
   */
  getOperationsByProcessingType(): { [key: string]: POOperation[] } {
    if (!this.purchaseOrder?.operations) return {};
    
    const grouped: { [key: string]: POOperation[] } = {};
    this.purchaseOrder.operations.forEach(op => {
      if (!grouped[op.processingTypeName]) {
        grouped[op.processingTypeName] = [];
      }
      grouped[op.processingTypeName].push(op);
    });
    
    return grouped;
  }

  /**
   * Get processing type names
   */
  getProcessingTypeNames(): string[] {
    return Object.keys(this.getOperationsByProcessingType());
  }
}

