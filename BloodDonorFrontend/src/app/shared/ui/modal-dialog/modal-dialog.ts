import { AfterViewInit, Component, ElementRef, OnDestroy, input, output, viewChild } from '@angular/core';

let nextId = 0;

// A modal built on the native <dialog>: focus trapping, Esc and the backdrop come from the browser.
// The dialog opens as soon as it is created and is removed by its parent with @if, which also
// throws away everything inside it (typed values, shown contact numbers) when it closes.
@Component({
  selector: 'app-modal-dialog',
  templateUrl: './modal-dialog.html',
  styleUrl: './modal-dialog.css',
})
export class ModalDialog implements AfterViewInit, OnDestroy {
  readonly title = input.required<string>();
  // Set to false while a request is running so the dialog cannot be dismissed half-way.
  readonly dismissible = input<boolean>(true);
  readonly closed = output<void>();

  protected readonly titleId = `bd-modal-title-${nextId++}`;

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  private destroyed = false;
  private pressedOnBackdrop = false;

  ngAfterViewInit(): void {
    const element = this.dialog().nativeElement;
    if (!element.open) {
      element.showModal();
    }
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    const element = this.dialog().nativeElement;
    if (element.open) {
      element.close();
    }
  }

  protected onCancel(event: Event): void {
    if (!this.dismissible()) {
      event.preventDefault();
    }
  }

  protected onClose(): void {
    if (!this.destroyed) {
      this.closed.emit();
    }
  }

  protected onMouseDown(event: MouseEvent): void {
    this.pressedOnBackdrop = event.target === event.currentTarget;
  }

  // A click counts as a backdrop click only if it started and ended outside the panel,
  // so dragging to select text inside the dialog never closes it.
  protected onClick(event: MouseEvent): void {
    if (this.pressedOnBackdrop && event.target === event.currentTarget) {
      this.close();
    }
    this.pressedOnBackdrop = false;
  }

  protected close(): void {
    if (this.dismissible()) {
      this.dialog().nativeElement.close();
    }
  }
}