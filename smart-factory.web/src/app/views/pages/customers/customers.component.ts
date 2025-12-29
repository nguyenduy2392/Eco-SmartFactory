import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CustomerService } from '../../../services/customer.service';
import { Customer } from '../../../models/customer.interface';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

@Component({
  selector: 'app-customers',
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class CustomersComponent implements OnInit {
  customers: Customer[] = [];
  filteredCustomers: Customer[] = [];
  loading = false;
  showDialog = false;
  showFilters = false;
  isEdit = false;
  selectedCustomer: Customer | null = null;
  searchText = '';
  currentPage = 1;
  pageSize = 10;

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
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadCustomers();
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

    this.filteredCustomers = result;
    this.currentPage = 1;
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
    this.showDialog = true;
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
    this.showDialog = true;
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
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Cập nhật chủ hàng thành công'
          });
          this.showDialog = false;
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
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Tạo chủ hàng mới thành công'
          });
          this.showDialog = false;
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
   * View purchase orders for customer
   */
  viewPurchaseOrders(customer: Customer): void {
    // Navigate to Processing PO list filtered by customer
    this.router.navigate(['/processing-po'], { 
      queryParams: { customerId: customer.id } 
    });
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

