
// vd: DoiTuongParams : PagingParams
export class PagingParams {
    PageNumber?: number;
    PageSize?: number;
    Keyword?: string;
    SortValue?: string;
    SortKey?: string;
    fromDate?: string;
    toDate?: string;
    Filter?: any;
    filterColumns?: FilterColumn[];
}

export class FilterColumn {
    colKey?: string;
    colValue?: any;
    filterCondition?: FilterCondition;
    isFilter?: boolean;
    isString?: boolean;
    stt?: number;
    colNameVI?: string;
    constructor(colKey: string, colValue: string, filterCondition: FilterCondition, isFilter?: boolean) {
        this.colKey = colKey;
        this.colValue = colValue;
        this.filterCondition = filterCondition;
        this.isFilter = isFilter;
    }
}

export enum FilterCondition {
    Chua = 1,
    KhongChua = 2,
    Bang = 3,
    Khac = 4,
    BatDau = 5,
    KetThuc = 6,
    NhoHon = 7,
    NhoHonHoacBang = 8,
    LonHon = 9,
    LonHonHoacBang = 10,
    Trong = 11,
    KhongTrong = 12
}
