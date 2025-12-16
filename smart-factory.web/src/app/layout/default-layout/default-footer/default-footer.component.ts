import { Component } from '@angular/core';
import { LayoutService } from '../layout.service';

@Component({
    selector: 'app-default-footer',
    templateUrl: './default-footer.component.html',
    styleUrls: ['./default-footer.component.scss'],
    standalone: true,
})
export class DefaultFooterComponent {
  constructor(public layoutService:LayoutService) {

  }
}
