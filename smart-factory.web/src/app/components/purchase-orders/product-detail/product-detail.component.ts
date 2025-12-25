import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

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
    private location: Location
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
    // TODO: Replace with actual API call
    this.loading = true;
    
    // Mock data for demonstration
    setTimeout(() => {
      this.product = {
        id: this.productId!,
        poId: this.poId!,
        poNumber: 'PO-2023-089',
        productName: 'Executive Office Chair - Ergonomic Series',
        sku: 'SKU-CHAIR-0089-BLK',
        quantity: 500,
        material: 'Mesh Fabric',
        color: 'Midnight Black',
        colorHex: '#000000',
        description: 'Standard ergonomic model with lumbar support and adjustable armrests.',
        status: 'In Production',
        components: [
          {
            id: '1',
            componentName: 'Wheel Base Assembly',
            partId: 'CPT-WHL-001',
            material: 'Reinforced Nylon',
            quantityRequired: '1 pc',
            status: 'Ready',
            description: 'Main structural support'
          },
          {
            id: '2',
            componentName: 'Gas Lift Mechanism',
            partId: 'CPT-GAS-024',
            material: 'Steel',
            quantityRequired: '1 pc',
            status: 'Processing',
            description: 'Height adjustment unit'
          },
          {
            id: '3',
            componentName: 'Armrest Set (L/R)',
            partId: 'CPT-ARM-112',
            material: 'PU + Plastic',
            quantityRequired: '1 set',
            status: 'Pending',
            description: 'Adjustable padded arms'
          }
        ]
      };
      this.loading = false;
    }, 500);
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

