import { Component, inject } from '@angular/core';
import { ToastService } from '../../../core/ui/toast.service';

@Component({
  selector: 'app-toast-host',
  templateUrl: './toast-host.html',
  styleUrl: './toast-host.css',
})
export class ToastHost {
  protected readonly toastService = inject(ToastService);
}