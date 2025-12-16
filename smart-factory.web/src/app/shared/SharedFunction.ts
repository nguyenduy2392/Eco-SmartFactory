export function interpolateString(str: string, params: any): string {
    return str.replace(/{{(\w+)}}/g, (match, p1) => params[p1] || '');
}
export function sumwidthConfig(widthConfig: any[]): string {
    let sum = 0;

    widthConfig.forEach(element => {
        let width = +element.substr(0, element.length - 2);
        sum += width
    });
    return sum + 'px';
}
interface LabelObj {
    name: string;
    label: string;
    width: string;
    ord: number;
    isShow: string;
}
export function toLowerCaseFirst(str: string): string {
    if (!str) return str; // Kiểm tra xem chuỗi có hợp lệ không
    return str.charAt(0).toLowerCase() + str.slice(1); // Chuyển đổi chữ cái đầu tiên và nối với phần còn lại của chuỗi
  }
export function sortColume(labels: { [key: string]: LabelObj }) {
    const labelsArray: LabelObj[] = Object.entries(labels).map(([key, value]) => {
        return { name: toLowerCaseFirst(key), ...value } as LabelObj;
    });
    labelsArray.sort((a, b) => a.ord - b.ord);
    return labelsArray;
}

/// Kiểm tra tính hợp lệ của emmail
export function isValidEmail(email: string): boolean {
    // Biểu thức chính quy kiểm tra email hợp lệ
    const emailRegex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailRegex.test(email);
  }