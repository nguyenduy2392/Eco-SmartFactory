export function GetUser() {
    let user = localStorage.getItem("user");
    if (user) {
        return JSON.parse(user);
    }

    console.log(user);
    return {
        id: "ea740ce0-eda3-40e1-8ef7-8b7900c9e95e",
        status: 1,
        userName: "admin",
        email: "admin@pmbk.vn",
        avatar: " ",
        birthday: null,
        gender: 1,
        fullName: "Quản trị viên",
        password: "",
        address: "551 Nguyễn Bỉnh Khiêm, Đông Hải, Hải An, Hải Phòng",
        phone: "0775331777",
        mobile: null,
        yahoo: null,
        skype: null,
        facebook: null,
        detail: null,
        skin: null,
        lastLogin: null,
        parrentId: "ea740ce0-eda3-40e1-8ef7-8b7900c9e95e",
        companyId: null,
        isRootAdmin: true
    };
}