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
                        label: 'PMC',
                        icon: 'pi pi-fw pi-calendar',
                        routerLink: ['/pmc']
                    },
                    {
                        label: 'Chủ hàng',
                        icon: 'pi pi-fw pi-building',
                        routerLink: ['/customers']
                    },
                    {
                        label: 'Kho nguyên vật liệu',
                        icon: 'pi pi-fw pi-warehouse',
                        routerLink: ['/warehouse']
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
                            },
                            {
                                label: 'Nguyên vật liệu',
                                icon: 'pi pi-fw pi-circle',
                                routerLink: ['/system/materials']
                            },
                            {
                                label: 'Đơn vị tính',
                                icon: 'pi pi-fw pi-circle',
                                routerLink: ['/system/units-of-measure']
                            }
                        ]
                    }
                ]
            }
        ];
    }
}
