import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ProductService } from '../../../services/system/product.service';
import { ProductWithPrice } from '../../../models/interface/product.interface';
import { MessageService } from 'primeng/api';
import { Table } from 'primeng/table';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class ProductsComponent implements OnInit {
  @ViewChild('dt') dt!: Table;
  
  products: ProductWithPrice[] = [];
  loading = false;
  searchText = '';

  constructor(
    private productService: ProductService,
    private router: Router,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadProducts();
  }

  /**
   * Load danh sách sản phẩm với giá
   */
  loadProducts(): void {
    this.loading = true;
    this.productService.getAllWithPrices().subscribe({
      next: (products) => {
        console.log('Products loaded:', products);
        this.products = products || [];
        this.loading = false;
        if (this.products.length === 0) {
          console.log('No products found in database');
        }
      },
      error: (error) => {
        console.error('Error loading products:', error);
        console.error('Error details:', error.error);
        this.loading = false;
        this.products = [];
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.error || error.message || 'Không thể tải danh sách sản phẩm. Vui lòng kiểm tra kết nối API.'
        });
      }
    });
  }

  /**
   * Xem chi tiết sản phẩm
   */
  viewProduct(id: string): void {
    this.router.navigate(['/products', id]);
  }

  /**
   * Tạo sản phẩm mới
   */
  createProduct(): void {
    this.router.navigate(['/products/new']);
  }

  /**
   * Format currency
   */
  formatCurrency(value: number | null | undefined): string {
    if (value == null) return '-';
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  }

  /**
   * Apply global filter
   */
  applyGlobalFilter(): void {
    if (this.dt) {
      this.dt.filterGlobal(this.searchText, 'contains');
    }
  }
}
