import { Component, Input, OnInit } from '@angular/core';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss'],
  imports:[ConfirmDialogModule]
})
export class ConfirmDialogComponent {
  @Input() content: any = null;
  constructor() { }
}
