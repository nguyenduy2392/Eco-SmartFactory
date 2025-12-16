import { LocalKeys } from "../../constant/app.constant";

export function objectKeysToArray(obj) {
    return Object.keys(obj);
}

export function Translations(translate: any, labelObject: any): void {
    let array = objectKeysToArray(labelObject);
    translate.get(array).subscribe((res: any) => labelObject = res);
}

export function IsNull(value: any): boolean {
    return value === null || value === undefined || value == "";
}

export function ToUrlParam(model: any) {
    let data = RemoveNullable(model);
    let params = Object.keys(data)
        .map(
            key => Array.isArray(data[key]) ?
                data[key].map(v => `${key}=${v}`).join('&') :
                `${key}=${data[key]}`
        )
        .join('&');
    return `?${params}`
}

// Function to remove nullable properties from an object
export function RemoveNullable(obj) {
    return Object.entries(obj)
        .filter(([_, v]) => v != null)
        .reduce((acc, [k, v]) => ({ ...acc, [k]: v }), {});
}

export function GetCompanyInfo() {
    let companyInfo = localStorage.getItem(LocalKeys.CompanyInfo);
    if (companyInfo) {
        return JSON.parse(companyInfo);
    }

    return {
        id: "",
        companyName: "",
        website: "",
        taxCode: "",
        address: "",
        email: "",
        logo: "",
        phone: "",
        chief: "",
        creator: "",
        recipient: "",
        warehouseKeeper: "",
        accountant: "",
        qrCode: ""
    }
}


/** Lấy thông tin người dùng từ localStorage */
export function GetUser() {
    let user = localStorage.getItem("user");
    if (user) {
        return JSON.parse(user);
    }
    return {
        id: "ea740ce0-eda3-40e1-8ef7-8b7900c9e95e",
        status: 1,
        userName: "",
        email: "",
        avatar: " ",
        birthday: null,
        gender: 1,
        fullName: "Quản trị viên",
        password: "",
        address: "",
        phone: "",
        mobile: null,
        yahoo: null,
        skype: null,
        facebook: null,
        detail: null,
        skin: null,
        lastLogin: null,
        parrentId: "",
        companyId: null,
        isRootAdmin: true
    };
}