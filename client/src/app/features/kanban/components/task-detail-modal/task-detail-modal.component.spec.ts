import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { TaskDetailModalComponent } from './task-detail-modal.component';
import { Priority, TaskItem } from '../../models/kanban.models';

describe('TaskDetailModalComponent', () => {
  let component: TaskDetailModalComponent;
  let fixture: ComponentFixture<TaskDetailModalComponent>;

  const mockTask: TaskItem = {
    id: 'task-1',
    title: 'Test Task',
    description: 'A test task description',
    priority: Priority.Medium,
    dueDate: '2024-12-31T00:00:00Z',
    order: 0,
    isArchived: false,
    createdAt: '2024-01-01T00:00:00Z',
    labels: [{ id: 'label-1', name: 'Bug', color: '#ef4444' }],
    checklists: [
      { id: 'check-1', text: 'Item 1', isCompleted: true, order: 0 },
      { id: 'check-2', text: 'Item 2', isCompleted: false, order: 1 }
    ]
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskDetailModalComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(TaskDetailModalComponent);
    component = fixture.componentInstance;
    component.task = mockTask;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display task title', () => {
    const title = fixture.nativeElement.querySelector('.task-title');
    expect(title.textContent).toBe('Test Task');
  });

  it('should display task description', () => {
    const description = fixture.nativeElement.querySelector('.description-content');
    expect(description.textContent.trim()).toBe('A test task description');
  });

  it('should display checklist items', () => {
    const items = fixture.nativeElement.querySelectorAll('.checklist-item');
    expect(items.length).toBe(2);
  });

  it('should calculate completed count correctly', () => {
    expect(component.completedCount).toBe(1);
  });

  it('should calculate progress percent correctly', () => {
    expect(component.progressPercent).toBe(50);
  });

  it('should emit close event when overlay clicked', () => {
    const closeSpy = vi.spyOn(component.close, 'emit');
    const overlay = fixture.nativeElement.querySelector('.modal-overlay');
    overlay.click();

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should emit close event when close button clicked', () => {
    const closeSpy = vi.spyOn(component.close, 'emit');
    const closeBtn = fixture.nativeElement.querySelector('.header-actions .btn-icon');
    closeBtn.click();

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should not close modal when content is clicked', () => {
    const closeSpy = vi.spyOn(component.close, 'emit');
    const content = fixture.nativeElement.querySelector('.modal-content');
    content.click();

    expect(closeSpy).not.toHaveBeenCalled();
  });

  it('should switch to title edit mode when title clicked', () => {
    const title = fixture.nativeElement.querySelector('.task-title');
    title.click();
    fixture.detectChanges();

    expect(component.editingTitle()).toBe(true);
    expect(component.editedTitle).toBe('Test Task');
  });

  it('should emit update when title is saved', () => {
    const updateSpy = vi.spyOn(component.update, 'emit');

    component.startEditTitle();
    component.editedTitle = 'Updated Title';
    component.saveTitle();

    expect(updateSpy).toHaveBeenCalledWith(expect.objectContaining({
      title: 'Updated Title'
    }));
  });

  it('should not emit update when title is unchanged', () => {
    const updateSpy = vi.spyOn(component.update, 'emit');

    component.startEditTitle();
    component.saveTitle();

    expect(updateSpy).not.toHaveBeenCalled();
  });

  it('should switch to description edit mode when clicked', () => {
    const desc = fixture.nativeElement.querySelector('.description-content');
    desc.click();
    fixture.detectChanges();

    expect(component.editingDescription()).toBe(true);
  });

  it('should emit update when priority changes', () => {
    const updateSpy = vi.spyOn(component.update, 'emit');

    component.selectedPriority = Priority.High;
    component.onPriorityChange();

    expect(updateSpy).toHaveBeenCalledWith(expect.objectContaining({
      priority: Priority.High
    }));
  });

  it('should emit update when due date changes', () => {
    const updateSpy = vi.spyOn(component.update, 'emit');

    component.selectedDueDate = '2025-01-15';
    component.onDueDateChange();

    expect(updateSpy).toHaveBeenCalledWith(expect.objectContaining({
      dueDate: '2025-01-15'
    }));
  });

  it('should emit toggleChecklistItem when checkbox is clicked', () => {
    const toggleSpy = vi.spyOn(component.toggleChecklistItem, 'emit');
    const checkbox = fixture.nativeElement.querySelector('.checkbox');
    checkbox.click();

    expect(toggleSpy).toHaveBeenCalledWith('check-1');
  });

  it('should show add checklist form when add button clicked', () => {
    component.startAddChecklistItem();
    fixture.detectChanges();

    expect(component.addingChecklistItem()).toBe(true);
    const addForm = fixture.nativeElement.querySelector('.add-form');
    expect(addForm).toBeTruthy();
  });

  it('should emit addChecklistItemEvent when item is added', () => {
    const addSpy = vi.spyOn(component.addChecklistItemEvent, 'emit');

    component.startAddChecklistItem();
    component.newChecklistText = 'New Item';
    component.addChecklistItem();

    expect(addSpy).toHaveBeenCalledWith('New Item');
    expect(component.addingChecklistItem()).toBe(false);
  });

  it('should emit archiveTask when archive button clicked', () => {
    const archiveSpy = vi.spyOn(component.archiveTask, 'emit');
    const archiveBtn = fixture.nativeElement.querySelector('.footer-left .btn-secondary');
    archiveBtn.click();

    expect(archiveSpy).toHaveBeenCalledWith(true);
  });

  it('should emit deleteTask when delete is confirmed', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    const deleteSpy = vi.spyOn(component.deleteTask, 'emit');
    const deleteBtn = fixture.nativeElement.querySelector('.btn-danger');
    deleteBtn.click();

    expect(deleteSpy).toHaveBeenCalled();
  });

  it('should not emit deleteTask when delete is cancelled', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false);
    const deleteSpy = vi.spyOn(component.deleteTask, 'emit');
    const deleteBtn = fixture.nativeElement.querySelector('.btn-danger');
    deleteBtn.click();

    expect(deleteSpy).not.toHaveBeenCalled();
  });

  it('should show archived badge when task is archived', () => {
    component.task = { ...mockTask, isArchived: true };
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.archived-badge');
    expect(badge).toBeTruthy();
    expect(badge.textContent).toContain('Archived');
  });

  it('should render nothing when task is null', () => {
    component.task = null;
    fixture.detectChanges();

    const overlay = fixture.nativeElement.querySelector('.modal-overlay');
    expect(overlay).toBeNull();
  });
});
