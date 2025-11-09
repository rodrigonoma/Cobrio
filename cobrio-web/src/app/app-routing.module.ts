import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { MainLayoutComponent } from './shared/layout/main-layout/main-layout.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { AuthGuard } from './core/guards/auth.guard';
import { AssinaturasListComponent } from './features/assinaturas/assinaturas-list/assinaturas-list.component';
import { PlanosListComponent } from './features/planos/planos-list/planos-list.component';
import { RelatoriosComponent } from './features/relatorios/relatorios/relatorios.component';
import { PermissoesComponent } from './features/permissoes/permissoes.component';
import { RelatoriosAvancadosComponent } from './features/relatorios/relatorios-avancados/relatorios-avancados.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'assinaturas', component: AssinaturasListComponent },
      { path: 'planos', component: PlanosListComponent },
      { path: 'relatorios', component: RelatoriosAvancadosComponent },
      { path: 'permissoes', component: PermissoesComponent },
      {
        path: 'regras-cobranca',
        loadChildren: () => import('./features/regras-cobranca/regras-cobranca.module').then(m => m.RegrasCobrancaModule)
      },
      {
        path: 'usuarios',
        loadChildren: () => import('./features/usuarios/usuarios.module').then(m => m.UsuariosModule)
      },
      {
        path: 'configuracoes',
        loadChildren: () => import('./features/configuracoes/configuracoes.module').then(m => m.ConfiguracoesModule)
      },
      {
        path: 'templates',
        loadChildren: () => import('./features/templates/templates.module').then(m => m.TemplatesModule)
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
