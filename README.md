# NuGet Package Popularity Tracker

This Azure Functions project runs hourly to log total download count for certain NuGet packages and store the results in a SQL database. 

After running for an extended period of time, these records will be automatically plotted to reveal trends in NuGet package popularity over time.

This project recently started running (June 25, 2020) so come back later to see how it turns out...

You can check the current status of this Azure Function here:
https://packagepopularitytracker.azurewebsites.net/