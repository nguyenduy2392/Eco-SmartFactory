import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CustomerService } from '../../../services/customer.service';
import { Customer } from '../../../models/customer.interface';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';
import { Menu } from 'primeng/menu';
import { UiModalService } from '../../../services/shared/ui-modal.service';
import { CustomerFormComponent } from './customer-form/customer-form.component';

@Component({
  selector: 'app-customers',
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class CustomersComponent implements OnInit {
  @ViewChild('actionMenu') actionMenu!: Menu;

  customers: Customer[] = [];
  filteredCustomers: Customer[] = [];
  paginatedCustomers: Customer[] = [];
  loading = false;
  showDeleteDialog = false;
  showFilters = false;
  showSettings = false;
  isEdit = false;
  selectedCustomer: Customer | null = null;
  customerToDelete: Customer | null = null;
  searchText = '';
  currentPage = 1;
  pageSize = 10;
  selectAll = false;
  sortField = '';
  sortOrder: 'asc' | 'desc' = 'asc';
  
  actionMenuItems: any[] = [];
  currentActionCustomer: Customer | null = null;

  // Form data
  customerForm: any = {
    code: '',
    name: '',
    address: '',
    contactPerson: '',
    email: '',
    phone: '',
    paymentTerms: '',
    notes: '',
    isActive: true
  };

  // Avatar colors
  private avatarColors = [
    '#3B82F6', // blue
    '#10B981', // green
    '#F97316', // orange
    '#A855F7', // purple
    '#EF4444'  // red
  ];

  constructor(
    private customerService: CustomerService,
    private messageService: MessageService,
    private router: Router,
    private uiModalService: UiModalService
  ) { }

  ngOnInit(): void {
    this.loadCustomers();
  }

  /**
   * Navigate to customer detail page
   */
  navigateToDetail(customer: Customer): void {
    this.router.navigate(['/customers', customer.id]);
  }

  /**
   * Load danh sách chủ hàng
   */
  loadCustomers(): void {
    this.loading = true;
    this.customerService.getAll().subscribe({
      next: (customers) => {
        this.customers = customers;
        this.applyFilters();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải danh sách chủ hàng'
        });
      }
    });
  }

  /**
   * Apply search and filters
   */
  applyFilters(): void {
    let result = [...this.customers];

    // Search filter
    if (this.searchText) {
      const searchLower = this.searchText.toLowerCase();
      result = result.filter(c => 
        c.name.toLowerCase().includes(searchLower) ||
        c.phone?.toLowerCase().includes(searchLower) ||
        c.contactPerson?.toLowerCase().includes(searchLower) ||
        c.code.toLowerCase().includes(searchLower)
      );
    }

    // Add selected property for checkbox
    result = result.map(c => ({ ...c, selected: false }));

    // Apply sorting
    if (this.sortField) {
      result.sort((a, b) => {
        const aValue = (a as any)[this.sortField] || '';
        const bValue = (b as any)[this.sortField] || '';
        const comparison = aValue.toString().localeCompare(bValue.toString());
        return this.sortOrder === 'asc' ? comparison : -comparison;
      });
    }

    this.filteredCustomers = result;
    this.currentPage = 1;
    this.updatePaginatedCustomers();
  }

  /**
   * Apply status filter
   */
  applyStatusFilter(status: 'all' | 'active' | 'inactive'): void {
    let result = [...this.customers];

    if (status === 'active') {
      result = result.filter(c => c.isActive);
    } else if (status === 'inactive') {
      result = result.filter(c => !c.isActive);
    }

    // Apply search if exists
    if (this.searchText) {
      const searchLower = this.searchText.toLowerCase();
      result = result.filter(c => 
        c.name.toLowerCase().includes(searchLower) ||
        c.phone?.toLowerCase().includes(searchLower) ||
        c.contactPerson?.toLowerCase().includes(searchLower) ||
        c.code.toLowerCase().includes(searchLower)
      );
    }

    result = result.map(c => ({ ...c, selected: false }));

    if (this.sortField) {
      result.sort((a, b) => {
        const aValue = (a as any)[this.sortField] || '';
        const bValue = (b as any)[this.sortField] || '';
        const comparison = aValue.toString().localeCompare(bValue.toString());
        return this.sortOrder === 'asc' ? comparison : -comparison;
      });
    }

    this.filteredCustomers = result;
    this.currentPage = 1;
    this.updatePaginatedCustomers();
  }

  /**
   * On search input
   */
  onSearch(): void {
    this.applyFilters();
  }

  /**
   * Mở dialog tạo mới
   */
  openCreateDialog(): void {
    this.isEdit = false;
    this.customerForm = {
      code: '',
      name: '',
      address: '',
      contactPerson: '',
      email: '',
      phone: '',
      paymentTerms: '',
      notes: '',
      isActive: true
    };
    
    this.uiModalService.openModal({
      title: 'Thêm chủ hàng mới',
      bodyComponent: CustomerFormComponent,
      bodyData: {
        customerForm: this.customerForm,
        isEdit: this.isEdit,
        onSave: () => this.saveCustomer()
      },
      size: '50vw',
      showFooter: false
    });
  }

  /**
   * Mở dialog chỉnh sửa
   */
  openEditDialog(customer: Customer): void {
    this.isEdit = true;
    this.selectedCustomer = customer;
    this.customerForm = {
      code: customer.code,
      name: customer.name,
      address: customer.address || '',
      contactPerson: customer.contactPerson || '',
      email: customer.email || '',
      phone: customer.phone || '',
      paymentTerms: customer.paymentTerms || '',
      notes: customer.notes || '',
      isActive: customer.isActive
    };
    
    this.uiModalService.openModal({
      title: 'Chỉnh sửa chủ hàng',
      bodyComponent: CustomerFormComponent,
      bodyData: {
        customerForm: this.customerForm,
        isEdit: this.isEdit,
        onSave: () => this.saveCustomer()
      },
      size: '50vw',
      showFooter: false
    });
  }

  /**
   * Lưu chủ hàng
   */
  saveCustomer(): void {
    if (!this.customerForm.code || !this.customerForm.name) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng nhập đầy đủ mã và tên chủ hàng'
      });
      return;
    }

    if (this.isEdit && this.selectedCustomer) {
      // Update
      this.customerService.update(this.selectedCustomer.id, this.customerForm).subscribe({
        next: () => {
          this.uiModalService.closeModal();
          this.uiModalService.closeModal();
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Cập nhật chủ hàng thành công'
          });
          this.loadCustomers();
        },
        error: (error) => {
          console.error('Error updating customer:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: error.error?.message || 'Không thể cập nhật chủ hàng'
          });
        }
      });
    } else {
      // Create
      this.customerService.create(this.customerForm).subscribe({
        next: () => {
          this.uiModalService.closeModal();
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Tạo chủ hàng mới thành công'
          });
          this.loadCustomers();
        },
        error: (error) => {
          console.error('Error creating customer:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: error.error?.message || 'Không thể tạo chủ hàng'
          });
        }
      });
    }
  }

  /**
   * Get badge severity cho trạng thái
   */
  getStatusSeverity(isActive: boolean): string {
    return isActive ? 'success' : 'danger';
  }

  /**
   * Get status text
   */
  getStatusText(isActive: boolean): string {
    return isActive ? 'Hoạt động' : 'Ngừng hoạt động';
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
    if (!name) return this.avatarColors[0];
    const index = name.charCodeAt(0) % this.avatarColors.length;
    return this.avatarColors[index];
  }

  /**
   * Truncate address for display
   */
  truncateAddress(address?: string): string {
    if (!address) return '-';
    return address.length > 40 ? address.substring(0, 40) + '...' : address;
  }

  /**
   * View customer detail page
   */
  viewPurchaseOrders(customer: Customer): void {
    // Navigate to customer detail page
    this.router.navigate(['/customers', customer.id]);
  }

  /**
   * Sort by field
   */
  sortBy(field: string): void {
    if (this.sortField === field) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortOrder = 'asc';
    }
    this.applyFilters();
  }

  /**
   * Format date
   */
  formatDate(dateString?: string): string {
    if (!dateString) return '-';
    const date = new Date(dateString);
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const day = date.getDate();
    const month = months[date.getMonth()];
    const year = date.getFullYear();
    return `${day} ${month}, ${year}`;
  }

  /**
   * Toggle select all
   */
  toggleSelectAll(): void {
    this.paginatedCustomers.forEach(customer => {
      (customer as any).selected = this.selectAll;
    });
  }

  /**
   * Toggle customer selection
   */
  toggleCustomer(customer: Customer): void {
    const selected = this.paginatedCustomers.filter(c => (c as any).selected).length;
    this.selectAll = selected === this.paginatedCustomers.length;
  }

  /**
   * Check if customer is selected
   */
  isSelected(customer: Customer): boolean {
    return (customer as any).selected || false;
  }

  /**
   * Call customer
   */
  callCustomer(customer: Customer): void {
    if (customer.phone) {
      window.location.href = `tel:${customer.phone}`;
    } else {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Chủ hàng này chưa có số điện thoại'
      });
    }
  }

  /**
   * Message customer
   */
  messageCustomer(customer: Customer): void {
    if (customer.phone) {
      window.location.href = `sms:${customer.phone}`;
    } else {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Chủ hàng này chưa có số điện thoại'
      });
    }
  }

  /**
   * Show action menu
   */
  showActionMenu(event: Event, customer: Customer): void {
    this.currentActionCustomer = customer;
    this.actionMenuItems = this.getActionMenuItems(customer);
    // Menu will be toggled by ViewChild reference
  }

  /**
   * Get action menu items for a customer
   */
  getActionMenuItems(customer: Customer): any[] {
    return [
      {
        label: 'Xem chi tiết',
        icon: 'pi pi-eye',
        command: () => this.viewPurchaseOrders(customer)
      },
      {
        label: 'Chỉnh sửa',
        icon: 'pi pi-pencil',
        command: () => this.openEditDialog(customer)
      },
      {
        separator: true
      },
      {
        label: 'Xóa',
        icon: 'pi pi-trash',
        styleClass: 'p-menuitem-danger',
        command: () => this.confirmDelete(customer)
      }
    ];
  }

  /**
   * Confirm delete
   */
  confirmDelete(customer: Customer): void {
    this.customerToDelete = customer;
    this.showDeleteDialog = true;
  }

  /**
   * Delete customer
   */
  deleteCustomer(): void {
    if (!this.customerToDelete) return;

    this.customerService.delete(this.customerToDelete.id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Xóa chủ hàng thành công'
        });
        this.showDeleteDialog = false;
        this.customerToDelete = null;
        this.loadCustomers();
      },
      error: (error) => {
        console.error('Error deleting customer:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error.error?.message || 'Không thể xóa chủ hàng'
        });
      }
    });
  }

  /**
   * Update paginated customers
   */
  updatePaginatedCustomers(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.paginatedCustomers = this.filteredCustomers.slice(start, end);
  }

  /**
   * Pagination helpers
   */
  getFirstRecord(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getLastRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredCustomers.length);
  }

  getTotalPages(): number {
    return Math.ceil(this.filteredCustomers.length / this.pageSize);
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
      this.updatePaginatedCustomers();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.getTotalPages()) {
      this.currentPage++;
      this.updatePaginatedCustomers();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.getTotalPages()) {
      this.currentPage = page;
      this.updatePaginatedCustomers();
    }
  }
}

