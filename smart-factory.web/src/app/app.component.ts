import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { NgxSpinnerModule } from 'ngx-spinner';
import { Title } from '@angular/platform-browser';
import { iconSubset } from './icons/icon-subset';
import { UiAlertComponent } from "./components/ui-alert/ui-alert.component";
import { UiToastComponent } from './components/ui-toast/ui-toast.component';
import { UiToastService } from './services/shared/ui-toast.service';
import { UiModalComponent } from "./components/ui-modal/ui-modal.component";
import { PrimengModule } from './primeng.module';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { PrimeNGConfig } from 'primeng/api';
import { NgHttpLoaderComponent, Spinkit } from 'ng-http-loader';
import { LoadingComponent } from './layout/loading/loading.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule,
    PrimengModule,
    RouterOutlet,
    NgxSpinnerModule,
    UiModalComponent,
    ConfirmDialogComponent,
    NgHttpLoaderComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, AfterViewInit {
  title = "Bao bì Việt Nhật - PMBK";
  loadingRoute: any = false;
  isConnected: any = true;
  placement: string = 'top-right';

  /// Loading component
  public loading = LoadingComponent;

  constructor(
    private router: Router,
    private titleService: Title,
    private toastService: UiToastService,
    private primeNgConfig: PrimeNGConfig
  ) {
    this.titleService.setTitle(this.title);
  }
  ngAfterViewInit() {
  }
  ngOnInit(): void {
    this.router.events.subscribe((evt) => {
      if (!(evt instanceof NavigationEnd)) {
        return;
      }
    });

    //enable ripple effect
    this.primeNgConfig.ripple = true;
  }
}
