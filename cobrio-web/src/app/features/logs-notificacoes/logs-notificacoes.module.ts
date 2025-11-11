import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

// PrimeNG Modules
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { AccordionModule } from 'primeng/accordion';

// Components
import { LogsListComponent } from './logs-list/logs-list.component';
import { LogDetailModalComponent } from './log-detail-modal/log-detail-modal.component';

const routes: Routes = [
  {
    path: '',
    component: LogsListComponent
  }
];

@NgModule({
  declarations: [
    LogsListComponent,
    LogDetailModalComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild(routes),
    TableModule,
    ButtonModule,
    DialogModule,
    TagModule,
    CalendarModule,
    DropdownModule,
    InputTextModule,
    TooltipModule,
    AccordionModule
  ]
})
export class LogsNotificacoesModule { }
