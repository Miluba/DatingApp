import { Component, OnInit } from '@angular/core';
import { User } from '../../../models/User';
import { UserService } from '../../../services/user.service';
import { AlertifyService } from '../../../services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResults } from '../../../models/pagination';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];
  pagination: Pagination;
  user: User = JSON.parse(localStorage.getItem('user'));
  userParams: any = {};
  genderList = [
    { value: 'male', display: 'Males' },
    { value: 'female', display: 'Females' }
  ];

  constructor(
    private userService: UserService,
    private alertify: AlertifyService,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data['users'].result;
      this.pagination = data['users'].pagination;
    });
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';

  }

  resetFilters = () => {
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.loadUsers();
  }

  pageChanged = (event: any): void => {
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }

  loadUsers = () => {
    this.userService
      .getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams)
      .subscribe(
        (res: PaginatedResults<User[]>) => {
          this.users = res.result;
          this.pagination = res.pagination;
        },
        error => {
          this.alertify.error(error);
        }
      );
  }
}
