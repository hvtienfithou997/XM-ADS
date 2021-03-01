function onSubmit() {
    let user_name = $('#email').val();
    let full_name = $('#full_name').val();
    let password = $('#password').val();
    let email = $('#email').val();
    let roles = [];
    $("[name='role']:checked").each(function () {
        roles.push($(this).val());
    });
    var obj = {
        "user_name": user_name, "full_name": full_name, "password": password, "email": email, "old_password": "", "roles": roles
    };

    callAPI(`${API_URL}/user/add`, obj, "POST", function (res) {
        if (res.success) {
            $.notify("Thêm thành công", "success");
        } else {
            $.notify(`Lỗi xảy ra ${res.msg}`, "error");
        }
    });
};

function search(page) {
    let term = $(`[name='term']`).val();
    if (typeof page === 'undefined') {
        page = 1;
    }
    let id_team = $('#group_user').val();
    
    callAPI(`${API_URL}/User/search?term=${term}&id_team=${id_team}&page=${page}&page_size=${PAGE_SIZE}`, null, "GET", function (res) {
        if (res.success && res.data != null) {
            $(".totalRecs").html("Tổng số người dùng: " + res.total);
            
            let html = "";
            res.data.forEach(item => {
                html += `<tr>`;
                html += `<td>${item.user_name} (${item.full_name})<br>${item.email}</td>`;
                html += `<td>${item.email}</td>`;
                html += `<td>${item.team_name == null ? "" : item.team_name}</td>`;
                html += `<td>${item.type == 0 ? "USER" : "ADMIN"}</td>`;
                html += `<td>${epochToTime(item.last_login)} <br>${item.browser != null ? item.browser : ""}</td><td class="last-button"><a class="btn btn-warning" href="edit?id=${item.user_name}">Sửa</a>`;
                html += `<a class="btn btn-danger delete" href="#" id="${item.user_name}" onclick="deleteRec(this.id)">Xóa</a>`;
                html += `<button class="btn btn-danger" data-toggle="modal" data-target="#modal-reset-password" id="${item.user_name}" onclick="showForm(this.id)">Reset mật khẩu</button></td>`;
                html += `</tr>`;
            });
            $("#div_data").html(html);
        }
    });
}
function showForm(id) {
    let html = `<input type=text value="${id}" class='d-none' id="id-user">`;
    html += `<h4>Nhập mật khẩu mới</p>`;
    html += `<input type="password" class="form-control" id="password-input">`;
    $('#reset-password').html(html);
}
function resetPassword() {
    let id = $('#id-user').val();
    let reset_password = $('#password-input').val();
    var obj = {
        "id_user": id, "password": reset_password
    };
    callAPI(`${API_URL}/User/reset`, obj, "PUT", function (res) {
        
        if (res.success) {
            $.notify("Đặt lại mật khẩu thành công", "success");
            $('#modal-reset-password').modal('hide');
        } else {
            $.notify(`Lỗi xảy ra ${res.msg}`, "error");
        }
    });
}

function bindDetail(id) {
    callAPI(`${API_URL}/User/view?id=${id}`, null, "GET", function (res) {
        if (res.success) {
            if (res.data != null && res.data != undefined && res.data != "") {
                
                $("#user_detail").empty();
                $("#user_detail").append(`<li>Id :<span class="font-weight-bold">${res.data.id_user}</span></li>`);
                $("#user_detail").append(`<li>User name :<span class="font-weight-bold">${res.data.user_name}</span></li>`);
                $("#user_detail").append(`<li>Full Name :<span class="font-weight-bold">${res.data.full_name}</span></li>`);
                $("#user_detail").append(`<li>Id Team :<span class="font-weight-bold">${res.data.id_team}</span></li>`);
            }
        } else {
            $.notify(`Lỗi xảy ra ${res.msg}`, "error");
        }
    });
}
function deleteRec(id) {
    var mess = confirm("Bạn có muốn xóa bản ghi này không?");
    if (mess) {
        callAPI(`${API_URL}/user/delete?id=${id}`,
            null,
            "DELETE",
            function (res) {
                if (res) {
                    $.notify("Xóa thành công", "success");
                } else {
                    $.notify(`Lỗi xảy ra ${res.msg}`, "error");
                }
            });
    } else {
        return false;
    }
};