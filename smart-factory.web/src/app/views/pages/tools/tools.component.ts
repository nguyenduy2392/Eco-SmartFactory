import { Component, OnInit } from '@angular/core';
import { ToolService } from '../../../services/tool.service';
import { MaterialService } from '../../../services/material.service';
import { Tool } from '../../../models/tool.interface';
import { Material } from '../../../models/material.interface';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

interface InventoryItem {
  id: string;
  code: string;
  name: string;
  type: string;
  unit: string;
  currentStock: number;
  minStock?: number;
  supplier?: string;
  status: string;
  itemType: 'material' | 'tool';
  description?: string;
  isActive: boolean;
}

@Component({
  selector: 'app-tools',
  templateUrl: './tools.component.html',
  styleUrls: ['./tools.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class ToolsComponent implements OnInit {
  materials: Material[] = [];
  tools: Tool[] = [];
  items: InventoryItem[] = [];
  filteredItems: InventoryItem[] = [];
  loading = false;
  showDialog = false;
  showFilters = false;
  isEdit = false;
  selectedItem: InventoryItem | null = null;
  searchText = '';
  selectedType: string = '';
  selectedStockStatus: string = '';
  currentPage = 1;
  pageSize = 10;

  typeOptions = [
    { label: 'Tất cả loại', value: '' },
    { label: 'Plastic', value: 'Plastic' },
    { label: 'Paint', value: 'Paint' },
    { label: 'Mold', value: 'Mold' },
    { label: 'Tool', value: 'Tool' },
    { label: 'Component', value: 'Component' }
  ];

  stockStatusOptions = [
    { label: 'Tất cả', value: '' },
    { label: 'Còn hàng', value: 'instock' },
    { label: 'Sắp hết', value: 'lowstock' },
    { label: 'Hết hàng', value: 'outstock' }
  ];

  // Form data
  itemForm: any = {
    code: '',
    name: '',
    type: '',
    unit: '',
    currentStock: 0,
    minStock: 0,
    supplier: '',
    description: '',
    isActive: true
  };

  constructor(
    private toolService: ToolService,
    private materialService: MaterialService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  /**
   * Load materials and tools
   */
  loadData(): void {
    this.loading = true;
    
    // Load materials
    this.materialService.getAll().subscribe({
      next: (materials) => {
        this.materials = materials;
        this.combineItems();
      },
      error: (error) => {
        console.error('Error loading materials:', error);
        this.loading = false;
      }
    });

    // Load tools
    this.toolService.getAll().subscribe({
      next: (tools) => {
        this.tools = tools;
        this.combineItems();
      },
      error: (error) => {
        console.error('Error loading tools:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Combine materials and tools into unified items
   */
  combineItems(): void {
    const materialItems: InventoryItem[] = this.materials.map(m => ({
      id: m.id,
      code: m.code,
      name: m.name,
      type: m.type,
      unit: m.unit,
      currentStock: m.currentStock,
      minStock: m.minStock,
      supplier: m.supplier,
      status: this.getMaterialStockStatus(m),
      itemType: 'material',
      description: m.description,
      isActive: m.isActive
    }));

    const toolItems: InventoryItem[] = this.tools.map(t => ({
      id: t.id,
      code: t.code,
      name: t.name,
      type: t.type,
      unit: 'Unit',
      currentStock: t.status === 'Available' ? 1 : 0,
      minStock: undefined,
      supplier: t.ownerName,
      status: t.status,
      itemType: 'tool',
      description: t.description,
      isActive: t.isActive
    }));

    this.items = [...materialItems, ...toolItems];
    this.applyFilters();
    this.loading = false;
  }

  /**
   * Get stock status for material
   */
  getMaterialStockStatus(material: Material): string {
    if (material.currentStock === 0) return 'outstock';
    if (material.currentStock <= material.minStock) return 'lowstock';
    return 'instock';
  }

  /**
   * Apply search and filters
   */
  applyFilters(): void {
    let result = [...this.items];

    // Search filter
    if (this.searchText) {
      const searchLower = this.searchText.toLowerCase();
      result = result.filter(item => 
        item.name.toLowerCase().includes(searchLower) ||
        item.code.toLowerCase().includes(searchLower) ||
        item.supplier?.toLowerCase().includes(searchLower)
      );
    }

    // Type filter
    if (this.selectedType) {
      result = result.filter(item => item.type === this.selectedType);
    }

    // Stock status filter
    if (this.selectedStockStatus) {
      result = result.filter(item => item.status === this.selectedStockStatus);
    }

    this.filteredItems = result;
    this.currentPage = 1;
  }

  /**
   * On search input
   */
  onSearch(): void {
    this.applyFilters();
  }

  /**
   * Get item icon
   */
  getItemIcon(type: string): string {
    const iconMap: { [key: string]: string } = {
      'Plastic': 'pi pi-circle-fill',
      'Paint': 'pi pi-palette',
      'Mold': 'pi pi-cog',
      'Tool': 'pi pi-wrench',
      'Component': 'pi pi-th-large'
    };
    return iconMap[type] || 'pi pi-box';
  }

  /**
   * Get item icon class
   */
  getItemIconClass(type: string): string {
    const classMap: { [key: string]: string } = {
      'Plastic': 'icon-blue',
      'Paint': 'icon-orange',
      'Mold': 'icon-purple',
      'Tool': 'icon-gray',
      'Component': 'icon-teal'
    };
    return classMap[type] || 'icon-default';
  }

  /**
   * Get type severity for tag
   */
  getTypeSeverity(type: string): string {
    const severityMap: { [key: string]: string } = {
      'Plastic': 'info',
      'Paint': 'warning',
      'Mold': 'secondary',
      'Tool': 'contrast',
      'Component': 'success'
    };
    return severityMap[type] || 'info';
  }

  /**
   * Get quantity class
   */
  getQuantityClass(item: InventoryItem): string {
    if (item.status === 'outstock') return 'quantity-out';
    if (item.status === 'lowstock') return 'quantity-low';
    return 'quantity-normal';
  }

  /**
   * Get stock status class
   */
  getStockStatusClass(item: InventoryItem): string {
    if (item.status === 'instock' || item.status === 'Available') return 'status-dot-green';
    if (item.status === 'lowstock') return 'status-dot-red';
    if (item.status === 'outstock') return 'status-dot-gray';
    return 'status-dot-blue';
  }

  /**
   * Get stock status text
   */
  getStockStatusText(item: InventoryItem): string {
    if (item.itemType === 'tool') {
      const statusMap: { [key: string]: string } = {
        'Available': 'Sẵn sàng',
        'InUse': 'Đang sử dụng',
        'Maintenance': 'Bảo trì',
        'Returned': 'Đã trả'
      };
      return statusMap[item.status] || item.status;
    }
    
    const statusMap: { [key: string]: string } = {
      'instock': 'Còn hàng',
      'lowstock': 'Sắp hết',
      'outstock': 'Hết hàng'
    };
    return statusMap[item.status] || 'Còn hàng';
  }

  /**
   * Format quantity
   */
  formatQuantity(qty: number): string {
    return qty.toLocaleString('vi-VN');
  }

  /**
   * Open create dialog
   */
  openCreateDialog(): void {
    this.isEdit = false;
    this.selectedItem = null;
    this.itemForm = {
      code: '',
      name: '',
      type: '',
      unit: '',
      currentStock: 0,
      minStock: 0,
      supplier: '',
      description: '',
      isActive: true
    };
    this.showDialog = true;
  }

  /**
   * Open edit dialog
   */
  openEditDialog(item: InventoryItem): void {
    this.isEdit = true;
    this.selectedItem = item;
    this.itemForm = {
      code: item.code,
      name: item.name,
      type: item.type,
      unit: item.unit,
      currentStock: item.currentStock,
      minStock: item.minStock || 0,
      supplier: item.supplier || '',
      description: item.description || '',
      isActive: item.isActive
    };
    this.showDialog = true;
  }

  /**
   * Save item
   */
  saveItem(): void {
    // TODO: Implement save functionality
    this.messageService.add({
      severity: 'info',
      summary: 'Thông báo',
      detail: 'Chức năng lưu đang được phát triển'
    });
    this.showDialog = false;
  }

  /**
   * View history
   */
  viewHistory(item: InventoryItem): void {
    // TODO: Implement view history
    this.messageService.add({
      severity: 'info',
      summary: 'Thông báo',
      detail: 'Chức năng xem lịch sử đang được phát triển'
    });
  }

  /**
   * Export report
   */
  exportReport(): void {
    // TODO: Implement export
    this.messageService.add({
      severity: 'info',
      summary: 'Thông báo',
      detail: 'Chức năng xuất báo cáo đang được phát triển'
    });
  }

  /**
   * Pagination helpers
   */
  getFirstRecord(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getLastRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredItems.length);
  }

  getTotalPages(): number {
    return Math.ceil(this.filteredItems.length / this.pageSize);
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage(): void {
    if (this.currentPage < this.getTotalPages()) {
      this.currentPage++;
    }
  }
}
