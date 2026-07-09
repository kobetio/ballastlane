import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

/**
 * Shell for every authenticated route: a toolbar with the app title, the current user's
 * name, and a logout action, wrapping a `<router-outlet>` for the active feature page.
 */
@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, MatToolbarModule, MatButtonModule, MatIconModule],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainLayout {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentUser = this.authService.currentUser;

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
