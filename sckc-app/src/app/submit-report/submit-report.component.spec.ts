import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ReactiveFormsModule } from '@angular/forms';

import { SubmitReportComponent } from './submit-report.component';

describe('SubmitReportComponent', () => {
  let component: SubmitReportComponent;
  let fixture: ComponentFixture<SubmitReportComponent>;
  let http: HttpTestingController;

  const valid = {
    title: 'River Wye, Glasbury to Hay',
    date: '2026-10-18',
    trip: 'River Wye',
    report: 'We launched at Glasbury at ten. The river was low but lovely, and nobody swam.',
    author: 'Lee',
    email: 'lee@example.com',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SubmitReportComponent],
      imports: [ReactiveFormsModule],
      providers: [provideHttpClient(), provideHttpClientTesting()],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(SubmitReportComponent);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('does not send an incomplete report', () => {
    component.form.patchValue({ title: 'Too little' });
    component.submit();

    http.expectNone(() => true);
    expect(component.errorFor('report')).toBe('This is needed');
    expect(component.errorFor('email')).toBe('This is needed');
  });

  it('sends the report and shows the thank-you message', () => {
    component.form.patchValue(valid);
    component.submit();

    const request = http.expectOne((r) => r.method === 'POST' && r.url.endsWith('reports'));
    expect(request.request.body.title).toBe(valid.title);
    expect(request.request.body.website).toBe('');
    request.flush('Thanks for your trip report.');

    expect(component.submitted).toBe('Thanks for your trip report.');
    expect(component.form.controls.title.value).toBe('');
  });

  it('puts the API validation messages on their fields and keeps the text', () => {
    component.form.patchValue(valid);
    component.submit();

    http
      .expectOne((r) => r.method === 'POST')
      .flush({ errors: { date: ["The trip date can't be in the future."] } }, { status: 400, statusText: 'Bad Request' });

    expect(component.errorFor('date')).toBe("The trip date can't be in the future.");
    expect(component.error).toContain('highlighted fields');
    expect(component.form.controls.report.value).toBe(valid.report);
  });

  it('keeps the text when the server fails', () => {
    component.form.patchValue(valid);
    component.submit();

    http.expectOne((r) => r.method === 'POST').flush({}, { status: 500, statusText: 'Server Error' });

    expect(component.error).toContain("couldn't send your report");
    expect(component.form.controls.report.value).toBe(valid.report);
    expect(component.busy).toBeFalse();
  });
});
