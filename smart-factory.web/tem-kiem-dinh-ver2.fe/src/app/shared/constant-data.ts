export function GetAllUnits() {
    return [
        {
            name: 'mm',
            value: 0
        },
        {
            name: 'cm',
            value: 1
        },
        {
            name: 'in',
            value: 2
        },
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