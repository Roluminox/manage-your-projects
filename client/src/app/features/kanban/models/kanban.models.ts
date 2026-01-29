export enum Priority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export interface Label {
  id: string;
  name: string;
  color: string;
}

export interface ChecklistItem {
  id: string;
  text: string;
  isCompleted: boolean;
  order: number;
}

export interface TaskItem {
  id: string;
  title: string;
  description?: string;
  priority: Priority;
  dueDate?: string;
  order: number;
  isArchived: boolean;
  createdAt: string;
  updatedAt?: string;
  labels: Label[];
  checklists: ChecklistItem[];
}

export interface TaskItemSummary {
  id: string;
  title: string;
  priority: Priority;
  dueDate?: string;
  order: number;
  isArchived: boolean;
  labelCount: number;
  checklistTotal: number;
  checklistCompleted: number;
}

export interface Column {
  id: string;
  name: string;
  order: number;
  tasks: TaskItem[];
}

export interface Project {
  id: string;
  name: string;
  description?: string;
  color: string;
  createdAt: string;
  updatedAt?: string;
  columns: Column[];
}

export interface ProjectSummary {
  id: string;
  name: string;
  description?: string;
  color: string;
  createdAt: string;
  columnCount: number;
  taskCount: number;
}

export interface CreateProjectRequest {
  name: string;
  description?: string;
  color?: string;
}

export interface UpdateProjectRequest {
  name: string;
  description?: string;
  color?: string;
}

export interface CreateColumnRequest {
  name: string;
}

export interface UpdateColumnRequest {
  name: string;
}

export interface ReorderColumnsRequest {
  columnIds: string[];
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: Priority;
  dueDate?: string;
  labelIds?: string[];
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  priority: Priority;
  dueDate?: string;
  labelIds?: string[];
}

export interface MoveTaskRequest {
  targetColumnId: string;
  newOrder: number;
}

export interface ReorderTasksRequest {
  taskIds: string[];
}

export interface ArchiveTaskRequest {
  archive: boolean;
}

export interface AddChecklistItemRequest {
  text: string;
}

export const PRIORITY_LABELS: Record<Priority, string> = {
  [Priority.Low]: 'Low',
  [Priority.Medium]: 'Medium',
  [Priority.High]: 'High',
  [Priority.Critical]: 'Critical'
};

export const PRIORITY_COLORS: Record<Priority, string> = {
  [Priority.Low]: '#22c55e',
  [Priority.Medium]: '#eab308',
  [Priority.High]: '#f97316',
  [Priority.Critical]: '#ef4444'
};

export const DEFAULT_PROJECT_COLORS = [
  '#6366f1', // Indigo
  '#8b5cf6', // Violet
  '#ec4899', // Pink
  '#ef4444', // Red
  '#f97316', // Orange
  '#eab308', // Yellow
  '#22c55e', // Green
  '#14b8a6', // Teal
  '#0ea5e9', // Sky
  '#3b82f6', // Blue
] as const;
