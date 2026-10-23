import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';

import { LandingComponent } from './landing.component';

describe('LandingComponent', () => {
  let fixture: ComponentFixture<LandingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      declarations: [LandingComponent],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(LandingComponent);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should route newcomers and members to their own pages', () => {
    const links: string[] = Array.from(
      fixture.nativeElement.querySelectorAll('a.landing-choice') as NodeListOf<HTMLAnchorElement>
    ).map((a) => a.getAttribute('href') ?? '');
    expect(links).toEqual(['/new-to-the-club', '/home']);
  });
});
