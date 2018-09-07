import { Injectable } from '@angular/core';
declare let alertify: any; // let tslint know that alertify.js is available globally

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {
  constructor() {}
  confirm = (message: string, okCallback: () => any) => {
    alertify.confirm(message, okClickEvent => {
      if (okClickEvent) {
        okCallback();
      }
    });
  }

  success = (message: string) => {
    alertify.success(message);
  }

  error = (message: string) => {
    alertify.error(message);
  }

  warning = (message: string) => {
    alertify.warning(message);
  }

  message = (message: string) => {
    alertify.message(message);
  }
}
