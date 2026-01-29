import { Routes } from '@angular/router';

export const KANBAN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/project-list/project-list.component').then(
        (m) => m.ProjectListComponent
      ),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/board/board.component').then(
        (m) => m.BoardComponent
      ),
  },
];
