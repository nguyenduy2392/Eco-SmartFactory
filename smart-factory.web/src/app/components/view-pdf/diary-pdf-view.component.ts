import { Component, Input, OnInit } from '@angular/core';
import { PrimengModule } from '../../primeng.module';
import { SharedModule } from '../../shared.module';
import { EnvService } from '../../env.service';
import { UiModalService } from '../../services/shared/ui-modal.service';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-diary-pdf-view',
  standalone: true,
  imports: [
    PrimengModule,
    SharedModule],
  templateUrl: './diary-pdf-view.component.html',
  styleUrl: './diary-pdf-view.component.scss'
})

export class DiaryPdfViewComponent implements OnInit {
  @Input() url: string = "";

  show: any;

  constructor(
    private _env: EnvService,
    private _modal: UiModalService,
    private _sanitizer: DomSanitizer,
  ) { }

  /** Hàm khởi tạo dữ liệu
   */
  ngOnInit() {
    this.show = this._sanitizer.bypassSecurityTrustResourceUrl(`${this._env.baseApiUrl}${this.url}`);
  }

  /// Đóng
  Close() {
    this._modal.closeModal();
  }
}
