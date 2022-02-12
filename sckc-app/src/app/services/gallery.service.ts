import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Photos, PhotoSet } from '../models/photo-set';

@Injectable({
  providedIn: 'root',
})
export class GalleryService {
  constructor(private http: HttpClient) {}

  getPhotoSets() {
    return this.http.get<PhotoSet[]>(`${environment.baseApiUrl}gallery`);
  }

  getPhotos(id: string) {
    return this.http.get<Photos>(`${environment.baseApiUrl}gallery/${id}`);
  }
}
