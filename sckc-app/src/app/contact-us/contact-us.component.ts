import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-contact-us',
  templateUrl: './contact-us.component.html',
  styleUrls: ['./contact-us.component.scss'],
})
export class ContactUsComponent {
  name = '';
  email = '';
  subject = '';
  message = '';
  error = '';
  response = '';

  constructor(private http: HttpClient) {}

  sendMessage() {
    this.error = '';
    this.response = '';

    let errorMessage = '';
    if (!this.name) errorMessage += 'Please enter your name<br/>';
    if (!this.email) errorMessage += 'Please enter an email address<br/>';
    if (!this.subject) errorMessage += 'Please enter a subject<br/>';
    if (!this.message) errorMessage += 'Please enter a message<br/>';

    if (errorMessage) {
      this.error = errorMessage;
      return false;
    }

    this.http
      .post(`${environment.baseApiUrl}ContactUs`, {
        name: this.name,
        email: this.email,
        subject: this.subject,
        message: this.message,
      })
      .subscribe(
        (result) => {
          this.response =
            'Thanks for getting in touch. Someone will reply to your email soon';
          this.name = '';
          this.subject = '';
          this.email = '';
          this.message = '';
        },
        (error) =>
          (this.error =
            'Unfortunately an error occurred. Please try again, or send an email to <a href="mailto:enquiries@sheffieldcitykayakclub.co.uk">enquiries@sheffieldcitykayakclub.co.uk</a>')
      );

    return false;
  }
}
