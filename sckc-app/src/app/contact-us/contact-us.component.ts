import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { MatFormFieldControl } from '@angular/material/form-field';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-contact-us',
  templateUrl: './contact-us.component.html',
  styleUrls: ['./contact-us.component.scss'],
})
export class ContactUsComponent {
  public name: string = '';
  email = '';
  subject = '';
  message = '';

  constructor(private http: HttpClient) {}

  sendMessage() {
    this.http
      .post(`${environment.baseApiUrl}ContactUs`, {
        name: this.name,
        email: this.email,
        subject: this.subject,
        message: this.message,
      })
      .subscribe((result) => {
        console.log(result);
      });

    return false;
  }
}
