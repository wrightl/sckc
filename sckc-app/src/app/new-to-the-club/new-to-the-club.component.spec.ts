import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';

import { NewToTheClubComponent } from './new-to-the-club.component';
import { RIVERINFO } from '../data/river';

describe('NewToTheClubComponent', () => {
  let fixture: ComponentFixture<NewToTheClubComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      declarations: [NewToTheClubComponent],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(NewToTheClubComponent);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should show the non-member river price from the shared data', () => {
    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain(`£${RIVERINFO.adultSessionCostNonMember.toFixed(2)}`);
  });

  it('should deep-link to the kit lists', () => {
    const hrefs: string[] = Array.from(
      fixture.nativeElement.querySelectorAll('a') as NodeListOf<HTMLAnchorElement>
    ).map((a) => a.getAttribute('href') ?? '');
    expect(hrefs).toContain('/river#what-to-bring');
    expect(hrefs).toContain('/pool#what-to-bring');
  });
});
