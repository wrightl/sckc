import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { environment } from 'src/environments/environment';
import { Image } from '../carousel/interfaces';
import { GalleryService } from '../services/gallery.service';

@Component({
  selector: 'app-gallery',
  templateUrl: './gallery.component.html',
  styleUrls: ['./gallery.component.scss'],
})
export class GalleryComponent implements OnInit {
  isReady = false;
  images: Image[] = [];
  title = '';
  date?: Date;

  constructor(private service: GalleryService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') || '';
    this.service.getPhotos(id).subscribe((data) => {
      this.images = data.Photos.map((file) => ({
        path: `${environment.baseUrl}${file}`,
      }));
      this.title = data.Title;
      this.date = data.Date;
      this.isReady = true;
    });
  }
}
