import { Component, Input, OnChanges, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

@Component({
    selector: 'app-page-title',
    templateUrl: './page-title.component.html',
    styleUrls: ['./page-title.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class PageTitleComponent implements OnInit, OnChanges, OnDestroy {
  @Input() title = '';
  @Input() pageTitle? : string = undefined;
  @Input() keywords = '';

  private _tag: HTMLMetaElement | null | undefined;

  constructor(private metaTagService: Meta, private titleService: Title){
  }

  ngOnInit(): void {
    this._tag = this.metaTagService.addTag({
      name: 'keywords',
      content: this.keywords,
    });
  }

  // Runs again when the title arrives later, e.g. a gallery's name once its photos have loaded
  ngOnChanges(): void {
    const pageName = this.pageTitle ?? this.title;
    this.titleService.setTitle(
      pageName ? `${pageName} - Sheffield City Kayak Club` : 'Sheffield City Kayak Club'
    );
  }

  ngOnDestroy(): void {
    this.titleService.setTitle(`Sheffield City Kayak Club`);
    if (this._tag) this.metaTagService.removeTagElement(this._tag);
  }
}
