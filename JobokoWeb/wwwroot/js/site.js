var API_TOKEN = "",
    DIV_LOADING = "", API_URL = "", PAGE_SIZE = 50;
$.ajaxSetup({
    contentType: 'application/json',
    dataType: "json",
    crossDomain: true,
    xhrFields: {
        withCredentials: true
    },
    beforeSend: function (xhr) {
        xhr.setRequestHeader("Authorization", 'Bearer ' + API_TOKEN);
        $("body").append(DIV_LOADING);
    },
    success: function (data, textStatus, request) {
    },
    error: function (request, textStatus, errorThrown) {
        $("#div_loader").remove();
        if (request.status == 401) {
            let token_exp = request.getResponseHeader('token-expired');
            if (token_exp != null && token_exp == 'true') {
                document.location.href = "/";
            }
        }
    }, complete: function (xhr, status) {
        $("#div_loader").remove();
        let new_token = xhr.getResponseHeader('new-token');
        if (new_token != null && typeof new_token != 'undefined') {
            API_TOKEN = new_token;
        }
    }
});
$('.datepicker').datepicker({
    language: 'vi'
});
var customlogin = (callback) => {
    let obj = { "user": "admin", "pass": "" };
    $.ajax({
        type: "POST",
        contentType: 'application/json',
        dataType: "json",
        url: "https://localhost:44362/api/v1.0/token/login",
        data: JSON.stringify(obj),
        success: function (res) {
            $("#div_loader").remove();
            if (res.success) {
                API_TOKEN = res.token;
                if (typeof callback === "function") {
                    callback(res);
                }
            }
        },
        failure: function (response) {
            $("#div_loader").remove();
            console.log(`Lỗi xảy ra ${response.error}`, "error");
        },
        error: function (request, textStatus, errorThrown) {
            $("#div_loader").remove();
            console.log(`Lỗi xảy ra với API, vui lòng xem lại`, "error");
            if (request.status == 401) {
                let token_exp = request.getResponseHeader('token-expired');
                if (token_exp != null && token_exp == 'true') {
                    document.location.href = "/";
                }
                console.log(request.statusText);
            }
        }
    });
}
var paging = (total, func_name, page) => {
    page = parseInt(page);
    if (total === 0) {
        $('.paging').html(''); return;
    }
    let total_page = Math.ceil(total / PAGE_SIZE);
    //alert(total_page);
    let ext_space = false;
    let html_paging = `<div class="paginationjs-pages"><ul>`;
    if (page > 1)
        html_paging += `<li class="paging" onclick="${func_name}(${page - 1})"><a href="#">«</a></li>`;
    else
        html_paging += `<li class="paging"><a>«</a></li>`;
    if (total_page > 4) {
        if (page - 1 > 0) {
            html_paging += `<li class="paging 1" onclick="${func_name}(${page - 1})"><a href="#" data-href="${page - 1
                }">${page - 1}</a></li>`;
        }
        for (var i = 1; i <= total_page; i++) {
            if (i > 2 && i < total_page - 1) {
                if (!ext_space) {
                    ext_space = true;
                    html_paging += `<li class="current"><span class="font-weight-bold">${page}</span></li>`;
                    if (page < total_page) {
                        html_paging += `<li class="paging 1" onclick="${func_name}(${page + 1
                            })"><a href="#" data-href="${page + 1}">${page + 1}</a></li>`;
                    }
                    html_paging += `<b>trên ${total_page}</b>`;
                }

            } else {
                if (page === i) {

                } else {
                    if (i > 2 && i < total_page - 1) {
                        html_paging += `<li class="paging 2" onclick="${func_name}(${i})"><a href="#" data-href="${i
                            }">${i}</a></li>`;
                    }
                }
            }
        }
    } else {
        for (var j = 1; j <= total_page; j++) {
            if (j > 2 && j < total_page - 1) {
                if (!ext_space) {
                    ext_space = true;
                    html_paging += `...`;
                }
            } else {
                if (page === j) {
                    html_paging += `<li class="current"><span class="font-weight-bold">${j}</span></li>`;
                } else {
                    html_paging += `<li class="paging" onclick="${func_name}(${j})"><a href="#" data-href="${j}">${j}</a></li>`;
                }
            }
        }
    }

    if (page < total_page) {
        html_paging += `<li class="paging" onclick="${func_name}(${parseInt(page) + 1})"><a href="#">»</a></li>`;
    }
    else
        html_paging += `<li class="paging"><a>»</a></li>`;
    html_paging += `</ul></div>`;
    $('.paging').html(html_paging);
}
function convert(str) {
    var date = new Date(str),
        mnth = ("0" + (date.getMonth() + 1)).slice(-2),
        day = ("0" + date.getDate()).slice(-2);
    return [date.getFullYear(), mnth, day].join("-");
}
var reverseNumber = (input) => {
    return [].map.call(input, function (x) {
        return x;
    }).reverse().join('');
}

var plainNumber = (number) => {
    return number.split('.').join('');
}
var splitInDots = (input) => {
    try {
        var value = input.value,
            plain = plainNumber(value),
            reversed = reverseNumber(plain),
            reversedWithDots = reversed.match(/.{1,3}/g).join('.'),
            normal = reverseNumber(reversedWithDots);
        input.value = normal;
    } catch (e) {
        return e;
    }
};
var replaceDot = (str) => {
    try {
        if (str != null)
            str = str.replace(/\./g, '');
        return str;
    } catch (e) {
        return e;
    }
}
var epochToTime = (str) => {
    if (str === 0)
        return "";
    var date = new Date(str * 1000),
        month = ("0" + (date.getMonth() + 1)).slice(-2),
        day = ("0" + date.getDate()).slice(-2);
    return [day, month, date.getFullYear()].join("-");
}
var formatCurency = (val) => {
    while (/(\d+)(\d{3})/.test(val.toString())) {
        val = val.toString().replace(/(\d+)(\d{3})/, '$1' + '.' + '$2');
    }
    return val;
}
function toDate(dateStr) {
    try {
        var parts = dateStr.split("-");
        return new Date(parts[2], parts[1] - 1, parts[0]);
    } catch (e) {
        return new Date();
    }
}
function toDateWithHour(dateStr) {
    var parts_hour = dateStr.split(" ")[0].split(":");
    var parts = dateStr.split(" ")[1].split("-");
    return new Date(parts[2], parts[1] - 1, parts[0], parts_hour[0], parts_hour[1]);
}
function epochToTimeWithHour(str) {
    if (str === 0)
        return "";
    var date = new Date(str * 1000),
        month = ("0" + (date.getMonth() + 1)).slice(-2),
        day = ("0" + date.getDate()).slice(-2);
    return `${[date.getHours().toString().padStart(2, '0'), date.getMinutes().toString().padStart(2, '0')].join(":")} ${[day, month, date.getFullYear()].join("-")}`;
}
function callAPI(url, obj, method, callback) {
    if (obj == null) {
        $.ajax({
            type: method,
            url: `${url}`,
            success: function (res, textStatus, request) {
                $("#div_loader").remove();
                if (typeof callback === "function") {
                    callback(res);
                }
            },
            failure: function (response) {
                $("#div_loader").remove();
                $.notify(`Lỗi xảy ra ${response.error}`, "error");
            },
            error: function (response) {
                $("#div_loader").remove();
                $.notify(`Lỗi xảy ra với API, vui lòng xem lại`, "error");
            }
        });
    } else {
        $.ajax({
            type: method,
            contentType: 'application/json',
            dataType: "json",
            url: `${url}`,
            data: JSON.stringify(obj),
            success: function (res) {
                $("#div_loader").remove();
                if (typeof callback === "function") {
                    callback(res);
                }
            },
            failure: function (response) {
                $("#div_loader").remove();
                $.notify(`Lỗi xảy ra ${response.error}`, "error");
            },
            error: function (request, textStatus, errorThrown) {
                $("#div_loader").remove();
                $.notify(`Lỗi xảy ra với API, vui lòng xem lại`, "error");
                if (request.status == 401) {
                    let token_exp = request.getResponseHeader('token-expired');
                    if (token_exp != null && token_exp == 'true') {
                        document.location.href = "/";
                    }
                    console.log(request.statusText);
                }
            }
        });
    }
}

var epochTicks = 621355968000000000;
var ticksPerMillisecond = 10000;
var maxDateMilliseconds = 8640000000000000;
var congratulationsTimer;

function ticksToDateString(ticks) {
    if (isNaN(ticks)) {
        return "NANA-NA-NATNA:NA:BA.TMAN";
    }

    var ticksSinceEpoch = ticks - epochTicks;
    var millisecondsSinceEpoch = ticksSinceEpoch / ticksPerMillisecond;

    if (millisecondsSinceEpoch > maxDateMilliseconds) {
        return "+WHOAWH-OA-ISTOO:FA:RA.WAYZ";
    }

    var date = new Date(millisecondsSinceEpoch);
    //date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate()
    //return date.toISOString().substring(0, 10);
    return date.getDate() + '-' + (date.getMonth() + 1) + '-' + date.getFullYear();
};
var dateTimeInTicks = function (date) {
    return date.getTime() * 10000 + 621355968000000000;
}