import { Component, OnInit } from '@angular/core';
import { LayoutService } from '../layout.service';
import { CommonModule, NgFor, NgIf } from '@angular/common';

@Component({
  selector: 'app-default-menu',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './default-menu.component.html',
  styleUrl: './default-menu.component.scss'
})
export class DefaultMenuComponent implements OnInit {

  model: any[] = [];

  constructor(public layoutService: LayoutService) { }

  ngOnInit() {
      this.model = [
          {
              label: 'Home',
              items: [
                  { label: 'Dashboard', icon: 'pi pi-fw pi-home', routerLink: ['/'] }
              ]
          },
      ];
  }
}
