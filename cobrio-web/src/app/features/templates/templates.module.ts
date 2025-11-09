import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { TemplatesRoutingModule } from './templates-routing.module';
import { TemplatesListComponent } from './templates-list/templates-list.component';
import { TemplateFormComponent } from './template-form/template-form.component';

// PrimeNG Modules
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ChipModule } from 'primeng/chip';
import { TagModule } from 'primeng/tag';
import { EditorModule } from 'primeng/editor';
import { DropdownModule } from 'primeng/dropdown';
import { CardModule } from 'primeng/card';

@NgModule({
  declarations: [
    TemplatesListComponent,
    TemplateFormComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TemplatesRoutingModule,
    // PrimeNG
    TableModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    DialogModule,
    ToastModule,
    ConfirmDialogModule,
    TooltipModule,
    ChipModule,
    TagModule,
    EditorModule,
    DropdownModule,
    CardModule
  ]
})
export class TemplatesModule { }
