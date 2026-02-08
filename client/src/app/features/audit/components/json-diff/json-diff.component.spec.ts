import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SimpleChange } from '@angular/core';
import { JsonDiffComponent } from './json-diff.component';

describe('JsonDiffComponent', () => {
  let component: JsonDiffComponent;
  let fixture: ComponentFixture<JsonDiffComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JsonDiffComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(JsonDiffComponent);
    component = fixture.componentInstance;
  });

  function setInputsAndDetect(oldValues: string | null, newValues: string | null): void {
    component.oldValues = oldValues;
    component.newValues = newValues;
    component.ngOnChanges();
    fixture.detectChanges();
  }

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should show no diff message when both values are null', () => {
    setInputsAndDetect(null, null);

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No changes recorded');
  });

  it('should show added entries for new values only', () => {
    setInputsAndDetect(null, '{"Title":"New Snippet","Language":"typescript"}');

    expect(component.diffEntries.length).toBe(2);
    expect(component.diffEntries.every(e => e.type === 'added')).toBe(true);
  });

  it('should show removed entries for old values only', () => {
    setInputsAndDetect('{"Title":"Deleted","Code":"old code"}', null);

    expect(component.diffEntries.length).toBe(2);
    expect(component.diffEntries.every(e => e.type === 'removed')).toBe(true);
  });

  it('should show changed entries when values differ', () => {
    setInputsAndDetect('{"Title":"Old Title","Language":"javascript"}', '{"Title":"New Title","Language":"javascript"}');

    expect(component.diffEntries.length).toBe(1);
    expect(component.diffEntries[0].key).toBe('Title');
    expect(component.diffEntries[0].type).toBe('changed');
    expect(component.diffEntries[0].oldValue).toBe('Old Title');
    expect(component.diffEntries[0].newValue).toBe('New Title');
  });

  it('should handle mixed added, removed, and changed entries', () => {
    setInputsAndDetect('{"Title":"Old","Removed":"gone"}', '{"Title":"New","Added":"here"}');

    const types = component.diffEntries.map(e => e.type);
    expect(types).toContain('changed');
    expect(types).toContain('added');
    expect(types).toContain('removed');
  });

  it('should format values correctly', () => {
    expect(component.formatValue('hello')).toBe('"hello"');
    expect(component.formatValue(42)).toBe('42');
    expect(component.formatValue(null)).toBe('null');
    expect(component.formatValue(true)).toBe('true');
  });

  it('should handle invalid JSON gracefully', () => {
    setInputsAndDetect('not json', '{"valid":"json"}');

    expect(component.diffEntries.length).toBe(1);
    expect(component.diffEntries[0].type).toBe('added');
  });
});
