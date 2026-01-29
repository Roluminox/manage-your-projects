import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TagBadgeComponent } from './tag-badge.component';

describe('TagBadgeComponent', () => {
  let component: TagBadgeComponent;
  let fixture: ComponentFixture<TagBadgeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TagBadgeComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(TagBadgeComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.componentRef.setInput('name', 'Test Tag');
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.tag-badge');
    expect(badge).toBeTruthy();
  });

  it('should display the tag name', () => {
    fixture.componentRef.setInput('name', 'Test Tag');
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.tag-badge');
    expect(badge.textContent.trim()).toContain('Test Tag');
  });

  it('should apply the background color', () => {
    fixture.componentRef.setInput('name', 'Test Tag');
    fixture.componentRef.setInput('color', '#ff0000');
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.tag-badge') as HTMLElement;
    expect(badge.style.backgroundColor).toBe('rgb(255, 0, 0)');
  });

  it('should not show remove button when not removable', () => {
    fixture.componentRef.setInput('name', 'Test Tag');
    fixture.componentRef.setInput('removable', false);
    fixture.detectChanges();

    const removeBtn = fixture.nativeElement.querySelector('.remove-btn');
    expect(removeBtn).toBeFalsy();
  });

  it('should show remove button when removable', () => {
    fixture.componentRef.setInput('name', 'Test Tag');
    fixture.componentRef.setInput('removable', true);
    fixture.detectChanges();

    const removeBtn = fixture.nativeElement.querySelector('.remove-btn');
    expect(removeBtn).toBeTruthy();
  });

  it('should emit remove event when remove button clicked', () => {
    fixture.componentRef.setInput('name', 'Test Tag');
    fixture.componentRef.setInput('removable', true);
    fixture.detectChanges();

    let emitted = false;
    component.remove.subscribe(() => {
      emitted = true;
    });

    const removeBtn = fixture.nativeElement.querySelector('.remove-btn') as HTMLButtonElement;
    removeBtn.click();

    expect(emitted).toBe(true);
  });

  it('should use white text for dark backgrounds', () => {
    fixture.componentRef.setInput('name', 'Test Tag');
    fixture.componentRef.setInput('color', '#000000');
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.tag-badge') as HTMLElement;
    expect(badge.style.color).toBe('rgb(255, 255, 255)');
  });

  it('should use black text for light backgrounds', () => {
    fixture.componentRef.setInput('name', 'Test Tag');
    fixture.componentRef.setInput('color', '#ffffff');
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.tag-badge') as HTMLElement;
    expect(badge.style.color).toBe('rgb(0, 0, 0)');
  });
});
