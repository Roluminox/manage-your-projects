import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { ChecklistItemComponent } from './checklist-item.component';
import { ChecklistItem } from '../../models/kanban.models';

describe('ChecklistItemComponent', () => {
  let component: ChecklistItemComponent;
  let fixture: ComponentFixture<ChecklistItemComponent>;

  const mockItem: ChecklistItem = {
    id: 'item-1',
    text: 'Test checklist item',
    isCompleted: false,
    order: 0
  };

  const mockCompletedItem: ChecklistItem = {
    id: 'item-2',
    text: 'Completed item',
    isCompleted: true,
    order: 1
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChecklistItemComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ChecklistItemComponent);
    component = fixture.componentInstance;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      component.item = mockItem;
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });
  });

  describe('Display', () => {
    it('should display item text', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.item-text');
      expect(textElement.textContent).toBe('Test checklist item');
    });

    it('should display checkbox', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const checkbox = fixture.nativeElement.querySelector('.checkbox') as HTMLInputElement;
      expect(checkbox).toBeTruthy();
      expect(checkbox.type).toBe('checkbox');
    });

    it('should display delete button', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const deleteButton = fixture.nativeElement.querySelector('.btn-delete');
      expect(deleteButton).toBeTruthy();
    });

    it('should have checkbox unchecked for incomplete item', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const checkbox = fixture.nativeElement.querySelector('.checkbox') as HTMLInputElement;
      expect(checkbox.checked).toBe(false);
    });

    it('should have checkbox checked for completed item', () => {
      component.item = mockCompletedItem;
      fixture.detectChanges();

      const checkbox = fixture.nativeElement.querySelector('.checkbox') as HTMLInputElement;
      expect(checkbox.checked).toBe(true);
    });
  });

  describe('Completed Styling', () => {
    it('should not have completed class for incomplete item', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const itemElement = fixture.nativeElement.querySelector('.checklist-item');
      expect(itemElement.classList.contains('completed')).toBe(false);
    });

    it('should have completed class for completed item', () => {
      component.item = mockCompletedItem;
      fixture.detectChanges();

      const itemElement = fixture.nativeElement.querySelector('.checklist-item');
      expect(itemElement.classList.contains('completed')).toBe(true);
    });

    it('should apply line-through style to completed item text', () => {
      component.item = mockCompletedItem;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.item-text');
      const styles = window.getComputedStyle(textElement);
      expect(styles.textDecoration).toContain('line-through');
    });
  });

  describe('Toggle Event', () => {
    it('should emit toggle event when checkbox is clicked', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const toggleSpy = vi.spyOn(component.toggle, 'emit');
      const checkbox = fixture.nativeElement.querySelector('.checkbox');
      checkbox.click();

      expect(toggleSpy).toHaveBeenCalled();
    });

    it('should emit toggle event when checkbox is changed', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const toggleSpy = vi.spyOn(component.toggle, 'emit');
      const checkbox = fixture.nativeElement.querySelector('.checkbox');
      checkbox.dispatchEvent(new Event('change'));

      expect(toggleSpy).toHaveBeenCalled();
    });

    it('should call onToggle method', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const toggleSpy = vi.spyOn(component.toggle, 'emit');
      component.onToggle();

      expect(toggleSpy).toHaveBeenCalled();
    });
  });

  describe('Delete Event', () => {
    it('should emit delete event when delete button is clicked', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const deleteSpy = vi.spyOn(component.delete, 'emit');
      const deleteButton = fixture.nativeElement.querySelector('.btn-delete');
      deleteButton.click();

      expect(deleteSpy).toHaveBeenCalled();
    });

    it('should call onDelete method', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const deleteSpy = vi.spyOn(component.delete, 'emit');
      component.onDelete();

      expect(deleteSpy).toHaveBeenCalled();
    });
  });

  describe('Accessibility', () => {
    it('should have aria-label on checkbox', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const checkbox = fixture.nativeElement.querySelector('.checkbox');
      expect(checkbox.getAttribute('aria-label')).toBe('Toggle Test checklist item');
    });

    it('should have aria-label on delete button', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const deleteButton = fixture.nativeElement.querySelector('.btn-delete');
      expect(deleteButton.getAttribute('aria-label')).toBe('Delete checklist item');
    });

    it('should have title on delete button', () => {
      component.item = mockItem;
      fixture.detectChanges();

      const deleteButton = fixture.nativeElement.querySelector('.btn-delete');
      expect(deleteButton.getAttribute('title')).toBe('Delete item');
    });
  });

  describe('Long Text Handling', () => {
    it('should handle long text gracefully', () => {
      const longTextItem: ChecklistItem = {
        id: 'item-long',
        text: 'This is a very long checklist item text that might need to wrap onto multiple lines in the UI to ensure proper display',
        isCompleted: false,
        order: 0
      };

      component.item = longTextItem;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.item-text');
      expect(textElement.textContent).toBe(longTextItem.text);
    });
  });
});
