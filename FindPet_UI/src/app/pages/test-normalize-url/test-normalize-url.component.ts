import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import normalizeUrl from 'normalize-url';

@Component({
  selector: 'app-test-normalize-url',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './test-normalize-url.component.html',
  styleUrls: ['./test-normalize-url.component.scss']
})
export class TestNormalizeUrlComponent {
  inputUrl: string = '';
  normalizedUrl: string = '';
  error: string = '';

  normalizeUrlInput(): void {
    this.error = '';
    this.normalizedUrl = '';

    if (!this.inputUrl.trim()) {
      this.error = 'Будь ласка, введіть URL';
      return;
    }

    try {
      this.normalizedUrl = normalizeUrl(this.inputUrl);
    } catch (err) {
      this.error = `Помилка: ${err}`;
    }
  }

  clearAll(): void {
    this.inputUrl = '';
    this.normalizedUrl = '';
    this.error = '';
  }

  useExample(url: string): void {
    this.inputUrl = url;
    this.normalizeUrlInput();
  }
}
