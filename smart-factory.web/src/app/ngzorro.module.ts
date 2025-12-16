import { NgModule } from '@angular/core';
import { Component } from '@angular/core';
import { NzButtonComponent } from 'ng-zorro-antd/button';
import { NzColorPickerComponent } from 'ng-zorro-antd/color-picker';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzCalendarModule, NzCalendarMode } from 'ng-zorro-antd/calendar';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzSliderModule } from 'ng-zorro-antd/slider';




@NgModule({
  declarations: [],
  imports: [
    NzButtonComponent,
    NzColorPickerComponent,
    NzDatePickerModule,
    NzCalendarModule,
    NzInputModule,
    NzSelectModule,
    NzDividerModule,
    NzUploadModule,
    NzModalModule,
    NzSliderModule,
  ],
  exports:[
    NzButtonComponent,
    NzColorPickerComponent,
    NzDatePickerModule,
    NzCalendarModule,
    NzInputModule,
    NzSelectModule,
    NzDividerModule,
    NzUploadModule,
    NzModalModule,
    NzSliderModule
  ]
})
export class NgzorroModule { }
