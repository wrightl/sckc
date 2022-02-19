import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ActivitiesComponent } from './activities/activities.component';
import { PeopleComponent } from './people/people.component';
import { EventsComponent } from './events/events.component';
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

const routes: Routes = [
  { path: 'home', component: HomeComponent },
  { path: 'activities', component: ActivitiesComponent },
  { path: 'people', component: PeopleComponent },
  { path: 'documents', component: DocumentsComponent },
  { path: 'events', component: EventsComponent },
  { path: 'gallery/:id', component: GalleryComponent },
  { path: 'joining', component: JoiningComponent },
  { path: 'photos', component: PhotosComponent },
  { path: 'location', component: LocationComponent },
  { path: 'pool', component: PoolComponent },
  { path: 'river', component: RiverComponent },
  { path: 'reports', component: ReportsComponent },
  { path: 'about', component: AboutComponent },
  { path: 'contactus', component: ContactUsComponent },
  { path: '', redirectTo: '/home', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
