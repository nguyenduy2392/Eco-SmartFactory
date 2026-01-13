import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { CustomerService } from '../../../../services/customer.service';
import { MaterialService } from '../../../../services/material.service';
import { Customer } from '../../../../models/customer.interface';
import { Material } from '../../../../models/material.interface';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../../shared.module';
import { PrimengModule } from '../../../../primeng.module';
import { AvailabilityCheckComponent } from '../../../../components/availability-check/availability-check.component';
import { POListComponent } from '../../../../components/processing-po/po-list/po-list.component';

@Component({
  selector: 'app-customer-detail',
  templateUrl: './customer-detail.component.html',
  styleUrls: ['./customer-detail.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule, AvailabilityCheckComponent, POListComponent]
})
export class CustomerDetailComponent implements OnInit {
  customer: Customer | null = null;
  customerId: string | null = null;
  loading = false;
  activeTabIndex = 0;

  // Tab 2: Materials
  materials: Material[] = [];
  filteredMaterials: Material[] = [];
  materialsLoading = false;
  selectedMaterialId: string | null = null;
  materialOptions: { label: string; value: string }[] = [];


  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private customerService: CustomerService,
    private materialService: MaterialService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.customerId = params['id'];
      if (this.customerId) {
        this.loadCustomerDetail();
        this.loadMaterials();
      }
    });
  }

  loadCustomerDetail(): void {
    if (!this.customerId) return;

    this.loading = true;
    this.customerService.getById(this.customerId).subscribe({
      next: (customer) => {
        this.customer = customer;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading customer:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải thông tin chủ hàng'
        });
        this.loading = false;
      }
    });
  }


  loadMaterials(): void {
    if (!this.customerId) return;

    this.materialsLoading = true;
    // Chỉ lấy materials thuộc riêng khách hàng này, không bao gồm materials dùng chung
    this.materialService.getByCustomerOnly(this.customerId).subscribe({
      next: (materials) => {
        this.materials = materials;
        this.updateMaterialOptions();
        this.applyMaterialFilter();
        this.materialsLoading = false;
      },
      error: (error) => {
        console.error('Error loading materials:', error);
        this.materialsLoading = false;
      }
    });
  }

  updateMaterialOptions(): void {
    this.materialOptions = [
      { label: 'Tất cả', value: '' },
      ...this.materials.map(m => ({
        label: `${m.code} - ${m.name}`,
        value: m.id
      }))
    ];
  }

  onMaterialFilterChange(): void {
    this.applyMaterialFilter();
  }

  applyMaterialFilter(): void {
    if (!this.selectedMaterialId || this.selectedMaterialId === '') {
      this.filteredMaterials = this.materials;
    } else {
      this.filteredMaterials = this.materials.filter(m => m.id === this.selectedMaterialId);
    }
  }


  goBack(): void {
    this.location.back();
  }

}

