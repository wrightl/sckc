import { Component, ChangeDetectionStrategy } from '@angular/core';
import { DOCUMENTS } from '../data/documents';

@Component({
    selector: 'app-documents',
    templateUrl: './documents.component.html',
    styleUrls: ['./documents.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class DocumentsComponent {
  documents = DOCUMENTS;
}
