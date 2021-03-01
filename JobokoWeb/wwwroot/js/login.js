$(document).ready(function() {
    $('#loginForm').submit(function (event) {
        event.preventDefault();
        let user = $('#username').val();
        let pass = $('#password').val();

        let url = API_URL + "/user/login";
        
            $.ajax({
                type: "POST",
                datatype: "json",
                headers: { "Content-Type": "application/json" },
                url: url,
                data: (JSON.stringify({
                    user: user,
                    pass: pass
                })),
                
                success: function(result) {
                    if (result.success){
                        $.notify(`Đăng nhập thành công, ${result.msg}"`, "success");
                        
                    } else {
                        $.notify(`${result.msg}"`, "error");
                        
                    }
                }
            });
            return false;
    });
});