import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LabelBadgeComponent, LabelBadgeData } from './label-badge.component';

describe('LabelBadgeComponent', () => {
  let component: LabelBadgeComponent;
  let fixture: ComponentFixture<LabelBadgeComponent>;

  const mockLabel: LabelBadgeData = {
    name: 'Bug',
    color: '#ef4444'
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LabelBadgeComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(LabelBadgeComponent);
    component = fixture.componentInstance;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      component.label = mockLabel;
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });
  });

  describe('Display', () => {
    beforeEach(() => {
      component.label = mockLabel;
      fixture.detectChanges();
    });

    it('should display label name', () => {
      const textElement = fixture.nativeElement.querySelector('.label-text');
      expect(textElement.textContent).toBe('Bug');
    });

    it('should display color dot', () => {
      const dotElement = fixture.nativeElement.querySelector('.label-dot');
      expect(dotElement).toBeTruthy();
    });

    it('should apply label color to CSS variable', () => {
      const badge = fixture.nativeElement.querySelector('.label-badge');
      expect(badge.style.getPropertyValue('--label-color')).toBe('#ef4444');
    });

    it('should have title attribute with label name', () => {
      const badge = fixture.nativeElement.querySelector('.label-badge');
      expect(badge.getAttribute('title')).toBe('Bug');
    });
  });

  describe('Size Variants', () => {
    it('should have medium size by default', () => {
      component.label = mockLabel;
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.label-badge');
      expect(badge.classList.contains('small')).toBe(false);
      expect(badge.classList.contains('large')).toBe(false);
    });

    it('should apply small class when size is small', () => {
      component.label = mockLabel;
      component.size = 'small';
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.label-badge');
      expect(badge.classList.contains('small')).toBe(true);
    });

    it('should apply large class when size is large', () => {
      component.label = mockLabel;
      component.size = 'large';
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.label-badge');
      expect(badge.classList.contains('large')).toBe(true);
    });
  });

  describe('Compact Mode', () => {
    it('should show text by default (non-compact)', () => {
      component.label = mockLabel;
      component.compact = false;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.label-text');
      expect(textElement).toBeTruthy();
    });

    it('should hide text in compact mode', () => {
      component.label = mockLabel;
      component.compact = true;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.label-text');
      expect(textElement).toBeNull();
    });

    it('should show dot even in compact mode', () => {
      component.label = mockLabel;
      component.compact = true;
      fixture.detectChanges();

      const dotElement = fixture.nativeElement.querySelector('.label-dot');
      expect(dotElement).toBeTruthy();
    });
  });

  describe('Different Labels', () => {
    it('should display different label colors', () => {
      const greenLabel: LabelBadgeData = { name: 'Feature', color: '#22c55e' };
      component.label = greenLabel;
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.label-badge');
      expect(badge.style.getPropertyValue('--label-color')).toBe('#22c55e');
    });

    it('should display different label names', () => {
      const urgentLabel: LabelBadgeData = { name: 'Urgent', color: '#f97316' };
      component.label = urgentLabel;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.label-text');
      expect(textElement.textContent).toBe('Urgent');
    });
  });

  describe('Long Text Handling', () => {
    it('should handle long label names with ellipsis', () => {
      const longLabel: LabelBadgeData = {
        name: 'This is a very long label name that should be truncated',
        color: '#6366f1'
      };
      component.label = longLabel;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.label-text');
      expect(textElement.textContent).toBe(longLabel.name);

      // Check that overflow styles are applied
      const styles = window.getComputedStyle(textElement);
      expect(styles.overflow).toBe('hidden');
      expect(styles.textOverflow).toBe('ellipsis');
    });
  });
});
