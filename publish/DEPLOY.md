# Deploying Ironbar to Azure

## Set up
https://medium.com/@bharatdwarkani/how-to-publish-asp-net-core-3-0-app-in-azure-linux-app-service-explained-from-scratch-6e45392ca256

## Setting start up
In azure app service, settings | configuration, go to general settings tab, add the following to `Startup Command`:  
`dotnet node.dll "/home/site/boot_node.config"`
 
Copy boot_node.config tp /home/site 
 
## Additional Notes
https://devblogs.microsoft.com/aspnet/grpc-web-for-net-now-available/