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
import { AppConfig, LayoutService } from './layout/service/app.layout.service';

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
  title = "Hải Tân";
  loadingRoute: any = false;
  isConnected: any = true;
  placement: string = 'top-right';

  /// Loading component
  public loading = LoadingComponent;

  constructor(
    private router: Router,
    private titleService: Title,
    private primeNgConfig: PrimeNGConfig,
    private layoutService: LayoutService
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

    //optional configuration with the default configuration
    const config: AppConfig = {
      ripple: true,                      //toggles ripple on and off
      inputStyle: 'outlined',             //default style for input elements
      menuMode: 'static',                 //layout mode of the menu, valid values are "static" and "overlay"
      colorScheme: 'light',               
      theme: 'lara-light-indigo',         
      scale: 14                           
    };
    this.layoutService.config.set(config);
  }
}
