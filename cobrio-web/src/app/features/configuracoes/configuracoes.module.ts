import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

// PrimeNG Modules
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

// Components
import { EmailConfigComponent } from './email-config/email-config.component';

const routes: Routes = [
  {
    path: 'email',
    component: EmailConfigComponent
  },
  {
    path: '',
    redirectTo: 'email',
    pathMatch: 'full'
  }
];

@NgModule({
  declarations: [
    EmailConfigComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    CardModule,
    InputTextModule,
    ButtonModule,
    ToastModule,
    TooltipModule,
    ProgressSpinnerModule
  ]
})
export class ConfiguracoesModule { }
