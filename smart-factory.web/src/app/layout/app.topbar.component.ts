import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { LayoutService } from "./service/app.layout.service";
import { PrimengModule } from '../primeng.module';
import { SharedModule } from '../shared.module';
import { GetUser } from '../shared/common';
import { Router } from '@angular/router';

@Component({
    standalone: true,
    imports: [
        PrimengModule,
        SharedModule],
    selector: 'app-topbar',
    templateUrl: './app.topbar.component.html'
})
export class AppTopBarComponent implements OnInit {

    userMenus: MenuItem[] | undefined; // Các chức năng khi click vào biểu tượng user

    items!: MenuItem[];

    user: any = null;

    @ViewChild('menubutton') menuButton!: ElementRef;

    @ViewChild('topbarmenubutton') topbarMenuButton!: ElementRef;

    @ViewChild('topbarmenu') menu!: ElementRef;

    constructor(
        public layoutService: LayoutService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.user = GetUser();
        console.log(this.user);
        this.userMenus = [
            {
                label: this.user.name,
                styleClass: 'menu-w-18rem',
                items: [
                    {
                        label: 'Tài khoản',
                        icon: 'pi pi-user',
                        command: () => {
                        }
                    },
                    {
                        label: 'Đổi mật khẩu',
                        icon: 'pi pi-lock',
                        command: () => {
                        }
                    },
                    { separator: true },
                    {
                        label: 'Đăng xuất',
                        icon: 'pi pi-sign-out',
                        command: () => {
                            this.logout()
                        }
                    }
                ]
            }
        ];
    }
    
    logout() {
        // Xóa tất cả dữ liệu trong localStorage
        localStorage.clear();

        // Di chuyển về trang đăng nhập
        this.router.navigate(['/login']);
    }
}
