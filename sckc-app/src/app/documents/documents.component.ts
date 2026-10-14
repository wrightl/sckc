import { Component } from '@angular/core';
import { DOCUMENTS } from '../data/documents';

@Component({
    selector: 'app-documents',
    templateUrl: './documents.component.html',
    styleUrls: ['./documents.component.scss'],
    standalone: false
})
export class DocumentsComponent {
  documents = DOCUMENTS;
}
