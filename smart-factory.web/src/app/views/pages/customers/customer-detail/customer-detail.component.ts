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
  paginatedMaterials: Material[] = [];
  materialsLoading = false;
  selectedMaterialId: string | null = null;
  materialOptions: { label: string; value: string }[] = [];
  currentPage = 1;
  pageSize = 10;


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
        this.updatePaginatedMaterials();
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
    this.currentPage = 1;
    this.updatePaginatedMaterials();
  }

  /**
   * Update paginated materials
   */
  updatePaginatedMaterials(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.paginatedMaterials = this.filteredMaterials.slice(start, end);
  }

  /**
   * Pagination helpers
   */
  getFirstRecord(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getLastRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredMaterials.length);
  }

  getTotalPages(): number {
    return Math.ceil(this.filteredMaterials.length / this.pageSize);
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const maxVisible = 5;
    
    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (this.currentPage <= 3) {
        for (let i = 1; i <= maxVisible; i++) {
          pages.push(i);
        }
      } else if (this.currentPage >= totalPages - 2) {
        for (let i = totalPages - maxVisible + 1; i <= totalPages; i++) {
          pages.push(i);
        }
      } else {
        for (let i = this.currentPage - 2; i <= this.currentPage + 2; i++) {
          pages.push(i);
        }
      }
    }
    
    return pages;
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePaginatedMaterials();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.getTotalPages()) {
      this.currentPage++;
      this.updatePaginatedMaterials();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.getTotalPages()) {
      this.currentPage = page;
      this.updatePaginatedMaterials();
    }
  }


  goBack(): void {
    this.location.back();
  }

  /**
   * Get customer initials for avatar
   */
  getInitials(name: string): string {
    if (!name) return '??';
    const words = name.trim().split(/\s+/);
    if (words.length >= 2) {
      return (words[0][0] + words[words.length - 1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  /**
   * Get avatar color based on customer name
   */
  getAvatarColor(name: string): string {
    const avatarColors = [
      '#3B82F6', // blue
      '#10B981', // green
      '#F97316', // orange
      '#A855F7', // purple
      '#EF4444'  // red
    ];
    if (!name) return avatarColors[0];
    const index = name.charCodeAt(0) % avatarColors.length;
    return avatarColors[index];
  }

}

