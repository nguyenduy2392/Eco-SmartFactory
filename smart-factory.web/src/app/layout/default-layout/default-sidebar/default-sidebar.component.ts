import { Component, ElementRef } from '@angular/core';
import { LayoutService } from '../layout.service';
import { DefaultMenuComponent } from '../default-menu/default-menu.component';

@Component({
  selector: 'app-default-sidebar',
  standalone: true,
  imports: [DefaultMenuComponent],
  templateUrl: './default-sidebar.component.html',
  styleUrl: './default-sidebar.component.scss'
})
export class DefaultSideBarComponent {
  constructor(public layoutService: LayoutService, public el: ElementRef) { }
}
