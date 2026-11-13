import { Component, output, signal } from '@angular/core';

// The same limits the server checks in ImageFile; the server repeats every check.
const MAX_SIZE = 5 * 1024 * 1024;
const ALLOWED_TYPES = ['image/jpeg', 'image/png'];

// An image field for a form: checks the chosen file, shows a preview and reports the
// file to the page. Nothing is sent from here; the page sends the file with its command.
@Component({
  selector: 'app-image-upload',
  styleUrl: './image-upload.scss',
  templateUrl: './image-upload.html',
})
export class ImageUpload {
  // The accepted file, or null when the chosen file was rejected or removed.
  readonly imageChange = output<File | null>();

  protected readonly file = signal<File | null>(null);
  protected readonly preview = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly dragging = signal(false);

  protected onChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const chosen = input.files?.[0];
    // Clear the input so choosing the same file again still raises a change.
    input.value = '';
    if (chosen) {
      this.choose(chosen);
    }
  }

  protected onDragOver(event: DragEvent): void {
    // Without preventDefault the browser does not allow a drop here.
    event.preventDefault();
    this.dragging.set(true);
  }

  protected onDragLeave(): void {
    this.dragging.set(false);
  }

  protected onDrop(event: DragEvent): void {
    // Without preventDefault the browser opens the dropped file instead.
    event.preventDefault();
    this.dragging.set(false);
    const dropped = event.dataTransfer?.files[0];
    if (dropped) {
      this.choose(dropped);
    }
  }

  // Returns the field to its initial state, for a form where the image is optional.
  protected remove(): void {
    this.file.set(null);
    this.preview.set(null);
    this.error.set(null);
    this.imageChange.emit(null);
  }

  protected sizeText(size: number): string {
    if (size < 1024 * 1024) {
      return `${Math.max(1, Math.round(size / 1024))} KB`;
    }
    return `${(size / (1024 * 1024)).toFixed(1).replace('.', ',')} MB`;
  }

  private choose(chosen: File): void {
    const reason = this.rejectionReason(chosen);
    if (reason) {
      this.file.set(null);
      this.preview.set(null);
      this.error.set(reason);
      this.imageChange.emit(null);
      return;
    }

    this.file.set(chosen);
    this.preview.set(null);
    this.error.set(null);
    this.imageChange.emit(chosen);

    // A data URL lives only in this page, so there is nothing to release afterwards.
    const reader = new FileReader();
    reader.onload = () => {
      // Ignore a late result when another file was chosen in the meantime.
      if (this.file() === chosen) {
        this.preview.set(reader.result as string);
      }
    };
    reader.readAsDataURL(chosen);
  }

  private rejectionReason(chosen: File): string | null {
    if (!ALLOWED_TYPES.includes(chosen.type)) {
      return `${chosen.name} nije podržan format. Dozvoljeni su JPG i PNG.`;
    }
    if (chosen.size > MAX_SIZE) {
      return 'Slika je veća od 5 MB.';
    }
    return null;
  }
}
