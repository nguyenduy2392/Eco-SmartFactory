import { Component, ElementRef, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { LayoutService } from '../layout.service';

@Component({
  selector: 'app-default-topbar',
  standalone: true,
  imports: [],
  templateUrl: './default-topbar.component.html',
  styleUrl: './default-topbar.component.scss'
})
export class DefaultTopBarComponent {
  items!: MenuItem[];

  @ViewChild('menubutton') menuButton!: ElementRef;

  @ViewChild('topbarmenubutton') topbarMenuButton!: ElementRef;

  @ViewChild('topbarmenu') menu!: ElementRef;

  constructor(public layoutService: LayoutService) { }
}
