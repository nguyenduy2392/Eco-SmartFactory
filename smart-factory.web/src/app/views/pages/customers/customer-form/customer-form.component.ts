import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CheckboxModule } from 'primeng/checkbox';
import { PrimengModule } from '../../../../primeng.module';
import { SharedModule } from '../../../../shared.module';
import { UiModalService } from '../../../../services/shared/ui-modal.service';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';

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
    SharedModule,
    CKEditorModule
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

  public Editor = ClassicEditor;

  constructor(private uiModalService: UiModalService) {}

  handleSave(): void {
    if (this.onSave) {
      this.onSave();
    }
  }

  handleClose(): void {
    this.uiModalService.closeModal();
  }
}
