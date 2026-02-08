import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

interface DiffEntry {
  key: string;
  oldValue: unknown;
  newValue: unknown;
  type: 'added' | 'removed' | 'changed' | 'unchanged';
}

@Component({
  selector: 'app-json-diff',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (diffEntries.length > 0) {
      <div class="diff-container">
        @for (entry of diffEntries; track entry.key) {
          <div class="diff-row" [class]="'diff-' + entry.type">
            <span class="diff-key">{{ entry.key }}</span>
            @switch (entry.type) {
              @case ('added') {
                <span class="diff-badge badge-added">added</span>
                <span class="diff-value new">{{ formatValue(entry.newValue) }}</span>
              }
              @case ('removed') {
                <span class="diff-badge badge-removed">removed</span>
                <span class="diff-value old">{{ formatValue(entry.oldValue) }}</span>
              }
              @case ('changed') {
                <span class="diff-badge badge-changed">changed</span>
                <span class="diff-value old">{{ formatValue(entry.oldValue) }}</span>
                <span class="diff-arrow">-></span>
                <span class="diff-value new">{{ formatValue(entry.newValue) }}</span>
              }
            }
          </div>
        }
      </div>
    } @else {
      <span class="no-diff">No changes recorded</span>
    }
  `,
  styles: [`
    .diff-container {
      font-family: 'Fira Code', 'Cascadia Code', monospace;
      font-size: 0.8rem;
      line-height: 1.6;
    }
    .diff-row {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.25rem 0.5rem;
      border-radius: 0.25rem;
    }
    .diff-added { background: rgba(34, 197, 94, 0.08); }
    .diff-removed { background: rgba(239, 68, 68, 0.08); }
    .diff-changed { background: rgba(234, 179, 8, 0.08); }
    .diff-key {
      font-weight: 600;
      color: var(--color-text-primary, #334155);
      min-width: 120px;
    }
    .diff-badge {
      padding: 0.1rem 0.4rem;
      border-radius: 0.25rem;
      font-size: 0.7rem;
      font-weight: 600;
      text-transform: uppercase;
    }
    .badge-added { background: #dcfce7; color: #166534; }
    .badge-removed { background: #fee2e2; color: #991b1b; }
    .badge-changed { background: #fef9c3; color: #854d0e; }
    .diff-value {
      padding: 0.1rem 0.3rem;
      border-radius: 0.2rem;
    }
    .diff-value.old {
      background: #fee2e2;
      color: #991b1b;
      text-decoration: line-through;
    }
    .diff-value.new {
      background: #dcfce7;
      color: #166534;
    }
    .diff-arrow {
      color: var(--color-text-secondary, #94a3b8);
    }
    .no-diff {
      color: var(--color-text-secondary, #94a3b8);
      font-size: 0.875rem;
    }
  `]
})
export class JsonDiffComponent implements OnChanges {
  @Input() oldValues: string | null = null;
  @Input() newValues: string | null = null;

  diffEntries: DiffEntry[] = [];

  ngOnChanges(): void {
    this.diffEntries = this.computeDiff();
  }

  formatValue(value: unknown): string {
    if (value === null || value === undefined) return 'null';
    if (typeof value === 'string') return `"${value}"`;
    return String(value);
  }

  private computeDiff(): DiffEntry[] {
    const oldObj = this.parseJson(this.oldValues);
    const newObj = this.parseJson(this.newValues);

    if (!oldObj && !newObj) return [];

    const entries: DiffEntry[] = [];
    const allKeys = new Set([
      ...Object.keys(oldObj || {}),
      ...Object.keys(newObj || {})
    ]);

    for (const key of allKeys) {
      const hasOld = oldObj && key in oldObj;
      const hasNew = newObj && key in newObj;

      if (hasNew && !hasOld) {
        entries.push({ key, oldValue: undefined, newValue: newObj![key], type: 'added' });
      } else if (hasOld && !hasNew) {
        entries.push({ key, oldValue: oldObj![key], newValue: undefined, type: 'removed' });
      } else if (hasOld && hasNew && JSON.stringify(oldObj![key]) !== JSON.stringify(newObj![key])) {
        entries.push({ key, oldValue: oldObj![key], newValue: newObj![key], type: 'changed' });
      }
    }

    return entries;
  }

  private parseJson(value: string | null): Record<string, unknown> | null {
    if (!value) return null;
    try {
      return JSON.parse(value);
    } catch {
      return null;
    }
  }
}
