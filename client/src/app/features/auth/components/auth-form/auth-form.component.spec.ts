import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthFormComponent } from './auth-form.component';

describe('AuthFormComponent', () => {
  describe('with default values', () => {
    @Component({
      standalone: true,
      imports: [AuthFormComponent],
      template: `
        <app-auth-form
          title="Test Title"
          subtitle="Test Subtitle"
          [error]="null"
          linkLabel="Need an account?"
          linkText="Sign up"
          linkRoute="/auth/register"
        >
          <div class="test-content">Form Content</div>
        </app-auth-form>
      `
    })
    class DefaultTestHostComponent {}

    let fixture: ComponentFixture<DefaultTestHostComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [DefaultTestHostComponent],
        providers: [provideRouter([])]
      }).compileComponents();

      fixture = TestBed.createComponent(DefaultTestHostComponent);
      fixture.detectChanges();
    });

    it('should display the title', () => {
      const title = fixture.nativeElement.querySelector('h1');
      expect(title.textContent).toBe('Test Title');
    });

    it('should display the subtitle', () => {
      const subtitle = fixture.nativeElement.querySelector('.subtitle');
      expect(subtitle.textContent).toBe('Test Subtitle');
    });

    it('should project content', () => {
      const content = fixture.nativeElement.querySelector('.test-content');
      expect(content).toBeTruthy();
      expect(content.textContent).toBe('Form Content');
    });

    it('should display link with correct text and route', () => {
      const link = fixture.nativeElement.querySelector('.auth-link a');
      expect(link.textContent).toBe('Sign up');
      expect(link.getAttribute('href')).toBe('/auth/register');
    });

    it('should display link label', () => {
      const linkParagraph = fixture.nativeElement.querySelector('.auth-link');
      expect(linkParagraph.textContent).toContain('Need an account?');
    });

    it('should not display error banner when error is null', () => {
      const errorBanner = fixture.nativeElement.querySelector('.error-banner');
      expect(errorBanner).toBeFalsy();
    });
  });

  describe('with error', () => {
    @Component({
      standalone: true,
      imports: [AuthFormComponent],
      template: `
        <app-auth-form
          title="Test"
          subtitle="Test"
          error="Something went wrong"
        >
          <div>Content</div>
        </app-auth-form>
      `
    })
    class ErrorTestHostComponent {}

    let fixture: ComponentFixture<ErrorTestHostComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [ErrorTestHostComponent],
        providers: [provideRouter([])]
      }).compileComponents();

      fixture = TestBed.createComponent(ErrorTestHostComponent);
      fixture.detectChanges();
    });

    it('should display error banner when error exists', () => {
      const errorBanner = fixture.nativeElement.querySelector('.error-banner');
      expect(errorBanner).toBeTruthy();
      expect(errorBanner.textContent).toContain('Something went wrong');
    });

    it('should have role="alert" on error banner', () => {
      const errorBanner = fixture.nativeElement.querySelector('.error-banner');
      expect(errorBanner.getAttribute('role')).toBe('alert');
    });
  });

  describe('without link', () => {
    @Component({
      standalone: true,
      imports: [AuthFormComponent],
      template: `
        <app-auth-form
          title="Test"
          subtitle="Test"
        >
          <div>Content</div>
        </app-auth-form>
      `
    })
    class NoLinkTestHostComponent {}

    let fixture: ComponentFixture<NoLinkTestHostComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [NoLinkTestHostComponent],
        providers: [provideRouter([])]
      }).compileComponents();

      fixture = TestBed.createComponent(NoLinkTestHostComponent);
      fixture.detectChanges();
    });

    it('should not display link when linkRoute is not provided', () => {
      const authLink = fixture.nativeElement.querySelector('.auth-link');
      expect(authLink).toBeFalsy();
    });

    it('should not display link when linkText is not provided', () => {
      const authLink = fixture.nativeElement.querySelector('.auth-link');
      expect(authLink).toBeFalsy();
    });
  });
});
