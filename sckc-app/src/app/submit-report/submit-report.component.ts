import { HttpErrorResponse } from '@angular/common/http';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ReportsService } from '../services/reports.service';

@Component({
    selector: 'app-submit-report',
    templateUrl: './submit-report.component.html',
    styleUrls: ['./submit-report.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class SubmitReportComponent {
  // Matches the limits the API enforces (TripReportSubmissionDto)
  readonly minReportLength = 50;
  readonly today = new Date().toISOString().slice(0, 10);

  form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(150)]],
    date: ['', [Validators.required]],
    trip: ['', [Validators.required, Validators.maxLength(150)]],
    triptype: ['', [Validators.maxLength(100)]],
    start: ['', [Validators.maxLength(150)]],
    triplength: ['', [Validators.maxLength(100)]],
    organiser: ['', [Validators.maxLength(100)]],
    coaches: ['', [Validators.maxLength(200)]],
    paddlers: ['', [Validators.maxLength(500)]],
    weather: ['', [Validators.maxLength(200)]],
    phrase: ['', [Validators.maxLength(150)]],
    report: ['', [Validators.required, Validators.minLength(this.minReportLength), Validators.maxLength(20000)]],
    author: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
    // Hidden from people; only spam bots fill it in
    website: [''],
  });

  busy = false;
  submitted = '';
  error = '';

  constructor(private fb: FormBuilder, private service: ReportsService) {}

  errorFor(name: keyof typeof this.form.controls): string {
    const control = this.form.controls[name];
    if (control.hasError('required')) {
      return 'This is needed';
    }
    if (control.hasError('email')) {
      return 'Enter an email address like name@example.com';
    }
    if (control.hasError('minlength')) {
      return `Write at least ${this.minReportLength} characters`;
    }
    if (control.hasError('maxlength')) {
      return `Keep this under ${control.getError('maxlength').requiredLength} characters`;
    }
    return control.getError('server') ?? '';
  }

  submit() {
    this.error = '';
    this.form.markAllAsTouched();
    if (this.form.invalid || this.busy) {
      return;
    }

    this.busy = true;
    this.service.submitReport(this.form.getRawValue()).subscribe({
      next: (message) => {
        this.busy = false;
        this.submitted = message;
        this.form.reset();
      },
      error: (err: HttpErrorResponse) => {
        this.busy = false;
        // A 400 lists the fields the API rejected; show each message on its field
        const fieldErrors: Record<string, string[]> = err.status === 400 ? err.error?.errors ?? {} : {};
        Object.entries(fieldErrors).forEach(([field, messages]) => {
          const control = this.form.get(field.charAt(0).toLowerCase() + field.slice(1));
          control?.setErrors({ server: messages[0] });
        });
        this.error = Object.keys(fieldErrors).length
          ? 'Some details need changing. Check the highlighted fields.'
          : "We couldn't send your report. Your text is still here, so try again in a few minutes, or email it to enquiries@sheffieldcitykayakclub.co.uk.";
      },
    });
  }

  writeAnother() {
    this.submitted = '';
  }
}
