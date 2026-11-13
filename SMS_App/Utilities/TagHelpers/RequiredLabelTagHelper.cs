using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SMS_App.Utilities.TagHelpers;

[HtmlTargetElement("label", Attributes = ForAttributeName)]
public class RequiredLabelTagHelper : LabelTagHelper
{
    private const string ForAttributeName = "asp-for";
    public RequiredLabelTagHelper(IHtmlGenerator htmlGenerator): base(htmlGenerator)
    {
        
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        var isRequired = For?.Metadata.IsRequired ?? false;
        if (isRequired)
        {
            output.Content.AppendHtml(" <span class=\"text-danger\">*</span>");
        }
    }
}
