import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BookingRequestConfComponent } from './booking-request-conf.component';

describe('BookingRequestConfComponent', () => {
  let component: BookingRequestConfComponent;
  let fixture: ComponentFixture<BookingRequestConfComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BookingRequestConfComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BookingRequestConfComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
