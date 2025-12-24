import { Component, OnInit } from '@angular/core';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

@Component({
  selector: 'app-tools',
  template: `
    <div class="grid">
      <div class="col-12">
        <div class="card">
          <h5>Quản lý Tool & Khuôn</h5>
          <p class="text-600">Chức năng đang được phát triển...</p>
          <p class="text-sm">Backend API cần được tạo trước khi sử dụng tính năng này.</p>
        </div>
      </div>
    </div>
  `,
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class ToolsComponent implements OnInit {
  ngOnInit(): void {}
}

