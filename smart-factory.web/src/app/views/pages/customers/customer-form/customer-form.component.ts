import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CheckboxModule } from 'primeng/checkbox';
import { PrimengModule } from '../../../../primeng.module';
import { SharedModule } from '../../../../shared.module';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    InputTextModule,
    InputTextareaModule,
    CheckboxModule,
    PrimengModule,
    SharedModule
  ],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss'
})
export class CustomerFormComponent {
  @Input() customerForm: any = {
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

  @Input() isEdit: boolean = false;
  @Input() onSave?: () => void;
}
