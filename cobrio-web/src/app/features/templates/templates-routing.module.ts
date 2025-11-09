import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TemplatesListComponent } from './templates-list/templates-list.component';
import { TemplateFormComponent } from './template-form/template-form.component';

const routes: Routes = [
  {
    path: '',
    component: TemplatesListComponent
  },
  {
    path: 'novo',
    component: TemplateFormComponent
  },
  {
    path: 'editar/:id',
    component: TemplateFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TemplatesRoutingModule { }
