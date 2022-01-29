import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ActivitiesComponent } from './activities/activities.component';
import { ContactusComponent } from './contactus/contactus.component';
import { EventsComponent } from './events/events.component';
import { GalleryComponent } from './gallery/gallery.component';

const routes: Routes = [
  { path: 'activities', component: ActivitiesComponent },
  { path: 'contactus', component: ContactusComponent },
  { path: 'events', component: EventsComponent },
  { path: 'gallery', component: GalleryComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
