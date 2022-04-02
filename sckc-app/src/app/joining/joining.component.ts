import { Component, OnDestroy, OnInit } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { MEMBERSHIP_INFO } from '../data/membership';

@Component({
  selector: 'app-joining',
  templateUrl: './joining.component.html',
  styleUrls: ['./joining.component.scss'],
})
export class JoiningComponent implements OnInit, OnDestroy {
  membershipyear: string;
  info = MEMBERSHIP_INFO;
  private _tag: HTMLMetaElement | null | undefined;

  constructor(private metaTagService: Meta, private titleService: Title) {
    this.membershipyear = new Date().getFullYear().toString();
  }

  ngOnInit(): void {
    this._tag = this.metaTagService.addTag({
      name: 'keywords',
      content: 'membership,joining',
    });

    this.titleService.setTitle("Membership - Sheffield City Kayak Club");
  }

  ngOnDestroy(): void {
    if (this._tag) this.metaTagService.removeTagElement(this._tag);
  }
}
