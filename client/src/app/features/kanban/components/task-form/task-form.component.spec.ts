import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { TaskFormComponent, TaskFormData } from './task-form.component';
import { Label, Priority, TaskItem } from '../../models/kanban.models';

describe('TaskFormComponent', () => {
  let component: TaskFormComponent;
  let fixture: ComponentFixture<TaskFormComponent>;

  const mockLabels: Label[] = [
    { id: 'label-1', name: 'Bug', color: '#ef4444' },
    { id: 'label-2', name: 'Feature', color: '#22c55e' },
    { id: 'label-3', name: 'Urgent', color: '#f97316' }
  ];

  const mockTask: TaskItem = {
    id: 'task-1',
    title: 'Existing Task',
    description: 'Task description',
    priority: Priority.High,
    dueDate: '2026-03-15T00:00:00Z',
    order: 1,
    isArchived: false,
    createdAt: '2026-01-01T00:00:00Z',
    labels: [mockLabels[0]],
    checklists: []
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskFormComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(TaskFormComponent);
    component = fixture.componentInstance;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });

    it('should initialize form with default values', () => {
      fixture.detectChanges();
      expect(component.form.value).toEqual({
        title: '',
        description: '',
        priority: Priority.Medium,
        dueDate: ''
      });
    });

    it('should display all priority options', () => {
      fixture.detectChanges();
      expect(component.priorityOptions.length).toBe(4);
      expect(component.priorityOptions.map(p => p.value)).toEqual([
        Priority.Low,
        Priority.Medium,
        Priority.High,
        Priority.Critical
      ]);
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should mark title as invalid when empty', () => {
      const titleControl = component.form.get('title');
      titleControl?.markAsTouched();
      expect(titleControl?.valid).toBe(false);
      expect(component.isFieldInvalid('title')).toBe(true);
    });

    it('should mark title as invalid when less than 3 characters', () => {
      const titleControl = component.form.get('title');
      titleControl?.setValue('ab');
      titleControl?.markAsTouched();
      expect(titleControl?.valid).toBe(false);
      expect(titleControl?.errors?.['minlength']).toBeTruthy();
    });

    it('should mark title as invalid when more than 200 characters', () => {
      const titleControl = component.form.get('title');
      titleControl?.setValue('a'.repeat(201));
      titleControl?.markAsTouched();
      expect(titleControl?.valid).toBe(false);
      expect(titleControl?.errors?.['maxlength']).toBeTruthy();
    });

    it('should mark title as valid when between 3 and 200 characters', () => {
      const titleControl = component.form.get('title');
      titleControl?.setValue('Valid title');
      expect(titleControl?.valid).toBe(true);
    });

    it('should not require description', () => {
      const descriptionControl = component.form.get('description');
      expect(descriptionControl?.valid).toBe(true);
    });

    it('should not require dueDate', () => {
      const dueDateControl = component.form.get('dueDate');
      expect(dueDateControl?.valid).toBe(true);
    });

    it('should have form invalid when title is empty', () => {
      expect(component.form.valid).toBe(false);
    });

    it('should have form valid when title is provided', () => {
      component.form.patchValue({ title: 'Valid title' });
      expect(component.form.valid).toBe(true);
    });
  });

  describe('Pre-populating with existing task', () => {
    beforeEach(() => {
      component.task = mockTask;
      component.availableLabels = mockLabels;
      fixture.detectChanges();
    });

    it('should populate form with task values', () => {
      expect(component.form.value.title).toBe('Existing Task');
      expect(component.form.value.description).toBe('Task description');
      expect(component.form.value.priority).toBe(Priority.High);
      expect(component.form.value.dueDate).toBe('2026-03-15');
    });

    it('should pre-select labels from task', () => {
      expect(component.isLabelSelected('label-1')).toBe(true);
      expect(component.isLabelSelected('label-2')).toBe(false);
      expect(component.isLabelSelected('label-3')).toBe(false);
    });
  });

  describe('Label Selection', () => {
    beforeEach(() => {
      component.availableLabels = mockLabels;
      fixture.detectChanges();
    });

    it('should toggle label selection on', () => {
      expect(component.isLabelSelected('label-1')).toBe(false);
      component.toggleLabel('label-1');
      expect(component.isLabelSelected('label-1')).toBe(true);
    });

    it('should toggle label selection off', () => {
      component.toggleLabel('label-1');
      expect(component.isLabelSelected('label-1')).toBe(true);
      component.toggleLabel('label-1');
      expect(component.isLabelSelected('label-1')).toBe(false);
    });

    it('should allow multiple labels to be selected', () => {
      component.toggleLabel('label-1');
      component.toggleLabel('label-2');
      expect(component.isLabelSelected('label-1')).toBe(true);
      expect(component.isLabelSelected('label-2')).toBe(true);
    });

    it('should render label chips when labels are available', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const labelChips = compiled.querySelectorAll('.label-chip');
      expect(labelChips.length).toBe(3);
    });

    it('should not render labels section when no labels available', async () => {
      // Need to recreate the component with empty labels to avoid ExpressionChangedAfterItHasBeenCheckedError
      const newFixture = TestBed.createComponent(TaskFormComponent);
      const newComponent = newFixture.componentInstance;
      newComponent.availableLabels = [];
      newFixture.detectChanges();

      const compiled = newFixture.nativeElement as HTMLElement;
      const labelsContainer = compiled.querySelector('.labels-container');
      expect(labelsContainer).toBeNull();
    });
  });

  describe('Form Submission', () => {
    beforeEach(() => {
      component.availableLabels = mockLabels;
      fixture.detectChanges();
    });

    it('should emit save event with form data on valid submit', () => {
      const saveSpy = vi.spyOn(component.save, 'emit');

      component.form.patchValue({
        title: 'New Task',
        description: 'Description',
        priority: Priority.High,
        dueDate: '2026-04-01'
      });
      component.toggleLabel('label-1');
      component.toggleLabel('label-2');

      component.onSubmit();

      expect(saveSpy).toHaveBeenCalledWith({
        title: 'New Task',
        description: 'Description',
        priority: Priority.High,
        dueDate: '2026-04-01',
        labelIds: ['label-1', 'label-2']
      } as TaskFormData);
    });

    it('should trim title and description', () => {
      const saveSpy = vi.spyOn(component.save, 'emit');

      component.form.patchValue({
        title: '  Trimmed Title  ',
        description: '  Trimmed Description  ',
        priority: Priority.Medium
      });

      component.onSubmit();

      expect(saveSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          title: 'Trimmed Title',
          description: 'Trimmed Description'
        })
      );
    });

    it('should convert empty description to undefined', () => {
      const saveSpy = vi.spyOn(component.save, 'emit');

      component.form.patchValue({
        title: 'Task Title',
        description: '   ',
        priority: Priority.Medium
      });

      component.onSubmit();

      expect(saveSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          description: undefined
        })
      );
    });

    it('should convert empty dueDate to undefined', () => {
      const saveSpy = vi.spyOn(component.save, 'emit');

      component.form.patchValue({
        title: 'Task Title',
        dueDate: ''
      });

      component.onSubmit();

      expect(saveSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          dueDate: undefined
        })
      );
    });

    it('should not emit save event when form is invalid', () => {
      const saveSpy = vi.spyOn(component.save, 'emit');

      component.form.patchValue({ title: '' });
      component.onSubmit();

      expect(saveSpy).not.toHaveBeenCalled();
    });

    it('should mark all fields as touched when submitting invalid form', () => {
      component.form.patchValue({ title: '' });
      component.onSubmit();

      expect(component.form.get('title')?.touched).toBe(true);
    });
  });

  describe('Cancel Action', () => {
    it('should emit cancel event when onCancel is called', () => {
      fixture.detectChanges();
      const cancelSpy = vi.spyOn(component.cancel, 'emit');

      component.onCancel();

      expect(cancelSpy).toHaveBeenCalled();
    });

    it('should emit cancel event when cancel button is clicked', () => {
      fixture.detectChanges();
      const cancelSpy = vi.spyOn(component.cancel, 'emit');
      const compiled = fixture.nativeElement as HTMLElement;
      const cancelButton = compiled.querySelector('.btn-secondary') as HTMLButtonElement;

      cancelButton.click();

      expect(cancelSpy).toHaveBeenCalled();
    });
  });

  describe('Submitting State', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have submitting signal initially false', () => {
      expect(component.submitting()).toBe(false);
    });

    it('should update submitting state via setSubmitting', () => {
      component.setSubmitting(true);
      expect(component.submitting()).toBe(true);

      component.setSubmitting(false);
      expect(component.submitting()).toBe(false);
    });

    it('should disable submit button when submitting', () => {
      component.form.patchValue({ title: 'Valid title' });
      fixture.detectChanges();

      const submitButton = fixture.nativeElement.querySelector('.btn-primary') as HTMLButtonElement;
      expect(submitButton.disabled).toBe(false);

      component.setSubmitting(true);
      fixture.detectChanges();

      expect(submitButton.disabled).toBe(true);
    });

    it('should show spinner when submitting', () => {
      component.form.patchValue({ title: 'Valid title' });
      component.setSubmitting(true);
      fixture.detectChanges();

      const spinner = fixture.nativeElement.querySelector('.spinner');
      expect(spinner).toBeTruthy();
    });
  });

  describe('UI Elements', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display "Create Task" button text for new task', () => {
      const submitButton = fixture.nativeElement.querySelector('.btn-primary') as HTMLButtonElement;
      expect(submitButton.textContent).toContain('Create');
    });

    it('should display "Update Task" button text for existing task', () => {
      // Need to recreate the component with a task to avoid ExpressionChangedAfterItHasBeenCheckedError
      const newFixture = TestBed.createComponent(TaskFormComponent);
      const newComponent = newFixture.componentInstance;
      newComponent.task = mockTask;
      newFixture.detectChanges();

      const submitButton = newFixture.nativeElement.querySelector('.btn-primary') as HTMLButtonElement;
      expect(submitButton.textContent).toContain('Update');
    });

    it('should display required indicator for title', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const requiredIndicator = compiled.querySelector('.required');
      expect(requiredIndicator).toBeTruthy();
    });

    it('should render title input', () => {
      const titleInput = fixture.nativeElement.querySelector('#title');
      expect(titleInput).toBeTruthy();
    });

    it('should render description textarea', () => {
      const descriptionTextarea = fixture.nativeElement.querySelector('#description');
      expect(descriptionTextarea).toBeTruthy();
    });

    it('should render priority select', () => {
      const prioritySelect = fixture.nativeElement.querySelector('#priority');
      expect(prioritySelect).toBeTruthy();
    });

    it('should render due date input', () => {
      const dueDateInput = fixture.nativeElement.querySelector('#dueDate');
      expect(dueDateInput).toBeTruthy();
    });
  });

  describe('Priority Conversion', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should convert priority string to number in form data', () => {
      const saveSpy = vi.spyOn(component.save, 'emit');

      component.form.patchValue({
        title: 'Test Task',
        priority: '2' // HTML select returns string
      });

      component.onSubmit();

      expect(saveSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          priority: Priority.High
        })
      );
    });
  });
});
