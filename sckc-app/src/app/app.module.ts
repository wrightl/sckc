import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { APP_BASE_HREF } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTreeModule } from '@angular/material/tree';

import { NgbCarouselModule } from '@ng-bootstrap/ng-bootstrap';

import { environment } from 'src/environments/environment';
import { ActivitiesComponent } from './activities/activities.component';
import { PeopleComponent } from './people/people.component';
import { PersonComponent } from './person/person.component';
import { FooterComponent } from './footer/footer.component';
import { EventsComponent } from './events/events.component';
import { CarouselComponent } from './carousel/carousel.component';
import { GalleryComponent } from './gallery/gallery.component';
import { PhotosComponent } from './photos/photos.component';
import { HttpClientModule } from '@angular/common/http';
import { LocationComponent } from './location/location.component';
import { PoolComponent } from './pool/pool.component';
import { RiverComponent } from './river/river.component';
import { ReportsComponent } from './reports/reports.component';
import { TripReportComponent } from './trip-report/trip-report.component';
import { EventCalendarComponent } from './event-calendar/event-calendar.component';
import { HomeComponent } from './home/home.component';
import { JoiningComponent } from './joining/joining.component';
import { DocumentsComponent } from './documents/documents.component';
import { AboutComponent } from './about/about.component';
import { InfoCardComponent } from './info-card/info-card.component';
import { PageTitleComponent } from './page-title/page-title.component';
import { ContactUsComponent } from './contact-us/contact-us.component';
import { ClubHistoryComponent } from './club-history/club-history.component';
import { TripsComponent } from './trips/trips.component';

@NgModule({
  declarations: [
    AppComponent,
    ActivitiesComponent,
    PeopleComponent,
    PersonComponent,
    FooterComponent,
    EventsComponent,
    CarouselComponent,
    GalleryComponent,
    PhotosComponent,
    LocationComponent,
    PoolComponent,
    RiverComponent,
    ReportsComponent,
    TripReportComponent,
    EventCalendarComponent,
    HomeComponent,
    JoiningComponent,
    DocumentsComponent,
    AboutComponent,
    InfoCardComponent,
    PageTitleComponent,
    ContactUsComponent,
    ClubHistoryComponent,
    TripsComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    BrowserAnimationsModule,
    HttpClientModule,
    MatButtonModule,
    MatCardModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatSidenavModule,
    MatTableModule,
    MatToolbarModule,
    MatTreeModule,
    NgbCarouselModule,
  ],
  exports: [MatInputModule],
  providers: [{ provide: APP_BASE_HREF, useValue: environment.baseHref }],
  bootstrap: [AppComponent],
})
export class AppModule {}
//{ provide: APP_BASE_HREF, useValue: '/app' }
