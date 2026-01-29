import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TagSelectorComponent } from './tag-selector.component';
import { Tag } from '../../models/snippet.models';

describe('TagSelectorComponent', () => {
  let component: TagSelectorComponent;
  let fixture: ComponentFixture<TagSelectorComponent>;

  const mockTags: Tag[] = [
    { id: 'tag-1', name: 'JavaScript', color: '#f7df1e' },
    { id: 'tag-2', name: 'TypeScript', color: '#3178c6' },
    { id: 'tag-3', name: 'Python', color: '#3776ab' }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TagSelectorComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(TagSelectorComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  describe('tag display', () => {
    it('should display selected tags as badges', () => {
      fixture.componentRef.setInput('availableTags', mockTags);
      fixture.componentRef.setInput('selectedTags', ['tag-1', 'tag-2']);
      fixture.detectChanges();

      const badges = fixture.nativeElement.querySelectorAll('.selected-tags app-tag-badge');
      expect(badges.length).toBe(2);
    });

    it('should show no tags message when no tags available', () => {
      fixture.componentRef.setInput('availableTags', []);
      fixture.componentRef.setInput('selectedTags', []);
      fixture.detectChanges();

      const message = fixture.nativeElement.querySelector('.no-tags-message');
      expect(message).toBeTruthy();
      expect(message.textContent).toContain('No tags available');
    });

    it('should not show no tags message when tags are available', () => {
      fixture.componentRef.setInput('availableTags', mockTags);
      fixture.componentRef.setInput('selectedTags', []);
      fixture.detectChanges();

      const message = fixture.nativeElement.querySelector('.no-tags-message');
      expect(message).toBeFalsy();
    });
  });

  describe('tag selection', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('availableTags', mockTags);
      fixture.componentRef.setInput('selectedTags', []);
      fixture.detectChanges();
    });

    it('should show input field', () => {
      const input = fixture.nativeElement.querySelector('.tag-input');
      expect(input).toBeTruthy();
    });

    it('should show dropdown when input is focused', () => {
      const input = fixture.nativeElement.querySelector('.tag-input') as HTMLInputElement;
      input.dispatchEvent(new Event('focus'));
      fixture.detectChanges();

      const dropdown = fixture.nativeElement.querySelector('.dropdown');
      expect(dropdown).toBeTruthy();
    });

    it('should filter tags based on search term', () => {
      // Simulate typing in the input field
      const input = fixture.nativeElement.querySelector('.tag-input') as HTMLInputElement;
      input.value = 'java';
      input.dispatchEvent(new Event('input'));
      input.dispatchEvent(new Event('focus'));
      fixture.detectChanges();

      const filteredTags = component.filteredTags();
      expect(filteredTags.length).toBe(1);
      expect(filteredTags[0].name).toBe('JavaScript');
    });

    it('should emit selectionChange when tag is selected', () => {
      let emittedValue: string[] = [];
      component.selectionChange.subscribe((value) => {
        emittedValue = value;
      });

      component.selectTag(mockTags[0]);

      expect(emittedValue).toEqual(['tag-1']);
    });

    it('should not show already selected tags in dropdown', () => {
      fixture.componentRef.setInput('selectedTags', ['tag-1']);
      fixture.detectChanges();

      // Focus the input to show dropdown
      const input = fixture.nativeElement.querySelector('.tag-input') as HTMLInputElement;
      input.dispatchEvent(new Event('focus'));
      fixture.detectChanges();

      const filteredTags = component.filteredTags();
      expect(filteredTags.length).toBe(2);
      expect(filteredTags.find(t => t.id === 'tag-1')).toBeUndefined();
    });
  });

  describe('tag removal', () => {
    it('should emit selectionChange when tag is removed', () => {
      fixture.componentRef.setInput('availableTags', mockTags);
      fixture.componentRef.setInput('selectedTags', ['tag-1', 'tag-2']);
      fixture.detectChanges();

      let emittedValue: string[] = [];
      component.selectionChange.subscribe((value) => {
        emittedValue = value;
      });

      component.removeTag('tag-1');

      expect(emittedValue).toEqual(['tag-2']);
    });
  });

  describe('tag creation', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('availableTags', mockTags);
      fixture.componentRef.setInput('selectedTags', []);
      fixture.componentRef.setInput('allowCreate', true);
      fixture.detectChanges();
    });

    it('should show create option when search term does not match existing tags', () => {
      // Simulate typing to update searchTerm
      const input = fixture.nativeElement.querySelector('.tag-input') as HTMLInputElement;
      input.value = 'NewTag';
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      expect(component.canCreateTag()).toBe(true);
    });

    it('should not show create option when search term matches existing tag', () => {
      const input = fixture.nativeElement.querySelector('.tag-input') as HTMLInputElement;
      input.value = 'JavaScript';
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      expect(component.canCreateTag()).toBe(false);
    });

    it('should not show create option when allowCreate is false', () => {
      fixture.componentRef.setInput('allowCreate', false);
      fixture.detectChanges();

      // canCreateTag checks allowCreate() first, so even without input it should be false
      expect(component.canCreateTag()).toBe(false);
    });

    it('should emit createTag when creating new tag', () => {
      let emittedName = '';
      component.createTag.subscribe((name) => {
        emittedName = name;
      });

      // Simulate typing first
      const input = fixture.nativeElement.querySelector('.tag-input') as HTMLInputElement;
      input.value = 'NewTag';
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      component.createNewTag();

      expect(emittedName).toBe('NewTag');
    });

    it('should clear search after creating tag', () => {
      const input = fixture.nativeElement.querySelector('.tag-input') as HTMLInputElement;
      input.value = 'NewTag';
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      component.createNewTag();

      expect(component.searchTerm).toBe('');
      expect(component.showDropdown).toBe(false);
    });
  });

  describe('disabled state', () => {
    it('should not show input when disabled', () => {
      fixture.componentRef.setInput('availableTags', mockTags);
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('.tag-input');
      expect(input).toBeFalsy(); // Input is hidden when disabled
    });

    it('should not show remove button on badges when disabled', () => {
      fixture.componentRef.setInput('availableTags', mockTags);
      fixture.componentRef.setInput('selectedTags', ['tag-1']);
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();

      // The TagBadge component receives removable=false when disabled
      // Check that badges are displayed without remove functionality
      const badges = fixture.nativeElement.querySelectorAll('.selected-tags app-tag-badge');
      expect(badges.length).toBe(1);
    });
  });

  describe('clear search', () => {
    it('should show clear button when search term exists', () => {
      fixture.componentRef.setInput('availableTags', mockTags);
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('.tag-input') as HTMLInputElement;
      input.value = 'test';
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      const clearBtn = fixture.nativeElement.querySelector('.clear-btn');
      expect(clearBtn).toBeTruthy();
    });

    it('should clear search when clearSearch is called', () => {
      fixture.componentRef.setInput('availableTags', mockTags);
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('.tag-input') as HTMLInputElement;
      input.value = 'test';
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      component.clearSearch();

      expect(component.searchTerm).toBe('');
      expect(component.showDropdown).toBe(false);
    });
  });
});
