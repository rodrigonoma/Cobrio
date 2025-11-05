import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UsuariosListComponent } from './usuarios-list/usuarios-list.component';
import { UsuarioFormComponent } from './usuario-form/usuario-form.component';

const routes: Routes = [
  {
    path: '',
    component: UsuariosListComponent
  },
  {
    path: 'novo',
    component: UsuarioFormComponent
  },
  {
    path: 'editar/:id',
    component: UsuarioFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UsuariosRoutingModule { }
