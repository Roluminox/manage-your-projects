import { Component, input, output } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { SnippetSummary } from '../../models/snippet.models';
import { TagBadgeComponent } from '../../../../shared/components/tag-badge/tag-badge.component';

@Component({
  selector: 'app-snippet-card',
  standalone: true,
  imports: [CommonModule, DatePipe, TagBadgeComponent],
  template: `
    <article class="snippet-card" (click)="cardClick.emit()">
      <div class="card-header">
        <h3 class="card-title">{{ snippet().title }}</h3>
        <button
          type="button"
          class="favorite-btn"
          [class.is-favorite]="snippet().isFavorite"
          (click)="onFavoriteClick($event)"
          [attr.aria-label]="snippet().isFavorite ? 'Remove from favorites' : 'Add to favorites'"
        >
          @if (snippet().isFavorite) {
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="icon">
              <path d="M11.645 20.91l-.007-.003-.022-.012a15.247 15.247 0 01-.383-.218 25.18 25.18 0 01-4.244-3.17C4.688 15.36 2.25 12.174 2.25 8.25 2.25 5.322 4.714 3 7.688 3A5.5 5.5 0 0112 5.052 5.5 5.5 0 0116.313 3c2.973 0 5.437 2.322 5.437 5.25 0 3.925-2.438 7.111-4.739 9.256a25.175 25.175 0 01-4.244 3.17 15.247 15.247 0 01-.383.219l-.022.012-.007.004-.003.001a.752.752 0 01-.704 0l-.003-.001z" />
            </svg>
          } @else {
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="icon">
              <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z" />
            </svg>
          }
        </button>
      </div>

      <div class="card-meta">
        <span class="language-badge">{{ snippet().language }}</span>
        <span class="date">{{ snippet().createdAt | date:'shortDate' }}</span>
      </div>

      @if (snippet().tags.length > 0) {
        <div class="card-tags">
          @for (tag of snippet().tags; track tag.id) {
            <app-tag-badge [name]="tag.name" [color]="tag.color" />
          }
        </div>
      }
    </article>
  `,
  styles: [`
    .snippet-card {
      background: var(--bg-secondary, #f8fafc);
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.5rem;
      padding: 1rem;
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .snippet-card:hover {
      border-color: var(--primary, #6366f1);
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 0.5rem;
      margin-bottom: 0.5rem;
    }

    .card-title {
      font-size: 1rem;
      font-weight: 600;
      margin: 0;
      line-height: 1.4;
      color: var(--text-primary, #1e293b);
      overflow: hidden;
      text-overflow: ellipsis;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
    }

    .favorite-btn {
      flex-shrink: 0;
      padding: 0.25rem;
      border: none;
      background: transparent;
      cursor: pointer;
      color: var(--text-muted, #94a3b8);
      transition: color 0.15s ease;
    }

    .favorite-btn:hover {
      color: var(--danger, #ef4444);
    }

    .favorite-btn.is-favorite {
      color: var(--danger, #ef4444);
    }

    .icon {
      width: 1.25rem;
      height: 1.25rem;
    }

    .card-meta {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 0.75rem;
      font-size: 0.75rem;
      color: var(--text-muted, #94a3b8);
    }

    .language-badge {
      background: var(--primary-light, #eef2ff);
      color: var(--primary, #6366f1);
      padding: 0.125rem 0.5rem;
      border-radius: 0.25rem;
      font-weight: 500;
      text-transform: uppercase;
      font-size: 0.625rem;
      letter-spacing: 0.05em;
    }

    .card-tags {
      display: flex;
      flex-wrap: wrap;
      gap: 0.25rem;
    }
  `]
})
export class SnippetCardComponent {
  readonly snippet = input.required<SnippetSummary>();

  readonly cardClick = output<void>();
  readonly favoriteClick = output<void>();

  onFavoriteClick(event: Event): void {
    event.stopPropagation();
    this.favoriteClick.emit();
  }
}
