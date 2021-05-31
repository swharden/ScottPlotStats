# NuGet Package Popularity Tracker

**This project creates charts that show the historical download count of NuGet packages.** Data is collected using Azure Functions, stored in blob storage, and plotted using [ScottPlot](https://swharden.com/scottplot), and served as a static image. Creating charts as web-accessible images allows them to be included in GitHub readme files.

[NuGet Trends](https://nugettrends.com/packages?ids=ScottPlot&months=36) is a similar project, but their data is only updated once per week, and is only available online in JSON format.