import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { AlertifyService } from '../../services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  photoUrl: string;

  constructor(
    public authService: AuthService,
    private alertify: AlertifyService,
    private router: Router
  ) {}

  ngOnInit() {
    this.authService.photoUrl.subscribe(
      photoUrl => {this.photoUrl = photoUrl; }
    );
  }

  login = () => {
    this.authService.login(this.model).subscribe(
      next => {
        this.alertify.success('login success');
      },
      error => {
        this.alertify.error(error);
      },
      () => {
        this.router.navigate(['/members']);
      }
    );
  }

  isLoggedIn = (): boolean => {
    return this.authService.isLoggedIn();
  }

  logout = () => {
    localStorage.removeItem('token');
    this.authService.decodedToken = null;
    localStorage.removeItem('user');
    this.authService.currentUser = null;
    this.alertify.message('logged out');
    this.router.navigate(['home']);
  }
}
