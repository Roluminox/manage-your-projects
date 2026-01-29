import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { ProjectListComponent } from './project-list.component';
import { KanbanStateService } from '../../services/kanban-state.service';
import { ProjectSummary, Project, TaskItem, Column } from '../../models/kanban.models';

describe('ProjectListComponent', () => {
  let component: ProjectListComponent;
  let fixture: ComponentFixture<ProjectListComponent>;
  let stateServiceMock: Partial<KanbanStateService>;
  let routerMock: Partial<Router>;

  const mockProjectSummary: ProjectSummary = {
    id: 'project-1',
    name: 'Test Project',
    description: 'A test project',
    color: '#6366f1',
    createdAt: '2024-01-01T00:00:00Z',
    columnCount: 3,
    taskCount: 10
  };

  beforeEach(async () => {
    stateServiceMock = {
      projects: signal<ProjectSummary[]>([]),
      selectedProject: signal<Project | null>(null),
      selectedTask: signal<TaskItem | null>(null),
      columns: signal<Column[]>([]),
      loading: signal(false),
      error: signal<string | null>(null),
      loadProjects: vi.fn(),
      loadProjectById: vi.fn(),
      createProject: vi.fn(),
      updateProject: vi.fn(),
      deleteProject: vi.fn(),
      clearSelectedProject: vi.fn(),
      clearError: vi.fn()
    };

    routerMock = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [ProjectListComponent],
      providers: [
        { provide: KanbanStateService, useValue: stateServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load projects on init', () => {
    expect(stateServiceMock.loadProjects).toHaveBeenCalled();
  });

  it('should show empty state when no projects', () => {
    const emptyState = fixture.nativeElement.querySelector('.empty-state');
    expect(emptyState).toBeTruthy();
    expect(emptyState.textContent).toContain('No projects yet');
  });

  it('should show loading state when loading', () => {
    (stateServiceMock.loading as any).set(true);
    fixture.detectChanges();

    const loadingState = fixture.nativeElement.querySelector('.loading-state');
    expect(loadingState).toBeTruthy();
  });

  it('should show error banner when error exists', () => {
    (stateServiceMock.error as any).set('Test error');
    fixture.detectChanges();

    const errorBanner = fixture.nativeElement.querySelector('.error-banner');
    expect(errorBanner).toBeTruthy();
    expect(errorBanner.textContent).toContain('Test error');
  });

  it('should display projects grid when projects exist', () => {
    (stateServiceMock.projects as any).set([mockProjectSummary]);
    fixture.detectChanges();

    const grid = fixture.nativeElement.querySelector('.projects-grid');
    expect(grid).toBeTruthy();

    const projectCard = fixture.nativeElement.querySelector('.project-card');
    expect(projectCard).toBeTruthy();
  });

  it('should display project information correctly', () => {
    (stateServiceMock.projects as any).set([mockProjectSummary]);
    fixture.detectChanges();

    const projectName = fixture.nativeElement.querySelector('.project-name');
    expect(projectName.textContent).toBe('Test Project');

    const projectDescription = fixture.nativeElement.querySelector('.project-description');
    expect(projectDescription.textContent).toBe('A test project');

    const stats = fixture.nativeElement.querySelectorAll('.stat');
    expect(stats[0].textContent).toContain('3 columns');
    expect(stats[1].textContent).toContain('10 tasks');
  });

  it('should switch to create view when new project button clicked', () => {
    const newBtn = fixture.nativeElement.querySelector('.page-header .btn-primary');
    newBtn.click();
    fixture.detectChanges();

    expect(component.viewMode()).toBe('create');
    const formTitle = fixture.nativeElement.querySelector('.form-title');
    expect(formTitle.textContent).toContain('Create New Project');
  });

  it('should navigate to project board when project card clicked', () => {
    (stateServiceMock.projects as any).set([mockProjectSummary]);
    fixture.detectChanges();

    const projectCard = fixture.nativeElement.querySelector('.project-card');
    projectCard.click();

    expect(routerMock.navigate).toHaveBeenCalledWith(['/projects', 'project-1']);
  });

  it('should clear error when close button clicked', () => {
    (stateServiceMock.error as any).set('Test error');
    fixture.detectChanges();

    const closeBtn = fixture.nativeElement.querySelector('.error-banner .close-btn');
    closeBtn.click();

    expect(stateServiceMock.clearError).toHaveBeenCalled();
  });

  it('should call createProject when form is submitted in create mode', () => {
    component.showCreateForm();
    fixture.detectChanges();

    component.form = {
      name: 'New Project',
      description: 'New description',
      color: '#8b5cf6'
    };

    component.onCreateProject();

    expect(stateServiceMock.createProject).toHaveBeenCalledWith({
      name: 'New Project',
      description: 'New description',
      color: '#8b5cf6'
    });
    expect(component.viewMode()).toBe('list');
  });

  it('should call updateProject when form is submitted in edit mode', () => {
    (stateServiceMock.projects as any).set([mockProjectSummary]);
    fixture.detectChanges();

    component.showEditForm(mockProjectSummary);
    fixture.detectChanges();

    component.form = {
      name: 'Updated Project',
      description: 'Updated description',
      color: '#ec4899'
    };

    component.onUpdateProject();

    expect(stateServiceMock.updateProject).toHaveBeenCalledWith('project-1', {
      name: 'Updated Project',
      description: 'Updated description',
      color: '#ec4899'
    });
    expect(component.viewMode()).toBe('list');
  });

  it('should call deleteProject when delete is confirmed', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    component.deleteProject('project-1');

    expect(stateServiceMock.deleteProject).toHaveBeenCalledWith('project-1');
  });

  it('should not call deleteProject when delete is cancelled', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.deleteProject('project-1');

    expect(stateServiceMock.deleteProject).not.toHaveBeenCalled();
  });

  it('should return to list when cancel is clicked', () => {
    component.showCreateForm();
    expect(component.viewMode()).toBe('create');

    component.backToList();
    expect(component.viewMode()).toBe('list');
  });

  it('should populate form when editing a project', () => {
    component.showEditForm(mockProjectSummary);

    expect(component.form.name).toBe('Test Project');
    expect(component.form.description).toBe('A test project');
    expect(component.form.color).toBe('#6366f1');
    expect(component.viewMode()).toBe('edit');
  });
});
