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
  loading = false;
  showDialog = false;
  isEdit = false;
  selectedCustomer: Customer | null = null;

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
}

