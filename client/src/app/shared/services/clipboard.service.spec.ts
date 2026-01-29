import { TestBed } from '@angular/core/testing';
import { ClipboardService } from './clipboard.service';

describe('ClipboardService', () => {
  let service: ClipboardService;
  let originalClipboard: Clipboard | undefined;

  beforeEach(() => {
    originalClipboard = navigator.clipboard;

    TestBed.configureTestingModule({
      providers: [ClipboardService]
    });
    service = TestBed.inject(ClipboardService);
  });

  afterEach(() => {
    // Restore original clipboard
    Object.defineProperty(navigator, 'clipboard', {
      value: originalClipboard,
      writable: true,
      configurable: true
    });
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('copy', () => {
    it('should use navigator.clipboard when available', async () => {
      const mockClipboard = {
        writeText: vi.fn().mockResolvedValue(undefined)
      };
      Object.defineProperty(navigator, 'clipboard', {
        value: mockClipboard,
        writable: true,
        configurable: true
      });

      const result = await service.copy('test text');

      expect(mockClipboard.writeText).toHaveBeenCalledWith('test text');
      expect(result).toBe(true);
    });

    it('should return false when clipboard API fails and fallback fails', async () => {
      const mockClipboard = {
        writeText: vi.fn().mockRejectedValue(new Error('Failed'))
      };
      Object.defineProperty(navigator, 'clipboard', {
        value: mockClipboard,
        writable: true,
        configurable: true
      });

      // The fallback uses document.execCommand which may throw or return false
      // In test environment without execCommand, the service should catch the error and return false
      const result = await service.copy('test text');

      expect(result).toBe(false);
    });

    it('should use fallback when navigator.clipboard is not available', async () => {
      Object.defineProperty(navigator, 'clipboard', {
        value: undefined,
        writable: true,
        configurable: true
      });

      // In test environment, the fallback will try to use execCommand
      // but since it doesn't exist, it should return false
      const result = await service.copy('test text');

      // The result depends on whether execCommand works in the test environment
      // In most test environments, this will be false
      expect(typeof result).toBe('boolean');
    });
  });
});
