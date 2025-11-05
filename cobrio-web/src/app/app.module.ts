import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TokenInterceptor, ErrorInterceptor } from './core/interceptors';
import { LoginComponent } from './features/auth/login/login.component';

// PrimeNG Modules
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { InputSwitchModule } from 'primeng/inputswitch';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { SkeletonModule } from 'primeng/skeleton';
import { MessageService, ConfirmationService } from 'primeng/api';
import { NgChartsModule } from 'ng2-charts';
import { MainLayoutComponent } from './shared/layout/main-layout/main-layout.component';
import { SidebarComponent } from './shared/layout/sidebar/sidebar.component';
import { HeaderComponent } from './shared/layout/header/header.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { AssinaturasListComponent } from './features/assinaturas/assinaturas-list/assinaturas-list.component';
import { PlanosListComponent } from './features/planos/planos-list/planos-list.component';
import { FinanceiroListComponent } from './features/financeiro/financeiro-list/financeiro-list.component';
import { RelatoriosComponent } from './features/relatorios/relatorios/relatorios.component';
import { PlanoFormComponent } from './features/planos/plano-form/plano-form.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    MainLayoutComponent,
    SidebarComponent,
    HeaderComponent,
    DashboardComponent,
    AssinaturasListComponent,
    PlanosListComponent,
    FinanceiroListComponent,
    RelatoriosComponent,
    PlanoFormComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    AppRoutingModule,
    // PrimeNG
    InputTextModule,
    ButtonModule,
    CheckboxModule,
    ToastModule,
    TableModule,
    DialogModule,
    ConfirmDialogModule,
    TooltipModule,
    TagModule,
    InputNumberModule,
    DropdownModule,
    InputSwitchModule,
    InputTextareaModule,
    SkeletonModule,
    NgChartsModule
  ],
  providers: [
    MessageService,
    ConfirmationService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
