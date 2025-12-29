import { OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { LayoutService } from './service/app.layout.service';
import { SharedModule } from '../shared.module';
import { PrimengModule } from '../primeng.module';
import { AppMenuitemComponent } from './app-menu-item/app.menuitem.component';

@Component({
    standalone: true,
    imports: [SharedModule, PrimengModule, AppMenuitemComponent],
    selector: 'app-menu',
    templateUrl: './app.menu.component.html'
})
export class AppMenuComponent implements OnInit {

    model: any[] = [];

    constructor(public layoutService: LayoutService) { }

    ngOnInit() {
        this.model = [
            {
                label: 'Trang chủ',
                items: [
                    { label: 'Tổng quan', icon: 'pi pi-fw pi-home', routerLink: ['/dashboard'] }
                ]
            },
            {
                label: 'Quản lý sản xuất',
                icon: 'pi pi-fw pi-box',
                items: [
                    {
                        label: 'Processing PO',
                        icon: 'pi pi-fw pi-file-import',
                        routerLink: ['/processing-po']
                    },
                    {
                        label: 'Process BOM',
                        icon: 'pi pi-fw pi-sitemap',
                        routerLink: ['/process-bom']
                    },
                    {
                        label: 'Kiểm tra khả dụng NVL',
                        icon: 'pi pi-fw pi-check-circle',
                        routerLink: ['/availability-check']
                    },
                    {
                        label: 'Sản phẩm',
                        icon: 'pi pi-fw pi-box',
                        routerLink: ['/products']
                    },
                    {
                        label: 'Chủ hàng',
                        icon: 'pi pi-fw pi-building',
                        routerLink: ['/customers']
                    },
                    {
                        label: 'Vật tư',
                        icon: 'pi pi-fw pi-box',
                        routerLink: ['/materials']
                    },
                    {
                        label: 'Tool & Khuôn',
                        icon: 'pi pi-fw pi-wrench',
                        routerLink: ['/tools']
                    }
                ]
            },
            {
                label: 'Hệ thống',
                icon: 'pi pi-fw pi-cog',
                items: [
                    {
                        label: 'Người dùng',
                        icon: 'pi pi-fw pi-user',
                        items: [
                            {
                                label: 'Người dùng',
                                icon: 'pi pi-fw pi-circle',
                                routerLink: ['/system/users']
                            },
                            {
                                label: 'Vai trò',
                                icon: 'pi pi-fw pi-circle',
                                routerLink: ['/system/roles']
                            }
                        ]
                    },
                    {
                        label: 'Cài đặt',
                        icon: 'pi pi-fw pi-cog',
                        items: [
                            {
                                label: 'Thông tin đơn vị',
                                icon: 'pi pi-fw pi-circle',
                                routerLink: ['/system/unit-info']
                            }
                        ]
                    },
                ]
            },
        ];
    }
}
