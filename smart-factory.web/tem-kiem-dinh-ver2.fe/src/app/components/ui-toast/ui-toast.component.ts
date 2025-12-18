import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, forwardRef, Input, Renderer2 } from '@angular/core';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-ui-toast',
  templateUrl: './ui-toast.component.html',
  styleUrl: './ui-toast.component.scss',
  providers: [{ provide: ToastModule, useExisting: forwardRef(() => UiToastComponent) }],
  standalone: true,
  imports: [CommonModule]

})
export class UiToastComponent{

  @Input() closeButton = true;
  @Input() title = '';
  @Input() message = '';
  @Input() showHeader = true; // Biến đầu vào để kiểm soát hiển thị của c-toast-header
  @Input() icon: string = ''; // Biến đầu vào để truyền icon
  constructor(
    public hostElement: ElementRef,
    public renderer: Renderer2,
    public changeDetectorRef: ChangeDetectorRef,
  ) {

  }
}
