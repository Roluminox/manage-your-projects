import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SnippetFormComponent } from './snippet-form.component';
import { Snippet, Tag } from '../../models/snippet.models';

describe('SnippetFormComponent', () => {
  let component: SnippetFormComponent;
  let fixture: ComponentFixture<SnippetFormComponent>;

  const mockTags: Tag[] = [
    { id: 'tag-1', name: 'JavaScript', color: '#f7df1e' },
    { id: 'tag-2', name: 'TypeScript', color: '#3178c6' }
  ];

  const mockSnippet: Snippet = {
    id: 'snippet-1',
    title: 'Test Snippet',
    code: 'console.log("test");',
    language: 'javascript',
    description: 'A test',
    isFavorite: false,
    createdAt: '2024-01-01',
    tags: [{ id: 'tag-1', name: 'JavaScript', color: '#f7df1e' }]
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SnippetFormComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(SnippetFormComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.detectChanges();

    const form = fixture.nativeElement.querySelector('.snippet-form');
    expect(form).toBeTruthy();
  });

  it('should display all form fields', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#title')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#language')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#code')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#description')).toBeTruthy();
  });

  it('should display available tags', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.detectChanges();

    const tagOptions = fixture.nativeElement.querySelectorAll('.tag-option');
    expect(tagOptions.length).toBe(2);
  });

  it('should show "Create Snippet" button for new snippet', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.componentRef.setInput('snippet', null);
    fixture.detectChanges();

    const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(submitBtn.textContent).toContain('Create');
  });

  it('should show "Update Snippet" button when editing', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.componentRef.setInput('snippet', mockSnippet);
    fixture.detectChanges();

    const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(submitBtn.textContent).toContain('Update');
  });

  it('should populate form when snippet is provided', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.componentRef.setInput('snippet', mockSnippet);
    fixture.detectChanges();

    const titleInput = fixture.nativeElement.querySelector('#title') as HTMLInputElement;
    const codeTextarea = fixture.nativeElement.querySelector('#code') as HTMLTextAreaElement;

    expect(titleInput.value).toBe('Test Snippet');
    expect(codeTextarea.value).toBe('console.log("test");');
  });

  it('should toggle tag selection', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.detectChanges();

    const tagOptions = fixture.nativeElement.querySelectorAll('.tag-option');
    const firstTag = tagOptions[0] as HTMLButtonElement;

    firstTag.click();
    fixture.detectChanges();
    expect(firstTag.classList.contains('selected')).toBe(true);

    firstTag.click();
    fixture.detectChanges();
    expect(firstTag.classList.contains('selected')).toBe(false);
  });

  it('should emit cancel when cancel button clicked', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.detectChanges();

    let emitted = false;
    component.cancel.subscribe(() => {
      emitted = true;
    });

    const cancelBtn = fixture.nativeElement.querySelector('.btn-secondary') as HTMLButtonElement;
    cancelBtn.click();

    expect(emitted).toBe(true);
  });

  it('should disable submit button when form is invalid', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.detectChanges();

    const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]') as HTMLButtonElement;
    expect(submitBtn.disabled).toBe(true);
  });

  it('should disable submit button when loading', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.componentRef.setInput('loading', false);
    fixture.detectChanges();

    const titleInput = fixture.nativeElement.querySelector('#title') as HTMLInputElement;
    const languageSelect = fixture.nativeElement.querySelector('#language') as HTMLSelectElement;
    const codeTextarea = fixture.nativeElement.querySelector('#code') as HTMLTextAreaElement;

    titleInput.value = 'Test';
    titleInput.dispatchEvent(new Event('input'));
    languageSelect.value = 'javascript';
    languageSelect.dispatchEvent(new Event('change'));
    codeTextarea.value = 'const x = 1;';
    codeTextarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();

    const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]') as HTMLButtonElement;
    expect(submitBtn.disabled).toBe(true);
  });

  it('should show "Saving..." when loading', () => {
    fixture.componentRef.setInput('availableTags', mockTags);
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();

    const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(submitBtn.textContent).toContain('Saving...');
  });
});
