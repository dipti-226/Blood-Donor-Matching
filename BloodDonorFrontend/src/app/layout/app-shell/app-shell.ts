import { Component, OnInit, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { HospitalContextService } from '../../core/auth/hospital-context.service';
import { SessionService } from '../../core/auth/session.service';
import { Roles } from '../../core/models/enums';
import { ToastService } from '../../core/ui/toast.service';
import { NAV_ITEMS } from '../nav-items';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.css',
})
export class AppShell implements OnInit {
  protected readonly session = inject(SessionService);
  private readonly hospitalContext = inject(HospitalContextService);
  private readonly toast = inject(ToastService);

  protected readonly hospital = this.hospitalContext.hospital;

  protected readonly navItems = computed(() =>
    NAV_ITEMS.filter((item) => this.session.hasAnyRole(item.roles)),
  );
  protected readonly bottomItems = computed(() =>
    this.navItems()
      .filter((item) => item.route !== '/notifications')
      .slice(0, 4),
  );
  protected readonly showBell = computed(() =>
    this.navItems().some((item) => item.route === '/notifications'),
  );
  protected readonly isDonor = computed(() => this.session.hasRole(Roles.Donor));

  ngOnInit(): void {
    // After a page reload the session survives but the hospital details do not.
    if (this.session.isHospitalUser() && !this.hospital()) {
      this.hospitalContext.load().subscribe({
        error: () => this.toast.error('We could not load your hospital details.'),
      });
    }
  }

  protected signOut(): void {
    this.session.signOut('manual');
  }
}