import { ApplicationConfig, provideZoneChangeDetection, importProvidersFrom } from '@angular/core';
import { provideRouter, withEnabledBlockingInitialNavigation, withHashLocation, withInMemoryScrolling, withRouterConfig, withViewTransitions } from '@angular/router';
import { routes } from './app.routes';
import { FormsModule } from '@angular/forms';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { HttpClient, provideHttpClient, withInterceptors, withInterceptorsFromDi } from '@angular/common/http';
import { EnvServiceProvider } from './env.service.provider';
import { FormatPricePipe } from './shared/pipes';
import { ConfirmationService, MessageService } from 'primeng/api';
import { registerLocaleData } from '@angular/common';
import vi from '@angular/common/locales/vi';
import { pendingRequestsInterceptor$ } from 'ng-http-loader';
import { jwtInterceptor } from './interceptors/jwt.interceptor';

registerLocaleData(vi);

export const appConfig: ApplicationConfig = {
  providers: [EnvServiceProvider, provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes,
      withRouterConfig({
        onSameUrlNavigation: 'reload'
      }),
      withInMemoryScrolling({
        scrollPositionRestoration: 'top',
        anchorScrolling: 'enabled'
      }),
      withEnabledBlockingInitialNavigation(),
      withViewTransitions(),
      withHashLocation()
    ),
    provideHttpClient(withInterceptors([jwtInterceptor, pendingRequestsInterceptor$])),
    provideAnimationsAsync(),
    provideHttpClient(),
    importProvidersFrom(FormsModule),
    FormatPricePipe,
    MessageService,
    ConfirmationService
  ]
};

