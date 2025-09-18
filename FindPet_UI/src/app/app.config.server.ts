import { provideServerRendering } from '@angular/ssr';
import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { appConfig } from './app.config';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { tokenInterceptor } from './interceptor/token.interceptor';

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering(),
    provideHttpClient(withInterceptors([tokenInterceptor]),withFetch())
  ]
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
