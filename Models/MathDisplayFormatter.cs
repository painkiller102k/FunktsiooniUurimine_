using System.Text.RegularExpressions;
using System.Net;

namespace FunktsiooniUurimine.Models;

public static partial class MathDisplayFormatter
{
    public static string Format(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var text = WebUtility.HtmlEncode(value);

        text = Regex.Replace(text, "(?i)(sqrt|√)\\(([^)]+)\\)", "<span class=\"math-root\">√</span>($2)");
        text = Regex.Replace(text, @"\^([0-9]+)", "<sup>$1</sup>");
        text = text.Replace("f'(x)", "f′(x)");
        text = text.Replace("f''(x)", "f″(x)");
        text = text.Replace("x'", "x′");
        text = text.Replace("x''", "x″");

        return text;
    }
}
