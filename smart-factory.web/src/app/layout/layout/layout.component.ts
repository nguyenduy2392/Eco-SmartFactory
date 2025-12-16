import { Component } from '@angular/core';
import { TopBarComponent } from './top-bar/top-bar.component';
import { FooterComponent } from './footer/footer.component';
import { SideBarComponent } from './side-bar/side-bar.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [TopBarComponent, FooterComponent, SideBarComponent, CommonModule, RouterModule],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {

}
