import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { RegrasCobrancaRoutingModule } from './regras-cobranca-routing.module';
import { RegrasListComponent } from './regras-list/regras-list.component';
import { RegraFormComponent } from './regra-form/regra-form.component';

// PrimeNG Modules
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ChipModule } from 'primeng/chip';
import { TagModule } from 'primeng/tag';
import { EditorModule } from 'primeng/editor';
import { FileUploadModule } from 'primeng/fileupload';
import { RadioButtonModule } from 'primeng/radiobutton';
import { CalendarModule } from 'primeng/calendar';
import { TesteWebhookComponent } from './teste-webhook/teste-webhook.component';

@NgModule({
  declarations: [
    RegrasListComponent,
    RegraFormComponent,
    TesteWebhookComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RegrasCobrancaRoutingModule,
    // PrimeNG
    TableModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    InputNumberModule,
    DropdownModule,
    DialogModule,
    ToastModule,
    ConfirmDialogModule,
    TooltipModule,
    ChipModule,
    TagModule,
    EditorModule,
    FileUploadModule,
    RadioButtonModule,
    CalendarModule
  ]
})
export class RegrasCobrancaModule { }
