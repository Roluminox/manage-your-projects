import { Injectable, computed, inject, signal } from '@angular/core';
import { catchError, finalize, of, tap } from 'rxjs';
import {
  AddChecklistItemRequest,
  Column,
  CreateColumnRequest,
  CreateProjectRequest,
  CreateTaskRequest,
  MoveTaskRequest,
  Project,
  ProjectSummary,
  ReorderColumnsRequest,
  ReorderTasksRequest,
  TaskItem,
  UpdateColumnRequest,
  UpdateProjectRequest,
  UpdateTaskRequest
} from '../models/kanban.models';
import { KanbanApiService } from './kanban-api.service';

interface KanbanState {
  projects: ProjectSummary[];
  selectedProject: Project | null;
  selectedTask: TaskItem | null;
  loading: boolean;
  error: string | null;
}

const initialState: KanbanState = {
  projects: [],
  selectedProject: null,
  selectedTask: null,
  loading: false,
  error: null
};

@Injectable({
  providedIn: 'root'
})
export class KanbanStateService {
  private readonly api = inject(KanbanApiService);

  private readonly state = signal<KanbanState>(initialState);

  readonly projects = computed(() => this.state().projects);
  readonly selectedProject = computed(() => this.state().selectedProject);
  readonly selectedTask = computed(() => this.state().selectedTask);
  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);

  readonly columns = computed(() => this.state().selectedProject?.columns ?? []);

  // Projects
  loadProjects(): void {
    this.updateState({ loading: true, error: null });

    this.api.getProjects().pipe(
      tap(projects => this.updateState({ projects })),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to load projects' });
        return of([]);
      }),
      finalize(() => this.updateState({ loading: false }))
    ).subscribe();
  }

  loadProjectById(id: string): void {
    this.updateState({ loading: true, error: null, selectedProject: null });

    this.api.getProjectById(id).pipe(
      tap(project => this.updateState({ selectedProject: project })),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to load project' });
        return of(null);
      }),
      finalize(() => this.updateState({ loading: false }))
    ).subscribe();
  }

  createProject(request: CreateProjectRequest): void {
    this.updateState({ loading: true, error: null });

    this.api.createProject(request).pipe(
      tap(() => this.loadProjects()),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to create project', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  updateProject(id: string, request: UpdateProjectRequest): void {
    this.updateState({ loading: true, error: null });

    this.api.updateProject(id, request).pipe(
      tap(project => {
        this.updateState({ selectedProject: project });
        this.loadProjects();
      }),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to update project', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  deleteProject(id: string): void {
    this.updateState({ loading: true, error: null });

    this.api.deleteProject(id).pipe(
      tap(() => {
        this.updateState({ selectedProject: null });
        this.loadProjects();
      }),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to delete project', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  // Columns
  createColumn(projectId: string, request: CreateColumnRequest): void {
    this.updateState({ loading: true, error: null });

    this.api.createColumn(projectId, request).pipe(
      tap(() => this.loadProjectById(projectId)),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to create column', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  updateColumn(id: string, request: UpdateColumnRequest): void {
    const projectId = this.state().selectedProject?.id;
    if (!projectId) return;

    this.api.updateColumn(id, request).pipe(
      tap(() => this.loadProjectById(projectId)),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to update column' });
        return of(null);
      })
    ).subscribe();
  }

  deleteColumn(id: string): void {
    const projectId = this.state().selectedProject?.id;
    if (!projectId) return;

    this.updateState({ loading: true, error: null });

    this.api.deleteColumn(id).pipe(
      tap(() => this.loadProjectById(projectId)),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to delete column', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  reorderColumns(projectId: string, columnIds: string[]): void {
    const currentProject = this.state().selectedProject;
    if (!currentProject) return;

    // Optimistic update
    const reorderedColumns = columnIds
      .map((id, index) => {
        const column = currentProject.columns.find(c => c.id === id);
        return column ? { ...column, order: index } : null;
      })
      .filter((c): c is Column => c !== null);

    this.updateState({
      selectedProject: { ...currentProject, columns: reorderedColumns }
    });

    const request: ReorderColumnsRequest = { columnIds };

    this.api.reorderColumns(projectId, request).pipe(
      catchError(error => {
        // Rollback on error
        this.updateState({ selectedProject: currentProject, error: error.error?.errors?.[0] || 'Failed to reorder columns' });
        return of(null);
      })
    ).subscribe();
  }

  // Tasks
  loadTaskById(id: string): void {
    this.updateState({ loading: true, error: null, selectedTask: null });

    this.api.getTaskById(id).pipe(
      tap(task => this.updateState({ selectedTask: task })),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to load task' });
        return of(null);
      }),
      finalize(() => this.updateState({ loading: false }))
    ).subscribe();
  }

  createTask(columnId: string, request: CreateTaskRequest): void {
    const projectId = this.state().selectedProject?.id;
    if (!projectId) return;

    this.updateState({ loading: true, error: null });

    this.api.createTask(columnId, request).pipe(
      tap(() => this.loadProjectById(projectId)),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to create task', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  updateTask(id: string, request: UpdateTaskRequest): void {
    const projectId = this.state().selectedProject?.id;
    if (!projectId) return;

    this.updateState({ loading: true, error: null });

    this.api.updateTask(id, request).pipe(
      tap(task => {
        this.updateState({ selectedTask: task });
        this.loadProjectById(projectId);
      }),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to update task', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  deleteTask(id: string): void {
    const projectId = this.state().selectedProject?.id;
    if (!projectId) return;

    this.updateState({ loading: true, error: null });

    this.api.deleteTask(id).pipe(
      tap(() => {
        this.updateState({ selectedTask: null });
        this.loadProjectById(projectId);
      }),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to delete task', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  moveTask(taskId: string, targetColumnId: string, newOrder: number): void {
    const currentProject = this.state().selectedProject;
    if (!currentProject) return;

    // Find source column and task
    let sourceColumn: Column | undefined;
    let task: TaskItem | undefined;

    for (const col of currentProject.columns) {
      const foundTask = col.tasks.find(t => t.id === taskId);
      if (foundTask) {
        sourceColumn = col;
        task = foundTask;
        break;
      }
    }

    if (!sourceColumn || !task) return;

    // Optimistic update
    const updatedColumns = currentProject.columns.map(col => {
      if (col.id === sourceColumn!.id && col.id !== targetColumnId) {
        // Remove from source
        return {
          ...col,
          tasks: col.tasks.filter(t => t.id !== taskId)
        };
      }
      if (col.id === targetColumnId) {
        // Add to target
        const newTasks = col.id === sourceColumn!.id
          ? col.tasks.filter(t => t.id !== taskId)
          : [...col.tasks];

        newTasks.splice(newOrder, 0, { ...task!, order: newOrder });

        return {
          ...col,
          tasks: newTasks.map((t, i) => ({ ...t, order: i }))
        };
      }
      return col;
    });

    this.updateState({
      selectedProject: { ...currentProject, columns: updatedColumns }
    });

    const request: MoveTaskRequest = { targetColumnId, newOrder };

    this.api.moveTask(taskId, request).pipe(
      catchError(error => {
        // Rollback on error
        this.updateState({ selectedProject: currentProject, error: error.error?.errors?.[0] || 'Failed to move task' });
        return of(null);
      })
    ).subscribe();
  }

  reorderTasks(columnId: string, taskIds: string[]): void {
    const currentProject = this.state().selectedProject;
    if (!currentProject) return;

    // Optimistic update
    const updatedColumns = currentProject.columns.map(col => {
      if (col.id === columnId) {
        const reorderedTasks = taskIds
          .map((id, index) => {
            const task = col.tasks.find(t => t.id === id);
            return task ? { ...task, order: index } : null;
          })
          .filter((t): t is TaskItem => t !== null);

        return { ...col, tasks: reorderedTasks };
      }
      return col;
    });

    this.updateState({
      selectedProject: { ...currentProject, columns: updatedColumns }
    });

    const request: ReorderTasksRequest = { taskIds };

    this.api.reorderTasks(columnId, request).pipe(
      catchError(error => {
        // Rollback on error
        this.updateState({ selectedProject: currentProject, error: error.error?.errors?.[0] || 'Failed to reorder tasks' });
        return of(null);
      })
    ).subscribe();
  }

  archiveTask(id: string, archive: boolean): void {
    const projectId = this.state().selectedProject?.id;
    if (!projectId) return;

    this.api.archiveTask(id, { archive }).pipe(
      tap(() => this.loadProjectById(projectId)),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to archive task' });
        return of(null);
      })
    ).subscribe();
  }

  // Checklists
  addChecklistItem(taskId: string, request: AddChecklistItemRequest): void {
    this.api.addChecklistItem(taskId, request).pipe(
      tap(() => this.loadTaskById(taskId)),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to add checklist item' });
        return of(null);
      })
    ).subscribe();
  }

  toggleChecklistItem(id: string): void {
    const task = this.state().selectedTask;
    if (!task) return;

    // Optimistic update
    const updatedChecklists = task.checklists.map(item =>
      item.id === id ? { ...item, isCompleted: !item.isCompleted } : item
    );

    this.updateState({
      selectedTask: { ...task, checklists: updatedChecklists }
    });

    this.api.toggleChecklistItem(id).pipe(
      catchError(error => {
        // Rollback on error
        this.updateState({ selectedTask: task, error: error.error?.errors?.[0] || 'Failed to toggle checklist item' });
        return of(null);
      })
    ).subscribe();
  }

  deleteChecklistItem(id: string): void {
    const task = this.state().selectedTask;
    if (!task) return;

    // Optimistic update
    const updatedChecklists = task.checklists.filter(item => item.id !== id);

    this.updateState({
      selectedTask: { ...task, checklists: updatedChecklists }
    });

    this.api.deleteChecklistItem(id).pipe(
      catchError(error => {
        // Rollback on error
        this.updateState({ selectedTask: task, error: error.error?.errors?.[0] || 'Failed to delete checklist item' });
        return of(null);
      })
    ).subscribe();
  }

  // Utility methods
  clearSelectedProject(): void {
    this.updateState({ selectedProject: null });
  }

  clearSelectedTask(): void {
    this.updateState({ selectedTask: null });
  }

  clearError(): void {
    this.updateState({ error: null });
  }

  private updateState(partialState: Partial<KanbanState>): void {
    this.state.update(state => ({ ...state, ...partialState }));
  }
}
