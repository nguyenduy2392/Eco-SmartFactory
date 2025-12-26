import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';
import { ProductService } from '../../../services/system/product.service';
import { MessageService } from 'primeng/api';

export interface ProductDetail {
  id: string;
  poId: string;
  poNumber: string;
  productName: string;
  sku: string;
  quantity: number;
  material: string;
  color: string;
  colorHex: string;
  description: string;
  imageUrl?: string;
  status: string;
  components: ComponentItem[];
}

export interface ComponentItem {
  id: string;
  componentName: string;
  partId: string;
  material: string;
  quantityRequired: string;
  status: 'Ready' | 'Processing' | 'Pending';
  imageUrl?: string;
  description?: string;
}

@Component({
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class ProductDetailComponent implements OnInit {
  poId: string | null = null;
  productId: string | null = null;
  product: ProductDetail | null = null;
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private productService: ProductService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.poId = params['poId'];
      this.productId = params['productId'];
      if (this.poId && this.productId) {
        this.loadProductDetail();
      }
    });
  }

  loadProductDetail(): void {
    if (!this.productId || !this.poId) {
      return;
    }

    this.loading = true;
    
    this.productService.getDetailByPO(this.productId, this.poId).subscribe({
      next: (apiProduct: any) => {
        // Map API response to component interface
        this.product = {
          id: apiProduct.id,
          poId: this.poId!,
          poNumber: apiProduct.poNumber,
          productName: apiProduct.productName,
          sku: apiProduct.sku,
          quantity: apiProduct.quantity,
          material: apiProduct.material || '',
          color: apiProduct.color || '',
          colorHex: apiProduct.colorHex || '#000000',
          description: apiProduct.description || '',
          imageUrl: apiProduct.imageUrl,
          status: apiProduct.status || 'Draft',
          components: apiProduct.components?.map((comp: any) => ({
            id: comp.id,
            componentName: comp.componentName,
            partId: comp.partId,
            material: comp.material || '',
            quantityRequired: comp.quantityRequired || '0 pcs',
            status: comp.status === 'Ready' ? 'Ready' : 
                    comp.status === 'Processing' ? 'Processing' : 'Pending',
            description: comp.description
          })) || []
        };
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading product detail:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải thông tin chi tiết sản phẩm'
        });
        // Fallback to empty product
        this.product = {
          id: this.productId!,
          poId: this.poId!,
          poNumber: '',
          productName: '',
          sku: '',
          quantity: 0,
          material: '',
          color: '',
          colorHex: '#000000',
          description: '',
          status: 'Draft',
          components: []
        };
      }
    });
  }

  goBack(): void {
    this.location.back();
  }

  goToPO(): void {
    if (this.poId) {
      this.router.navigate(['/purchase-orders', this.poId]);
    }
  }

  viewComponentDetail(componentId: string): void {
    if (this.poId && this.productId) {
      this.router.navigate(['/purchase-orders', this.poId, 'products', this.productId, 'components', componentId]);
    }
  }

  getStatusSeverity(status: string): string {
    switch (status) {
      case 'Ready':
        return 'success';
      case 'Processing':
        return 'warning';
      case 'Pending':
        return 'secondary';
      default:
        return 'info';
    }
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'Ready':
        return 'Sẵn sàng';
      case 'Processing':
        return 'Đang xử lý';
      case 'Pending':
        return 'Chờ xử lý';
      default:
        return status;
    }
  }

  saveChanges(): void {
    // TODO: Implement save functionality
    console.log('Save changes', this.product);
  }

  printLabel(): void {
    // TODO: Implement print functionality
    console.log('Print label');
  }

  updateImage(): void {
    // TODO: Implement image upload
    console.log('Update image');
  }

  addNewComponent(): void {
    // TODO: Implement add component dialog
    console.log('Add new component');
  }
}

