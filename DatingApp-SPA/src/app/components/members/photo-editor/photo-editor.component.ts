import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { Photo } from '../../../models/Photo';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../services/auth.service';
import { UserService } from '../../../services/user.service';
import { AlertifyService } from '../../../services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input()
  photos: Photo[];
  @Output()
  getMemberPhotoChange = new EventEmitter<string>();
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  currentMainPhoto: Photo;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private alertify: AlertifyService
  ) {}

  ngOnInit() {
    this.initUploader();
  }

  fileOverBase = (e: any): void => {
    this.hasBaseDropZoneOver = e;
  }

  initUploader = (): void => {
    this.uploader = new FileUploader({
      url: `${this.baseUrl}users/${
        this.authService.decodedToken.nameid
      }/photos`,
      authToken: `Bearer ${localStorage.getItem('token')}`,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = file => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      const res: Photo = JSON.parse(response);
      if (res) {
        const photo = {
          ...res
        };
        console.log(photo);
        this.photos.push(photo);
        if (photo.isMain) {
          this.currentMainPhoto = photo;
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem(
            'user',
            JSON.stringify(this.authService.currentUser)
          );
        }
      }
    };
  }

  setMainPhoto = (photo: Photo) => {
    this.userService
      .setMainPhoto(this.authService.decodedToken.nameid, photo.id)
      .subscribe(
        () => {
          this.currentMainPhoto = this.photos.filter(p => p.isMain === true)[0];
          this.currentMainPhoto.isMain = false;
          photo.isMain = true;
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem(
            'user',
            JSON.stringify(this.authService.currentUser)
          );
        },
        error => {
          this.alertify.error(error);
        }
      );
  }

  deletePhoto = (id: number) => {
    this.alertify.confirm('Are you sure you want to delete this photo?', () => {
      this.userService
        .deletePhoto(this.authService.decodedToken.nameid, id)
        .subscribe(
          () => {
            this.photos.splice(this.photos.findIndex(p => p.id === id), 1);
            this.alertify.success('Photo has been deleted');
          },
          error => {
            this.alertify.error('Failed to delete the photo');
          }
        );
    });
  }
}
