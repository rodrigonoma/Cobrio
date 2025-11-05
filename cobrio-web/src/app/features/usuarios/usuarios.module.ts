import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { UsuariosRoutingModule } from './usuarios-routing.module';
import { UsuariosListComponent } from './usuarios-list/usuarios-list.component';
import { UsuarioFormComponent } from './usuario-form/usuario-form.component';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@NgModule({
  declarations: [
    UsuariosListComponent,
    UsuarioFormComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    UsuariosRoutingModule,
    // PrimeNG
    ButtonModule,
    TableModule,
    ToastModule,
    InputTextModule,
    DropdownModule,
    TooltipModule,
    DialogModule,
    ConfirmDialogModule
  ]
})
export class UsuariosModule { }
