import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RegrasListComponent } from './regras-list/regras-list.component';
import { RegraFormComponent } from './regra-form/regra-form.component';
import { TesteWebhookComponent } from './teste-webhook/teste-webhook.component';

const routes: Routes = [
  {
    path: '',
    component: RegrasListComponent
  },
  {
    path: 'novo',
    component: RegraFormComponent
  },
  {
    path: 'editar/:id',
    component: RegraFormComponent
  },
  {
    path: 'teste-webhook',
    component: TesteWebhookComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RegrasCobrancaRoutingModule { }
