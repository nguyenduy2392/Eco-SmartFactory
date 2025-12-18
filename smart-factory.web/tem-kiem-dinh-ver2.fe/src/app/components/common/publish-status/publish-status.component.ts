import { Component, Input, OnInit } from '@angular/core';
import { PrimengModule } from '../../../primeng.module';
import { SharedModule } from 'primeng/api';

@Component({
  selector: 'app-publish-status',
  imports: [
    PrimengModule,
    SharedModule,
  ],
  standalone: true,
  templateUrl: './publish-status.component.html',
  styleUrls: ['./publish-status.component.css']
})
export class PublishStatusComponent implements OnInit {
  @Input() item: any; // Giá trị đã nhập vào input

  constructor() { }

  ngOnInit(): void { }
}
