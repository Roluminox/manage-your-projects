import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { vi } from 'vitest';
import { SnippetFiltersComponent } from './snippet-filters.component';
import { SnippetFilters, Tag } from '../../models/snippet.models';

@Component({
  standalone: true,
  imports: [SnippetFiltersComponent],
  template: `
    <app-snippet-filters
      [filters]="filters"
      [tags]="tags"
      (filtersChange)="onFiltersChange($event)"
      (resetFilters)="onResetFilters()"
    />
  `
})
class TestHostComponent {
  filters: SnippetFilters = {
    page: 1,
    pageSize: 10,
    sortDescending: true
  };
  tags: Tag[] = [
    { id: 'tag-1', name: 'JavaScript', color: '#f7df1e' },
    { id: 'tag-2', name: 'TypeScript', color: '#3178c6' }
  ];
  lastFiltersChange: Partial<SnippetFilters> | null = null;
  resetCount = 0;

  onFiltersChange(filters: Partial<SnippetFilters>): void {
    this.lastFiltersChange = filters;
  }

  onResetFilters(): void {
    this.resetCount++;
  }
}

describe('SnippetFiltersComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(async () => {
    vi.useFakeTimers();

    await TestBed.configureTestingModule({
      imports: [TestHostComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should create', () => {
    const container = fixture.nativeElement.querySelector('.filters-container');
    expect(container).toBeTruthy();
  });

  it('should display search input', () => {
    const input = fixture.nativeElement.querySelector('.search-input');
    expect(input).toBeTruthy();
  });

  it('should emit filtersChange with debounce when search changes', async () => {
    const input = fixture.nativeElement.querySelector('.search-input') as HTMLInputElement;
    input.value = 'test';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(host.lastFiltersChange).toBeNull();

    vi.advanceTimersByTime(300);

    expect(host.lastFiltersChange).toEqual({ searchTerm: 'test' });
  });

  it('should emit filtersChange when language changes', () => {
    const select = fixture.nativeElement.querySelector('.filter-select') as HTMLSelectElement;
    select.value = 'typescript';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(host.lastFiltersChange).toEqual({ language: 'typescript' });
  });

  it('should display tags in tag filter', () => {
    const selects = fixture.nativeElement.querySelectorAll('.filter-select');
    const tagSelect = selects[1] as HTMLSelectElement;
    const options = tagSelect.querySelectorAll('option');

    expect(options.length).toBe(3); // "All Tags" + 2 tags
  });

  it('should not show reset button when no filters active', () => {
    const resetBtn = fixture.nativeElement.querySelector('.reset-btn');
    expect(resetBtn).toBeFalsy();
  });

  it('should show reset button when filters are active', () => {
    host.filters = { ...host.filters, language: 'javascript' };
    fixture.detectChanges();

    const resetBtn = fixture.nativeElement.querySelector('.reset-btn');
    expect(resetBtn).toBeTruthy();
  });

  it('should emit resetFilters when reset button clicked', () => {
    host.filters = { ...host.filters, language: 'javascript' };
    fixture.detectChanges();

    const resetBtn = fixture.nativeElement.querySelector('.reset-btn') as HTMLButtonElement;
    resetBtn.click();

    expect(host.resetCount).toBe(1);
  });

  it('should show clear button when search term exists', () => {
    host.filters = { ...host.filters, searchTerm: 'test' };
    fixture.detectChanges();

    const clearBtn = fixture.nativeElement.querySelector('.clear-btn');
    expect(clearBtn).toBeTruthy();
  });
});
