import { Injectable, inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class ClipboardService {
  private readonly document = inject(DOCUMENT);

  async copy(text: string): Promise<boolean> {
    if (navigator.clipboard) {
      try {
        await navigator.clipboard.writeText(text);
        return true;
      } catch {
        return this.fallbackCopy(text);
      }
    }
    return this.fallbackCopy(text);
  }

  private fallbackCopy(text: string): boolean {
    const textarea = this.document.createElement('textarea');
    textarea.value = text;
    textarea.style.position = 'fixed';
    textarea.style.left = '-9999px';
    textarea.style.top = '-9999px';

    this.document.body.appendChild(textarea);
    textarea.focus();
    textarea.select();

    try {
      const successful = this.document.execCommand('copy');
      this.document.body.removeChild(textarea);
      return successful;
    } catch {
      this.document.body.removeChild(textarea);
      return false;
    }
  }
}
