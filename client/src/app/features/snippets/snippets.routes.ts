import { Routes } from '@angular/router';

export const SNIPPETS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/snippet-list/snippet-list.component').then(
        (m) => m.SnippetListComponent
      ),
  },
];
