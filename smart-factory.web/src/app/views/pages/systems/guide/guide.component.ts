import { AfterViewInit, Component, HostListener, OnInit, SecurityContext } from '@angular/core';
import { PrimengModule } from '../../../../primeng.module';
import { NgClass, NgIf } from '@angular/common';
import { SharedModule } from '../../../../shared.module';
import { JwtHelperService } from '@auth0/angular-jwt';
import { UiConfirm, UiModal } from '../../../../models/interface/uiInterface';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MenuItem } from 'primeng/api';
import moment from 'moment';
import { EnvService } from '../../../../env.service';
import { GenerateFile } from '../../../../shared/helper';
import { UiToastService } from '../../../../services/shared/ui-toast.service';
import { IsNull } from '../../../../services/shared/common';
import { DataTypes, FontSizes, FontStyles, InspectionTypes, Symbols, Units } from '../../../../services/shared/default-data';
import { ActivatedRoute, Router, RouterStateSnapshot } from '@angular/router';
import { UiConfirmService } from '../../../../services/shared/ui-confirm.service';
import { TextGlobalConstants } from '../../../../shared/TextGlobalContants';
import { AuthService } from '../../../../services/auth.service';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-guide',
  standalone: true,
  imports: [
    PrimengModule,
    SharedModule],
  templateUrl: './guide.component.html',
  styleUrl: './guide.component.scss'
})

export class GuideComponent implements OnInit {
  url: any;
  constructor(
    private _sanitizer: DomSanitizer,
    private _env: EnvService,
  ) { }

  ngOnInit() {
    this.url = this._sanitizer.bypassSecurityTrustResourceUrl(`https://docs.google.com/viewer?url=${this._env.baseApiUrl}files/docs/PMBK_Guide.pdf&embedded=true`);
  }
}
