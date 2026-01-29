export interface Tag {
  id: string;
  name: string;
  color: string;
}

export interface Snippet {
  id: string;
  title: string;
  code: string;
  language: string;
  description?: string;
  isFavorite: boolean;
  createdAt: string;
  updatedAt?: string;
  tags: Tag[];
}

export interface SnippetSummary {
  id: string;
  title: string;
  language: string;
  isFavorite: boolean;
  createdAt: string;
  tags: Tag[];
}

export interface SnippetListResponse {
  items: SnippetSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateSnippetRequest {
  title: string;
  code: string;
  language: string;
  description?: string;
  tagIds?: string[];
}

export interface UpdateSnippetRequest {
  title: string;
  code: string;
  language: string;
  description?: string;
  tagIds?: string[];
}

export interface CreateTagRequest {
  name: string;
  color?: string;
}

export interface SnippetFilters {
  page: number;
  pageSize: number;
  language?: string;
  tagId?: string;
  isFavorite?: boolean;
  sortBy?: string;
  sortDescending: boolean;
  searchTerm?: string;
}

export const SUPPORTED_LANGUAGES = [
  { value: 'csharp', label: 'C#' },
  { value: 'typescript', label: 'TypeScript' },
  { value: 'javascript', label: 'JavaScript' },
  { value: 'sql', label: 'SQL' },
  { value: 'python', label: 'Python' },
  { value: 'bash', label: 'Bash' },
  { value: 'json', label: 'JSON' },
  { value: 'yaml', label: 'YAML' },
  { value: 'html', label: 'HTML' },
  { value: 'css', label: 'CSS' },
  { value: 'go', label: 'Go' },
  { value: 'rust', label: 'Rust' },
  { value: 'java', label: 'Java' },
  { value: 'xml', label: 'XML' },
  { value: 'markdown', label: 'Markdown' },
  { value: 'plaintext', label: 'Plain Text' },
] as const;

export type SupportedLanguage = typeof SUPPORTED_LANGUAGES[number]['value'];
