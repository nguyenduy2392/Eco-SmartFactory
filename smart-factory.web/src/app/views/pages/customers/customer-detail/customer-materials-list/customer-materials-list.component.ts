import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MaterialService } from '../../../../../services/material.service';
import { Material } from '../../../../../models/material.interface';
import { SharedModule } from '../../../../../shared.module';
import { PrimengModule } from '../../../../../primeng.module';

@Component({
  selector: 'app-customer-materials-list',
  templateUrl: './customer-materials-list.component.html',
  styleUrls: ['./customer-materials-list.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class CustomerMaterialsListComponent implements OnInit, OnChanges {
  @Input() customerId?: string;

  materials: Material[] = [];
  filteredMaterials: Material[] = [];
  paginatedMaterials: Material[] = [];
  materialsLoading = false;
  selectedMaterialId: string | null = null;
  materialOptions: { label: string; value: string }[] = [];
  currentPage = 1;
  pageSize = 10;
  searchText = '';

  constructor(
    private materialService: MaterialService
  ) { }

  ngOnInit(): void {
    if (this.customerId) {
      this.loadMaterials();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['customerId'] && !changes['customerId'].firstChange) {
      this.loadMaterials();
    }
  }

  loadMaterials(): void {
    if (!this.customerId) return;

    this.materialsLoading = true;
    // Chỉ lấy materials thuộc riêng khách hàng này, không bao gồm materials dùng chung
    this.materialService.getByCustomerOnly(this.customerId).subscribe({
      next: (materials) => {
        this.materials = materials;
        this.updateMaterialOptions();
        this.updateFilteredMaterials();
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

  applyMaterialFilter(): void {
    this.currentPage = 1;
    this.updateFilteredMaterials();
    this.updatePaginatedMaterials();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.updateFilteredMaterials();
    this.updatePaginatedMaterials();
  }

  updateFilteredMaterials(): void {
    let filtered = this.materials;

    // Filter by selected material
    if (this.selectedMaterialId && this.selectedMaterialId !== '') {
      filtered = filtered.filter(m => m.id === this.selectedMaterialId);
    }

    // Filter by search text
    if (this.searchText) {
      const search = this.searchText.toLowerCase();
      filtered = filtered.filter(m =>
        m.code.toLowerCase().includes(search) ||
        m.name.toLowerCase().includes(search)
      );
    }

    this.filteredMaterials = filtered;
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
}
