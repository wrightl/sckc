import { Component, OnInit } from '@angular/core';
import { GalleryService } from '../services/gallery.service';
import { PhotoSet } from '../models/photo-set';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photos',
  templateUrl: './photos.component.html',
  styleUrls: ['./photos.component.scss']
})
export class PhotosComponent implements OnInit {

  isReady = false;
  photoSets: PhotoSet[] = [];
  baseUrl = environment.baseUrl;

  constructor(private service: GalleryService) { }

  ngOnInit(): void {
    this.service.getPhotoSets()
      .subscribe((data: PhotoSet[]) => {
        this.photoSets = data;
        this.isReady = true;
      });
  }

}
