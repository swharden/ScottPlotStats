# NuGet Package Popularity Tracker

This project uses **Azure Functions** check the total download count for certain NuGet packages every hour and store the results in a blob storage table, the plot the results using [ScottPlot](https://swharden.com/scottplot/). Logic resides in [src/Functions.cs](src/Functions.cs) and the latest graphs can be viewed here:

https://swhardendev.z13.web.core.windows.net/
