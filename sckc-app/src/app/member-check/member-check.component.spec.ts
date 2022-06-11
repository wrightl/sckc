import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberCheckComponent } from './member-check.component';

describe('MemberCheckComponent', () => {
  let component: MemberCheckComponent;
  let fixture: ComponentFixture<MemberCheckComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MemberCheckComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MemberCheckComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
