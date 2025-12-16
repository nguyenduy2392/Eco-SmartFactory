import { Pipe, PipeTransform } from '@angular/core';
import moment from 'moment';

@Pipe({
    name: 'formatPrice'
})
export class FormatPricePipe implements PipeTransform {
    constructor() { }

    transform(value: any, dinhDangSo?: any): any {
        if (value == null || value === undefined || value === '' || value == 0) {
            return '0';
        }

        if (!dinhDangSo) {
            dinhDangSo = 0;
        }

        let val = (value / 1).toFixed(dinhDangSo).replace('.', ',');
        let result = val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.') === 'NaN' ? value : val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
        if (result === 'NaN') {
            return value < 0 ? `-${value}` : value;
        } else {
            if (value < 0) {
                value = Math.abs(value);
                val = (value / 1).toFixed(dinhDangSo).replace(',', '.');
                const split = val.split(',');
                const decimal = split.length > 1 ? split[1] : null;
                return `-${split[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.') + (decimal ? (',' + decimal) : '')}`;
            } else {
                const split = val.split(',');
                const decimal = split.length > 1 ? split[1] : null;
                return split[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.') + (decimal ? (',' + decimal) : '');
            }
        }
    }
}

@Pipe({
    name: 'formatDate'
})
export class FormatDatePipe implements PipeTransform {
    constructor() { }

    transform(value: string): any {
        return moment(value).format('DD-MM-YYYY');
    }
}

@Pipe({
    name: 'formatDateTime'
})
export class FormatDateTimePipe implements PipeTransform {
    constructor() { }

    transform(value: string): any {
        return moment(value).format('DD-MM-YYYY hh:mm:ss');
    }
}


@Pipe({
    name: 'formatUnit'
})
export class FormatUnit implements PipeTransform {
    constructor() { }

    transform(value: number): any {
        switch (value) {
            case 0: return 'mm';
            case 1: return 'cm';
            case 2: return 'in';

        }
    }
}


@Pipe({
    name: 'formatInspectionType'
})
export class FormatInspectionType implements PipeTransform {
    constructor() { }

    transform(value: number): any {
        switch (value) {
            case 0: return 'Thử nghiệm sản phẩm';
            case 1: return 'Giám định';
            case 2: return 'Kiểm định';
            case 3: return 'Chứng nhận phù hợp tiêu chuẩn, quy chuẩn';
            case 4: return 'Đánh giá sản phẩm, dịch vụ theo yêu cầu bên thứ 2';
            case 5: return 'Kiểm tra chất lượng nhà nước';
            default:
                return '';
        }
    }
}

@Pipe({
    name: 'formatRole'
})
export class FormatRole implements PipeTransform {
    constructor() { }

    transform(value: number): any {
        switch (value) {
            case 0: return 'Quản trị viên';
            case 1: return 'Chuyên viên kiểm định';
            case 2: return 'Đối tác';
            default:
                return '';
        }
    }
}

@Pipe({
    name: 'formatGender'
})
export class FormatGender implements PipeTransform {
    constructor() { }

    transform(value: number): any {
        switch (value) {
            case 0: return 'Nam';
            case 1: return 'Nữ';
            default:
                return 'Khác';
        }
    }
}
