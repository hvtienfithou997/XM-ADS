#pragma checksum "F:\XmAds\JobokoWeb\Views\Home\Login.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "dbdf8f2262d2a1acea97dcf5013ed38e13ae042e"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_Login), @"mvc.1.0.view", @"/Views/Home/Login.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "F:\XmAds\JobokoWeb\Views\_ViewImports.cshtml"
using JobokoWeb;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "F:\XmAds\JobokoWeb\Views\_ViewImports.cshtml"
using JobokoWeb.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"dbdf8f2262d2a1acea97dcf5013ed38e13ae042e", @"/Views/Home/Login.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"55b3ce4724b2141f363937588dcafa91bfbb050e", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_Login : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 1 "F:\XmAds\JobokoWeb\Views\Home\Login.cshtml"
  
    ViewData["Title"] = "Privacy Policy";

#line default
#line hidden
#nullable disable
            WriteLiteral("<h1>");
#nullable restore
#line 4 "F:\XmAds\JobokoWeb\Views\Home\Login.cshtml"
Write(ViewData["Title"]);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"</h1>

<div class=""col-sm-12"">
    <div class=""form-group"">
        <div class=""col-sm-3""><button id=""btn_login"">Login</button></div>
    </div>
</div>
<script>
   
    $(function () {
        $(""#btn_login"").click(function () {
            
            let obj = { ""user"": ""admin"", ""pass"": """" };
            $.ajax({
                type: ""POST"",
                contentType: 'application/json',
                dataType: ""json"",
                url: ""https://localhost:44362/api/v1.0/token/login"",
                data: JSON.stringify(obj),
                success: function (res) {
                    $(""#div_loader"").remove();
                    if (res.success) {
                        API_TOKEN = res.token;
                    }
                },
                failure: function (response) {
                    $(""#div_loader"").remove();
                    console.log(`Lỗi xảy ra ${response.error}`, ""error"");
                },
                error: function (request, textSta");
            WriteLiteral(@"tus, errorThrown) {
                    $(""#div_loader"").remove();
                    console.log(`Lỗi xảy ra với API, vui lòng xem lại`, ""error"");
                    if (request.status == 401) {
                        let token_exp = request.getResponseHeader('token-expired');
                        if (token_exp != null && token_exp == 'true') {
                            document.location.href = ""/"";
                        }
                        console.log(request.statusText);
                    }
                }
            });
        });
    });
</script>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
