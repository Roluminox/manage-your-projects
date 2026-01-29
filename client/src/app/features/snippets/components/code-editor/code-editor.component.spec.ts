import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CodeEditorComponent } from './code-editor.component';

describe('CodeEditorComponent', () => {
  let component: CodeEditorComponent;
  let fixture: ComponentFixture<CodeEditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CodeEditorComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(CodeEditorComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  describe('edit mode', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('readonly', false);
      fixture.componentRef.setInput('code', '');
      fixture.detectChanges();
    });

    it('should show textarea when not readonly', () => {
      const textarea = fixture.nativeElement.querySelector('.code-textarea');
      expect(textarea).toBeTruthy();
    });

    it('should not show copy button when not readonly', () => {
      const copyBtn = fixture.nativeElement.querySelector('.copy-btn');
      expect(copyBtn).toBeFalsy();
    });

    it('should display line numbers', () => {
      const lineNumbers = fixture.nativeElement.querySelectorAll('.line-number');
      expect(lineNumbers.length).toBeGreaterThan(0);
    });

    it('should emit codeChange when user types', () => {
      let emittedValue = '';
      component.codeChange.subscribe((value) => {
        emittedValue = value;
      });

      const textarea = fixture.nativeElement.querySelector('.code-textarea') as HTMLTextAreaElement;
      textarea.value = 'const x = 1;';
      textarea.dispatchEvent(new Event('input'));

      expect(emittedValue).toBe('const x = 1;');
    });

    it('should update line numbers when code changes', () => {
      fixture.componentRef.setInput('code', 'line1\nline2\nline3');
      fixture.detectChanges();

      // Trigger input event to update line numbers
      const textarea = fixture.nativeElement.querySelector('.code-textarea') as HTMLTextAreaElement;
      textarea.value = 'line1\nline2\nline3\nline4';
      textarea.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      const lineNumbers = fixture.nativeElement.querySelectorAll('.line-number');
      expect(lineNumbers.length).toBe(4);
    });

    it('should insert spaces when Tab is pressed', () => {
      let emittedValue = '';
      component.codeChange.subscribe((value) => {
        emittedValue = value;
      });

      const textarea = fixture.nativeElement.querySelector('.code-textarea') as HTMLTextAreaElement;
      textarea.value = 'function test() {';
      textarea.selectionStart = textarea.selectionEnd = textarea.value.length;

      const tabEvent = new KeyboardEvent('keydown', { key: 'Tab' });
      textarea.dispatchEvent(tabEvent);

      expect(textarea.value).toContain('  ');
    });

    it('should display placeholder text', () => {
      fixture.componentRef.setInput('placeholder', 'Enter code here');
      fixture.detectChanges();

      const textarea = fixture.nativeElement.querySelector('.code-textarea') as HTMLTextAreaElement;
      expect(textarea.placeholder).toBe('Enter code here');
    });
  });

  describe('readonly mode', () => {
    const sampleCode = 'const greeting = "Hello";\nconsole.log(greeting);';

    beforeEach(() => {
      fixture.componentRef.setInput('readonly', true);
      fixture.componentRef.setInput('code', sampleCode);
      fixture.componentRef.setInput('language', 'javascript');
      fixture.detectChanges();
    });

    it('should show code display when readonly', () => {
      const codeDisplay = fixture.nativeElement.querySelector('.code-display');
      expect(codeDisplay).toBeTruthy();
    });

    it('should not show textarea when readonly', () => {
      const textarea = fixture.nativeElement.querySelector('.code-textarea');
      expect(textarea).toBeFalsy();
    });

    it('should show copy button when readonly', () => {
      const copyBtn = fixture.nativeElement.querySelector('.copy-btn');
      expect(copyBtn).toBeTruthy();
    });

    it('should emit copy event when copy button clicked', () => {
      let emittedCode = '';
      component.copy.subscribe((code) => {
        emittedCode = code;
      });

      const copyBtn = fixture.nativeElement.querySelector('.copy-btn') as HTMLButtonElement;
      copyBtn.click();

      expect(emittedCode).toBe(sampleCode);
    });

    it('should show copied state after clicking copy', () => {
      const copyBtn = fixture.nativeElement.querySelector('.copy-btn') as HTMLButtonElement;
      copyBtn.click();
      fixture.detectChanges();

      expect(component.copied).toBe(true);
    });

    it('should display correct number of line numbers', () => {
      const lineNumbers = fixture.nativeElement.querySelectorAll('.line-number');
      expect(lineNumbers.length).toBe(2); // 2 lines in sampleCode
    });

    it('should apply language class to code block', () => {
      const codeBlock = fixture.nativeElement.querySelector('.code-block');
      expect(codeBlock.classList.contains('language-javascript')).toBe(true);
    });
  });

  describe('language support', () => {
    it('should apply different language classes', () => {
      fixture.componentRef.setInput('readonly', true);
      fixture.componentRef.setInput('code', 'print("hello")');
      fixture.componentRef.setInput('language', 'python');
      fixture.detectChanges();

      const codeBlock = fixture.nativeElement.querySelector('.code-block');
      expect(codeBlock.classList.contains('language-python')).toBe(true);
    });
  });
});
