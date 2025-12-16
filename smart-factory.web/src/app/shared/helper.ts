export function GenerateFile(data, fileName) {
    const dateOb = new Date();
    const day = ('0' + dateOb.getDate()).slice(-2);
    const month = ('0' + (dateOb.getMonth() + 1)).slice(-2);
    const year = dateOb.getFullYear();
    const hours = ('0' + dateOb.getHours()).slice(-2);
    const minute = ('0' + (dateOb.getMinutes() + 1)).slice(-2);
    const sec = ('0' + (dateOb.getSeconds() + 1)).slice(-2);

    var name = `${fileName} ${year}${month}${day}${hours}${minute}${sec}.xlsx`;

    const element = document.createElement('a');
    element.href = `data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,${data}`;
    element.download = name;
    element.click();
}