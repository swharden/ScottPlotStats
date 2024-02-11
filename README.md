# ScottPlot NuGet Package Download Statistics

**This Azure Functions project logs and plots ScottPlot NuGet package downloads over time.** Data is collected using the NuGet API, stored as a CSV in Azure blob storage, plotted using [ScottPlot](https://swharden.com/scottplot), and served as a static image. Creating charts as web-accessible images allows them to be included in GitHub readme files.

[NuGet Trends](https://nugettrends.com/packages?ids=ScottPlot&months=36) is a similar project, but their data is only updated once per week, and they only serve data in JSON format (requiring JavaScript to plot it) so their graphs can not be displayed in GitHub readme files.

<div align="center">

<a href="https://scottplotstatsstorage.z20.web.core.windows.net/scottplot-download-count.png?nocache"><img src="https://scottplotstatsstorage.z20.web.core.windows.net/scottplot-download-count.png?nocache"></a>

</div>