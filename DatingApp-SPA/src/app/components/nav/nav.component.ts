import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};

  constructor(private authService: AuthService) {}

  ngOnInit() {}

  login = () => {
    this.authService.login(this.model).subscribe(
      next => {
        console.log('success');
      },
      error => {
        console.log('error');
      }
    );
  }

  isLoggedIn = (): boolean => {
    const token = localStorage.getItem('token');
    return !!token; // !! false if token is undefined or null true for a value
  }

  logout = () => {
    localStorage.removeItem('token');
    console.log('logged out');
  }
}
