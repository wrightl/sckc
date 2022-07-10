import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BusyButtonComponent } from './busy-button.component';

describe('BusyButtonComponent', () => {
  let component: BusyButtonComponent;
  let fixture: ComponentFixture<BusyButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BusyButtonComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BusyButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
