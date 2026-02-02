import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PriorityBadgeComponent, Priority, PRIORITY_CONFIG } from './priority-badge.component';

describe('PriorityBadgeComponent', () => {
  let component: PriorityBadgeComponent;
  let fixture: ComponentFixture<PriorityBadgeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PriorityBadgeComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(PriorityBadgeComponent);
    component = fixture.componentInstance;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      component.priority = Priority.Medium;
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });
  });

  describe('Priority Display', () => {
    it('should display Low priority correctly', () => {
      component.priority = Priority.Low;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.priority-text');
      const badge = fixture.nativeElement.querySelector('.priority-badge');

      expect(textElement.textContent).toBe('Low');
      expect(badge.style.getPropertyValue('--priority-color')).toBe('#22c55e');
    });

    it('should display Medium priority correctly', () => {
      component.priority = Priority.Medium;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.priority-text');
      const badge = fixture.nativeElement.querySelector('.priority-badge');

      expect(textElement.textContent).toBe('Medium');
      expect(badge.style.getPropertyValue('--priority-color')).toBe('#eab308');
    });

    it('should display High priority correctly', () => {
      component.priority = Priority.High;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.priority-text');
      const badge = fixture.nativeElement.querySelector('.priority-badge');

      expect(textElement.textContent).toBe('High');
      expect(badge.style.getPropertyValue('--priority-color')).toBe('#f97316');
    });

    it('should display Critical priority correctly', () => {
      component.priority = Priority.Critical;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.priority-text');
      const badge = fixture.nativeElement.querySelector('.priority-badge');

      expect(textElement.textContent).toBe('Critical');
      expect(badge.style.getPropertyValue('--priority-color')).toBe('#ef4444');
    });
  });

  describe('Icon Display', () => {
    it('should display icon', () => {
      component.priority = Priority.Medium;
      fixture.detectChanges();

      const iconElement = fixture.nativeElement.querySelector('.priority-icon');
      expect(iconElement).toBeTruthy();
    });

    it('should have correct icon path for each priority', () => {
      component.priority = Priority.Critical;
      fixture.detectChanges();

      const pathElement = fixture.nativeElement.querySelector('.priority-icon path');
      expect(pathElement.getAttribute('d')).toBe(PRIORITY_CONFIG[Priority.Critical].icon);
    });
  });

  describe('Size Variants', () => {
    it('should have medium size by default', () => {
      component.priority = Priority.Medium;
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.priority-badge');
      expect(badge.classList.contains('small')).toBe(false);
      expect(badge.classList.contains('large')).toBe(false);
    });

    it('should apply small class when size is small', () => {
      component.priority = Priority.Medium;
      component.size = 'small';
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.priority-badge');
      expect(badge.classList.contains('small')).toBe(true);
    });

    it('should apply large class when size is large', () => {
      component.priority = Priority.Medium;
      component.size = 'large';
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.priority-badge');
      expect(badge.classList.contains('large')).toBe(true);
    });
  });

  describe('Icon Only Mode', () => {
    it('should show text by default', () => {
      component.priority = Priority.Medium;
      component.iconOnly = false;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.priority-text');
      expect(textElement).toBeTruthy();
    });

    it('should hide text in icon only mode', () => {
      component.priority = Priority.Medium;
      component.iconOnly = true;
      fixture.detectChanges();

      const textElement = fixture.nativeElement.querySelector('.priority-text');
      expect(textElement).toBeNull();
    });

    it('should apply icon-only class', () => {
      component.priority = Priority.Medium;
      component.iconOnly = true;
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.priority-badge');
      expect(badge.classList.contains('icon-only')).toBe(true);
    });

    it('should show icon even in icon only mode', () => {
      component.priority = Priority.Medium;
      component.iconOnly = true;
      fixture.detectChanges();

      const iconElement = fixture.nativeElement.querySelector('.priority-icon');
      expect(iconElement).toBeTruthy();
    });
  });

  describe('Title Attribute', () => {
    it('should have title attribute with priority label', () => {
      component.priority = Priority.High;
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.priority-badge');
      expect(badge.getAttribute('title')).toBe('High');
    });

    it('should have correct title for Critical priority', () => {
      component.priority = Priority.Critical;
      fixture.detectChanges();

      const badge = fixture.nativeElement.querySelector('.priority-badge');
      expect(badge.getAttribute('title')).toBe('Critical');
    });
  });

  describe('Config Getter', () => {
    it('should return correct config for priority', () => {
      component.priority = Priority.High;
      expect(component.config).toBe(PRIORITY_CONFIG[Priority.High]);
    });

    it('should fallback to Medium config for invalid priority', () => {
      component.priority = 999 as Priority;
      expect(component.config).toBe(PRIORITY_CONFIG[Priority.Medium]);
    });
  });

  describe('PRIORITY_CONFIG', () => {
    it('should have config for all priorities', () => {
      expect(PRIORITY_CONFIG[Priority.Low]).toBeDefined();
      expect(PRIORITY_CONFIG[Priority.Medium]).toBeDefined();
      expect(PRIORITY_CONFIG[Priority.High]).toBeDefined();
      expect(PRIORITY_CONFIG[Priority.Critical]).toBeDefined();
    });

    it('should have label, color and icon for each priority', () => {
      Object.values(PRIORITY_CONFIG).forEach(config => {
        expect(config.label).toBeDefined();
        expect(config.color).toBeDefined();
        expect(config.icon).toBeDefined();
      });
    });
  });
});
