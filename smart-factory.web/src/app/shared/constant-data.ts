// Tất cả định dạng chữ
export function GetAllTextFormat() {
    return [
        {
            name: 'Bình thường',
            value: 0
        },
        {
            name: 'In đậm',
            value: 1
        },
        {
            name: 'In nghiêng',
            value: 2
        },
    ]
}

// Tất cả căn lề chữ
export function GetAllTextAlignment() {
    return [
        {
            name: 'Căn trái',
            value: -1
        },
        {
            name: 'Căn giữa',
            value: 0
        },
        {
            name: 'Căn phải',
            value: 1
        }
    ]
}

export function ConvertToPixel(value: number, unitType: number) {
    switch (unitType) {
        case 0: return value;
        case 1: return 2.8346456692913 * value;
        case 2: return 28.346456692913 * value;
        case 3: return 72 * value
    }
}

/// Danh sách các thuộc tính của hàng hóa
export const ProductProperties = {
    name: 'Name',
    size: 'Size',
    unit: 'Unit',
    class: 'Class'
}


/** Danh sách đơn vị tính */
export function GetAllUnits() {
    return [
        { key: 'Cái', value: 'Cái' },
        { key: 'Chai', value: 'Chai' },
        { key: 'Hũ', value: 'Hũ' },
        { key: 'Thỏi', value: 'Thỏi' },
        { key: 'Gói', value: 'Gói' },
        { key: 'Đôi', value: 'Đôi' },
        { key: 'Bộ', value: 'Bộ' },
        { key: 'Thùng', value: 'Thùng' },
        { key: 'Chiếc', value: 'Chiếc' },
        { key: 'Tấm', value: 'Tấm' },
        { key: 'Gói', value: 'Gói' },
        { key: 'Hộp', value: 'Hộp' },
        { key: 'Bao', value: 'Bao' },
        { key: 'Lọ', value: 'Lọ' },
        { key: 'Túi', value: 'Túi' },
        { key: 'Viên', value: 'Viên' },
        { key: 'Cuộn', value: 'Cuộn' },
        { key: 'Lon', value: 'Lon' },
        { key: 'Can', value: 'Can' },
        { key: 'Gram (g)', value: 'Gram (g)' },
        { key: 'Kilogram (kg)', value: 'Kilogram (kg)' },
        { key: 'Tấn', value: 'Tấn' },
        { key: 'Lít (L)', value: 'Lít (L)' },
        { key: 'Mililít (ml)', value: 'Mililít (ml)' },
        { key: 'Gallon', value: 'Gallon' },
        { key: 'Mét (m)', value: 'Mét (m)' },
        { key: 'Centimet (cm)', value: 'Centimet (cm)' },
        { key: 'Milimet (mm)', value: 'Milimet (mm)' },
        { key: 'Kilomet (km)', value: 'Kilomet (km)' },
        { key: 'Mét vuông (m²)', value: 'Mét vuông (m²)' },
        { key: 'Centimet vuông (cm²)', value: 'Centimet vuông (cm²)' },
        { key: 'Hecta (ha)', value: 'Hecta (ha)' },
        { key: 'Mét khối (m³)', value: 'Mét khối (m³)' },
        { key: 'Centimet khối (cm³)', value: 'Centimet khối (cm³)' }
    ]
}