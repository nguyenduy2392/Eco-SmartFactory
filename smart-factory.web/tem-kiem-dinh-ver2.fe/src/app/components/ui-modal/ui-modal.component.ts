import { ChangeDetectorRef, Component, ComponentFactoryResolver, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { UiModalService } from '../../services/shared/ui-modal.service';
import { UiModal } from '../../models/interface/uiInterface';
import { CommonModule } from '@angular/common';

import { Dialog, DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-ui-modal',
  standalone: true,
  imports: [
    CommonModule,
    DialogModule,
    ButtonModule,
    DividerModule
  ],
  templateUrl: './ui-modal.component.html',
  styleUrl: './ui-modal.component.scss'
})
export class UiModalComponent implements OnInit {
  visible = false;
  modalOptions: UiModal | null = null;
  @ViewChild('modalBody', { read: ViewContainerRef, static: true }) modalBody!: ViewContainerRef;
  constructor(
    private modalService: UiModalService,
    private cdr: ChangeDetectorRef,
    private resolver: ComponentFactoryResolver
  ) { }

  ngOnInit() {
    this.modalService.visible$.subscribe((visible) => {
      this.visible = visible;
      this.cdr.detectChanges(); // Đảm bảo giao diện người dùng được cập nhật
    });

    this.modalService.modal$.subscribe((modalOptions) => {
      this.modalOptions = modalOptions;
      this.loadBodyComponent();
      this.cdr.detectChanges(); // Đảm bảo giao diện người dùng được cập nhật
    });
  }

  close(result?: any) {
    this.modalService.closeModal(result);
  }

  handlePrimaryAction() {
    if (this.modalOptions?.onPrimaryAction) {
      this.modalOptions.onPrimaryAction();
    }else{
      this.close();
    }

  }

  handleSecondaryAction() {
    if (this.modalOptions?.onSecondaryAction) {
      this.modalOptions.onSecondaryAction();
    }
    this.close();
  }

  private loadBodyComponent() {
    if (this.modalOptions?.bodyComponent) {
      const factory = this.resolver.resolveComponentFactory(this.modalOptions.bodyComponent);
      this.modalBody.clear();
      const componentRef = this.modalBody.createComponent(factory);
      if (this.modalOptions.bodyData) {
        Object.assign(componentRef.instance, this.modalOptions.bodyData);
      }
    }
  }
}
