import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ActivitiesComponent } from './activities/activities.component';
import { PeopleComponent } from './people/people.component';
import { GalleryComponent } from './gallery/gallery.component';
import { PhotosComponent } from './photos/photos.component';
import { LocationComponent } from './location/location.component';
import { PoolComponent } from './pool/pool.component';
import { RiverComponent } from './river/river.component';
import { ReportsComponent } from './reports/reports.component';
import { HomeComponent } from './home/home.component';
import { JoiningComponent } from './joining/joining.component';
import { DocumentsComponent } from './documents/documents.component';
import { AboutComponent } from './about/about.component';
import { ContactUsComponent } from './contact-us/contact-us.component';
import { ClubHistoryComponent } from './club-history/club-history.component';
import { TripsComponent } from './trips/trips.component';
import { BookingSuccessComponent } from './booking-success/booking-success.component';
import { BookingComponent } from './booking/booking.component';
import { BookingRequestConfComponent } from './booking-request-conf/booking-request-conf.component';
import { MemberCheckComponent } from './member-check/member-check.component';
import { EnquireComponent } from './enquire/enquire.component';
import { ShePaddlesComponent } from './she-paddles/she-paddles.component';
import { LandingComponent } from './landing/landing.component';
import { NewToTheClubComponent } from './new-to-the-club/new-to-the-club.component';
import { SubmitReportComponent } from './submit-report/submit-report.component';

const routes: Routes = [
  { path: '', pathMatch: 'full', component: LandingComponent },
  { path: 'home', component: HomeComponent },
  { path: 'new-to-the-club', component: NewToTheClubComponent },
  { path: 'activities', component: ActivitiesComponent },
  { path: 'people', component: PeopleComponent },
  { path: 'documents', component: DocumentsComponent },
  { path: 'gallery/:id', component: GalleryComponent },
  { path: 'joining', component: JoiningComponent },
  { path: 'photos', component: PhotosComponent },
  { path: 'location', component: LocationComponent },
  { path: 'pool', component: PoolComponent },
  { path: 'enquire', component: EnquireComponent },
  { path: 'river', component: RiverComponent },
  { path: 'reports', component: ReportsComponent },
  { path: 'reports/write', component: SubmitReportComponent },
  { path: 'trips', component: TripsComponent },
  { path: 'about', component: AboutComponent },
  { path: 'contactus', component: ContactUsComponent },
  { path: 'history', component: ClubHistoryComponent },
  { path: 'member-check', component: MemberCheckComponent },
  { path: 'book', component: BookingComponent },
  { path: 'bookingsuccess', component: BookingSuccessComponent },
  { path: 'booking-conf', component: BookingRequestConfComponent },
  { path: 'she-paddles', component: ShePaddlesComponent },
  { path: '**', redirectTo: '/home', pathMatch: 'full' },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      initialNavigation: 'enabledBlocking',
      scrollPositionRestoration: 'enabled',
      anchorScrolling: 'enabled',
      // keep anchored headings clear of the sticky toolbar
      scrollOffset: [0, 80],
    }),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {}
