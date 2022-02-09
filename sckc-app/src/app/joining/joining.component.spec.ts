import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JoiningComponent } from './joining.component';

describe('JoiningComponent', () => {
  let component: JoiningComponent;
  let fixture: ComponentFixture<JoiningComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ JoiningComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(JoiningComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
