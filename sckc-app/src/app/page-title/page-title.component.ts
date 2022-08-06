import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

@Component({
  selector: 'app-page-title',
  templateUrl: './page-title.component.html',
  styleUrls: ['./page-title.component.scss'],
})
export class PageTitleComponent implements OnInit, OnDestroy {
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

    this.titleService.setTitle(`${this.pageTitle ?? this.title} - Sheffield City Kayak Club`);
  }

  ngOnDestroy(): void {
    this.titleService.setTitle(`Sheffield City Kayak Club`);
    if (this._tag) this.metaTagService.removeTagElement(this._tag);
  }
}
