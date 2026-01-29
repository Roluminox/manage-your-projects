import { Component, input, output, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import hljs from 'highlight.js/lib/core';

// Import commonly used languages
import javascript from 'highlight.js/lib/languages/javascript';
import typescript from 'highlight.js/lib/languages/typescript';
import csharp from 'highlight.js/lib/languages/csharp';
import python from 'highlight.js/lib/languages/python';
import sql from 'highlight.js/lib/languages/sql';
import bash from 'highlight.js/lib/languages/bash';
import json from 'highlight.js/lib/languages/json';
import yaml from 'highlight.js/lib/languages/yaml';
import xml from 'highlight.js/lib/languages/xml';
import css from 'highlight.js/lib/languages/css';
import go from 'highlight.js/lib/languages/go';
import rust from 'highlight.js/lib/languages/rust';
import java from 'highlight.js/lib/languages/java';

// Register languages
hljs.registerLanguage('javascript', javascript);
hljs.registerLanguage('typescript', typescript);
hljs.registerLanguage('csharp', csharp);
hljs.registerLanguage('python', python);
hljs.registerLanguage('sql', sql);
hljs.registerLanguage('bash', bash);
hljs.registerLanguage('json', json);
hljs.registerLanguage('yaml', yaml);
hljs.registerLanguage('xml', xml);
hljs.registerLanguage('html', xml);
hljs.registerLanguage('css', css);
hljs.registerLanguage('go', go);
hljs.registerLanguage('rust', rust);
hljs.registerLanguage('java', java);

@Component({
  selector: 'app-code-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="code-editor" [class.readonly]="readonly()">
      @if (!readonly()) {
        <div class="editor-container">
          <div class="line-numbers" aria-hidden="true">
            @for (line of lineNumbers; track $index) {
              <span class="line-number">{{ line }}</span>
            }
          </div>
          <textarea
            #textarea
            class="code-textarea"
            [value]="code()"
            (input)="onInput($event)"
            (keydown)="onKeyDown($event)"
            (scroll)="onScroll($event)"
            [placeholder]="placeholder()"
            spellcheck="false"
            autocomplete="off"
            autocorrect="off"
            autocapitalize="off"
          ></textarea>
        </div>
      } @else {
        <div class="code-display">
          <div class="line-numbers" aria-hidden="true">
            @for (line of lineNumbers; track $index) {
              <span class="line-number">{{ line }}</span>
            }
          </div>
          <pre class="code-pre"><code #codeBlock class="code-block" [class]="'language-' + language()"></code></pre>
        </div>
        <button
          type="button"
          class="copy-btn"
          (click)="onCopy()"
          [attr.aria-label]="copied ? 'Copied!' : 'Copy code'"
        >
          @if (copied) {
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="icon">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12.75l6 6 9-13.5" />
            </svg>
          } @else {
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="icon">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15.666 3.888A2.25 2.25 0 0013.5 2.25h-3c-1.03 0-1.9.693-2.166 1.638m7.332 0c.055.194.084.4.084.612v0a.75.75 0 01-.75.75H9a.75.75 0 01-.75-.75v0c0-.212.03-.418.084-.612m7.332 0c.646.049 1.288.11 1.927.184 1.1.128 1.907 1.077 1.907 2.185V19.5a2.25 2.25 0 01-2.25 2.25H6.75A2.25 2.25 0 014.5 19.5V6.257c0-1.108.806-2.057 1.907-2.185a48.208 48.208 0 011.927-.184" />
            </svg>
          }
        </button>
      }
    </div>
  `,
  styles: [`
    .code-editor {
      position: relative;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.5rem;
      overflow: hidden;
      background: #1e1e1e;
    }

    .editor-container,
    .code-display {
      display: flex;
      min-height: 200px;
      max-height: 500px;
      overflow: auto;
    }

    .line-numbers {
      display: flex;
      flex-direction: column;
      padding: 1rem 0;
      background: #252526;
      border-right: 1px solid #3c3c3c;
      user-select: none;
      min-width: 3rem;
      text-align: right;
    }

    .line-number {
      padding: 0 0.75rem;
      font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
      font-size: 0.8125rem;
      line-height: 1.5rem;
      color: #858585;
    }

    .code-textarea {
      flex: 1;
      padding: 1rem;
      border: none;
      background: transparent;
      color: #d4d4d4;
      font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
      font-size: 0.8125rem;
      line-height: 1.5rem;
      resize: none;
      outline: none;
      tab-size: 2;
      white-space: pre;
      overflow-wrap: normal;
      overflow-x: auto;
    }

    .code-textarea::placeholder {
      color: #6a6a6a;
    }

    .code-pre {
      flex: 1;
      margin: 0;
      padding: 1rem;
      overflow-x: auto;
    }

    .code-block {
      font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
      font-size: 0.8125rem;
      line-height: 1.5rem;
      color: #d4d4d4;
      background: transparent;
    }

    .copy-btn {
      position: absolute;
      top: 0.5rem;
      right: 0.5rem;
      padding: 0.5rem;
      border: none;
      background: rgba(255, 255, 255, 0.1);
      color: #d4d4d4;
      border-radius: 0.25rem;
      cursor: pointer;
      transition: background-color 0.15s ease;
    }

    .copy-btn:hover {
      background: rgba(255, 255, 255, 0.2);
    }

    .icon {
      width: 1rem;
      height: 1rem;
    }

    /* Highlight.js theme overrides for dark theme */
    :host ::ng-deep .hljs {
      background: transparent;
      color: #d4d4d4;
    }

    :host ::ng-deep .hljs-keyword {
      color: #569cd6;
    }

    :host ::ng-deep .hljs-string {
      color: #ce9178;
    }

    :host ::ng-deep .hljs-number {
      color: #b5cea8;
    }

    :host ::ng-deep .hljs-comment {
      color: #6a9955;
    }

    :host ::ng-deep .hljs-function {
      color: #dcdcaa;
    }

    :host ::ng-deep .hljs-class {
      color: #4ec9b0;
    }

    :host ::ng-deep .hljs-variable {
      color: #9cdcfe;
    }

    :host ::ng-deep .hljs-built_in {
      color: #4fc1ff;
    }

    :host ::ng-deep .hljs-attr {
      color: #9cdcfe;
    }

    :host ::ng-deep .hljs-tag {
      color: #569cd6;
    }

    :host ::ng-deep .hljs-name {
      color: #569cd6;
    }

    :host ::ng-deep .hljs-attribute {
      color: #9cdcfe;
    }
  `]
})
export class CodeEditorComponent implements OnInit, AfterViewInit, OnDestroy {
  readonly code = input<string>('');
  readonly language = input<string>('javascript');
  readonly readonly = input<boolean>(false);
  readonly placeholder = input<string>('Enter your code here...');

  readonly codeChange = output<string>();
  readonly copy = output<string>();

  @ViewChild('textarea') textarea?: ElementRef<HTMLTextAreaElement>;
  @ViewChild('codeBlock') codeBlock?: ElementRef<HTMLElement>;

  lineNumbers: number[] = [1];
  copied = false;
  private copyTimeout?: number;

  ngOnInit(): void {
    this.updateLineNumbers(this.code());
  }

  ngAfterViewInit(): void {
    if (this.readonly() && this.codeBlock) {
      this.highlightCode();
    }
  }

  private updateLineNumbers(code: string): void {
    const lines = code.split('\n').length;
    this.lineNumbers = Array.from({ length: Math.max(lines, 1) }, (_, i) => i + 1);
  }

  private highlightCode(): void {
    if (!this.codeBlock) return;

    const codeElement = this.codeBlock.nativeElement;
    codeElement.textContent = this.code();

    try {
      const language = this.language();
      if (hljs.getLanguage(language)) {
        const result = hljs.highlight(this.code(), { language });
        codeElement.innerHTML = result.value;
      } else {
        // Fallback to auto-detection or plain text
        codeElement.textContent = this.code();
      }
    } catch {
      codeElement.textContent = this.code();
    }
  }

  onInput(event: Event): void {
    const target = event.target as HTMLTextAreaElement;
    const value = target.value;
    this.updateLineNumbers(value);
    this.codeChange.emit(value);
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Tab') {
      event.preventDefault();
      const target = event.target as HTMLTextAreaElement;
      const start = target.selectionStart;
      const end = target.selectionEnd;

      // Insert tab at cursor position
      const value = target.value;
      const newValue = value.substring(0, start) + '  ' + value.substring(end);
      target.value = newValue;

      // Move cursor after the inserted tab
      target.selectionStart = target.selectionEnd = start + 2;

      this.updateLineNumbers(newValue);
      this.codeChange.emit(newValue);
    }
  }

  onScroll(event: Event): void {
    // Sync line numbers scroll with textarea
    const target = event.target as HTMLTextAreaElement;
    const lineNumbers = target.previousElementSibling as HTMLElement;
    if (lineNumbers) {
      lineNumbers.scrollTop = target.scrollTop;
    }
  }

  onCopy(): void {
    this.copy.emit(this.code());
    this.copied = true;

    if (this.copyTimeout) {
      clearTimeout(this.copyTimeout);
    }

    this.copyTimeout = window.setTimeout(() => {
      this.copied = false;
    }, 2000);
  }

  ngOnDestroy(): void {
    if (this.copyTimeout) {
      clearTimeout(this.copyTimeout);
    }
  }
}
