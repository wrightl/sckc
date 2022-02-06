import { Component } from '@angular/core';
import { PoolInfo, POOLINFO } from '../data/pool';

@Component({
  selector: 'app-pool',
  templateUrl: './pool.component.html',
  styleUrls: ['./pool.component.scss']
})
export class PoolComponent {

  info: PoolInfo = POOLINFO;

}
