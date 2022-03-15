import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-contact-us',
  templateUrl: './contact-us.component.html',
  styleUrls: ['./contact-us.component.scss'],
})
export class ContactUsComponent {
  name = new FormControl('', [Validators.required]);
  email = new FormControl('', [Validators.required, Validators.email]);
  subject = new FormControl('', [Validators.required]);
  message = new FormControl('', [Validators.required]);
  error = '';
  response = '';

  constructor(private http: HttpClient) {}

  getErrorMessage(field: FormControl) {
    if (field.hasError('required')) {
      return 'You must enter a value';
    }

    return field.hasError('email') ? 'Not a valid email' : '';
  }

  sendMessage() {
    this.error = '';
    this.response = '';

    this.name.markAsTouched();
    this.email.markAsTouched();
    this.subject.markAsTouched();
    this.message.markAsTouched();
    this.name.updateValueAndValidity();
    this.email.updateValueAndValidity();
    this.subject.updateValueAndValidity();
    this.message.updateValueAndValidity();

    if (
      this.name.errors ||
      this.email.errors ||
      this.subject.errors ||
      this.message.errors
    )
      return false;

    this.http
      .post(`${environment.baseApiUrl}ContactUs`, {
        name: this.name.value,
        email: this.email.value,
        subject: this.subject.value,
        message: this.message.value,
      })
      .subscribe(
        (result) => {
          this.response = result.toString();

          this.name.reset();
          this.subject.reset();
          this.email.reset();
          this.message.reset();
        },
        (error) =>
          (this.error =
            'Unfortunately an error occurred. Please try again, or send an email to <a href="mailto:enquiries@sheffieldcitykayakclub.co.uk">enquiries@sheffieldcitykayakclub.co.uk</a>')
      );

    return false;
  }
}
