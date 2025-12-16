import { Component, OnInit } from '@angular/core';
import { PrimengModule } from '../../../primeng.module';
import { MegaMenuItem, MenuItem } from 'primeng/api';
import { Router } from '@angular/router';

@Component({
  selector: 'app-menu-bar',
  standalone: true,
  imports: [PrimengModule],
  templateUrl: './menu-bar.component.html',
  styleUrl: './menu-bar.component.scss'
})
export class MenuBarComponent implements OnInit {
  menuItems: MenuItem[] | undefined;

  constructor(private router: Router) {

  }
  ngOnInit(): void {
    this.menuItems = [
      {
        label: 'Tổng quan',
        icon: 'pi pi-home',
        command: () => {
          this.router.navigate(['/dashboard']);
        }
      },
      {
        label: 'Danh mục',
        icon: 'pi pi-file',
        items: [
          {
            label: 'Vật tư hàng hóa',
            icon: 'pi pi-circle',
            command: () => {
              this.router.navigate(['/products']);
            }
          },
          {
            label: 'Khách hàng',
            icon: 'pi pi-circle',
            command: () => {
              this.router.navigate(['/customers']);
            }
          }
        ]
      },

      {
        label: 'Phiếu xuất kho',
        icon: 'pi pi-qrcode',
        command: () => {
          this.router.navigate(['/diary-export']);
        }
      },
      {
        label: 'Hệ thống',
        icon: 'pi pi-cog',
        items: [
          {
            label: 'Người dùng',
            icon: 'pi pi-circle',
            command: () => {
              this.router.navigate(['/system/users']);
            }
          },
          {
            label: 'Thông tin công ty',
            icon: 'pi pi-circle',
            command: () => {
              this.router.navigate(['/system/unit-info']);
            }
          },
          // {
          //   label: 'Nhật ký hệ thống',
          //   icon: 'pi pi-circle',
          //   command: () => {
          //     this.router.navigate(['/system/history']);
          //   }
          // }
        ]
      }
    ];
  }
}
