import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SnippetCardComponent } from './snippet-card.component';
import { SnippetSummary } from '../../models/snippet.models';

describe('SnippetCardComponent', () => {
  let component: SnippetCardComponent;
  let fixture: ComponentFixture<SnippetCardComponent>;

  const mockSnippet: SnippetSummary = {
    id: 'snippet-1',
    title: 'Test Snippet',
    language: 'javascript',
    isFavorite: false,
    createdAt: '2024-01-01T00:00:00Z',
    tags: [{ id: 'tag-1', name: 'JS', color: '#f7df1e' }]
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SnippetCardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(SnippetCardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.componentRef.setInput('snippet', mockSnippet);
    fixture.detectChanges();

    const card = fixture.nativeElement.querySelector('.snippet-card');
    expect(card).toBeTruthy();
  });

  it('should display snippet title', () => {
    fixture.componentRef.setInput('snippet', mockSnippet);
    fixture.detectChanges();

    const title = fixture.nativeElement.querySelector('.card-title');
    expect(title.textContent).toContain('Test Snippet');
  });

  it('should display snippet language', () => {
    fixture.componentRef.setInput('snippet', mockSnippet);
    fixture.detectChanges();

    const language = fixture.nativeElement.querySelector('.language-badge');
    expect(language.textContent).toContain('javascript');
  });

  it('should display tags', () => {
    fixture.componentRef.setInput('snippet', mockSnippet);
    fixture.detectChanges();

    const tags = fixture.nativeElement.querySelectorAll('app-tag-badge');
    expect(tags.length).toBe(1);
  });

  it('should emit cardClick when card is clicked', () => {
    fixture.componentRef.setInput('snippet', mockSnippet);
    fixture.detectChanges();

    let emitted = false;
    component.cardClick.subscribe(() => {
      emitted = true;
    });

    const card = fixture.nativeElement.querySelector('.snippet-card') as HTMLElement;
    card.click();

    expect(emitted).toBe(true);
  });

  it('should emit favoriteClick when favorite button is clicked', () => {
    fixture.componentRef.setInput('snippet', mockSnippet);
    fixture.detectChanges();

    let emitted = false;
    component.favoriteClick.subscribe(() => {
      emitted = true;
    });

    const favoriteBtn = fixture.nativeElement.querySelector('.favorite-btn') as HTMLButtonElement;
    favoriteBtn.click();

    expect(emitted).toBe(true);
  });

  it('should not emit cardClick when favorite button is clicked', () => {
    fixture.componentRef.setInput('snippet', mockSnippet);
    fixture.detectChanges();

    let cardClicked = false;
    component.cardClick.subscribe(() => {
      cardClicked = true;
    });

    const favoriteBtn = fixture.nativeElement.querySelector('.favorite-btn') as HTMLButtonElement;
    favoriteBtn.click();

    expect(cardClicked).toBe(false);
  });

  it('should show filled heart when snippet is favorite', () => {
    fixture.componentRef.setInput('snippet', { ...mockSnippet, isFavorite: true });
    fixture.detectChanges();

    const favoriteBtn = fixture.nativeElement.querySelector('.favorite-btn');
    expect(favoriteBtn.classList.contains('is-favorite')).toBe(true);
  });

  it('should show outline heart when snippet is not favorite', () => {
    fixture.componentRef.setInput('snippet', { ...mockSnippet, isFavorite: false });
    fixture.detectChanges();

    const favoriteBtn = fixture.nativeElement.querySelector('.favorite-btn');
    expect(favoriteBtn.classList.contains('is-favorite')).toBe(false);
  });
});
