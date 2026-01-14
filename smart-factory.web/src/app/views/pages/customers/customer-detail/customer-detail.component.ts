import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { CustomerService } from '../../../../services/customer.service';
import { Customer } from '../../../../models/customer.interface';
import { MessageService } from 'primeng/api';
import { SharedModule } from '../../../../shared.module';
import { PrimengModule } from '../../../../primeng.module';
import { AvailabilityCheckComponent } from '../../../../components/availability-check/availability-check.component';
import { POListComponent } from '../../../../components/processing-po/po-list/po-list.component';
import { CustomerMaterialsListComponent } from './customer-materials-list/customer-materials-list.component';

@Component({
  selector: 'app-customer-detail',
  templateUrl: './customer-detail.component.html',
  styleUrls: ['./customer-detail.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule, AvailabilityCheckComponent, POListComponent, CustomerMaterialsListComponent]
})
export class CustomerDetailComponent implements OnInit {
  customer: Customer | null = null;
  customerId: string | null = null;
  loading = false;
  activeTabIndex = 0;


  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private customerService: CustomerService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.customerId = params['id'];
      if (this.customerId) {
        this.loadCustomerDetail();
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

