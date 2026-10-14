import { Component, ChangeDetectionStrategy } from '@angular/core';
import { PoolInfo, POOLINFO } from '../data/pool';

@Component({
    selector: 'app-pool',
    templateUrl: './pool.component.html',
    styleUrls: ['./pool.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class PoolComponent {

  info: PoolInfo = POOLINFO;

}
