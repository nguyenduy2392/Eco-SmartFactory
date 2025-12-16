import { inject, NgModule } from '@angular/core';
import { CommonModule, NgStyle } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { CurrencyMaskConfig, CurrencyMaskModule, CURRENCY_MASK_CONFIG } from 'ng2-currency-mask';
import { RouterModule } from '@angular/router';
import { registerLocaleData } from '@angular/common';
import en from '@angular/common/locales/en';
import vi from '@angular/common/locales/vi';
import id from '@angular/common/locales/id';
import { NgScrollbar } from 'ngx-scrollbar';
import { FormatDatePipe, FormatDateTimePipe, FormatGender, FormatInspectionType, FormatPricePipe, FormatRole, FormatUnit } from './shared/pipes';
import { FormatCurrencyMaskDirective } from './shared/directives';
import { NgApexchartsModule } from "ng-apexcharts";

registerLocaleData(vi);
registerLocaleData(en);
registerLocaleData(id);

export const CustomCurrencyMaskConfig: CurrencyMaskConfig = {
    align: "right",
    allowNegative: true,
    decimal: ",",
    precision: 0,
    prefix: "",
    suffix: "",
    thousands: "."
};

@NgModule({
    declarations: [
        FormatPricePipe,
        FormatDatePipe,
        FormatDateTimePipe,
        FormatUnit,
        FormatInspectionType,
        FormatRole,
        FormatGender,
        FormatCurrencyMaskDirective
    ],
    imports: [
        CommonModule,
        NgStyle,
        NgScrollbar,
        FormsModule,
        CurrencyMaskModule,
        RouterModule,
        ReactiveFormsModule,
        NgApexchartsModule
    ],
    exports: [
        FormsModule,
        CommonModule,
        NgStyle,
        NgScrollbar,
        CurrencyMaskModule,
        RouterModule,
        ReactiveFormsModule,
        FormatPricePipe,
        FormatDatePipe,
        FormatDateTimePipe,
        FormatUnit,
        FormatRole,
        FormatGender,
        FormatInspectionType,
        FormatCurrencyMaskDirective
    ],
    providers: [
        {
            provide: [CURRENCY_MASK_CONFIG], useValue: CustomCurrencyMaskConfig
        }
    ]
})
export class SharedModule { }
