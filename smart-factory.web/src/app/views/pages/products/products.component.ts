import { Component, OnInit } from '@angular/core';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

@Component({
  selector: 'app-products',
  template: `
    <div class="grid">
      <div class="col-12">
        <div class="card">
          <h5>Quản lý Sản phẩm</h5>
          <p class="text-600">Chức năng đang được phát triển...</p>
          <p class="text-sm">Sử dụng ProductsController đã có sẵn trong backend.</p>
        </div>
      </div>
    </div>
  `,
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class ProductsComponent implements OnInit {
  ngOnInit(): void {}
}

