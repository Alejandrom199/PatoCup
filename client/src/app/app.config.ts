import { ApplicationConfig, LOCALE_ID } from '@angular/core'; // 1. Importar LOCALE_ID
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { credentialsInterceptor } from './core/interceptors/credentials-interceptor';

import { registerLocaleData } from '@angular/common';
import localeEsEc from '@angular/common/locales/es-EC';
import { errorHandlerInterceptor } from './core/interceptors/error-handler-interceptor';

registerLocaleData(localeEsEc);

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([
        credentialsInterceptor,
        errorHandlerInterceptor
      ]),
    ),
    
    { provide: LOCALE_ID, useValue: 'es-EC' }
  ]
};