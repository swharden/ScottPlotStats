using ScottPlot;
using System.Text;

namespace ScottPlotStats.Test;

internal class HtmlReport
{
    readonly StringBuilder SB = new();

    public string Title { get; set; } = "ScottPlot Report";

    public string Template = """
        <!doctype html>
        <html lang="en">
          <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{{TITLE}}</title>
            <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
          </head>
          <body>
            <div class="container">
              <main>{{CONTENT}}</main>
              <footer class='my-5'>{{FOOTER}}</footer>
            </div>
            <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz" crossorigin="anonymous"></script>
          </body>
        </html>
        """;

    public void AddEmbeddedSvg(Plot plot, int width = 600, int height = 400)
    {
        SB.AppendLine(plot.GetSvgXml(width, height));
    }

    public string GetHtml()
    {
        return Template
            .Replace("{{TITLE}}", Title)
            .Replace("{{FOOTER}}", $"Generated {DateTime.Now}")
            .Replace("{{CONTENT}}", SB.ToString());
    }

    public void WriteAllText(string file)
    {
        file = Path.GetFullPath(file);
        File.WriteAllText(file, GetHtml());
        Console.WriteLine($"Wrote: {file}");
    }
}
