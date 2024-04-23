# Deploy Ironbar using Docker locally

Make sure you have docker running. [Docker](https://www.docker.com/products/docker-desktop/)    

## Starting up boot_node  

Start boot node by running `docker-compose -f boot_node.docker-compose.yaml up --build`  

## Starting up a child_node
Start a child node  by running `docker-compose -f child_node.docker-compose.yaml up --build`.  At this time, the docker compose artifacts do not manually
create more child nodes and it has to be done manually.    

## If you want to run a two node system
Run `docker-compose -f two_node.docker-compose.yaml up --build`    



# Deploy Ironbar using Kubernetes

## Set up
1. Install [Docker](https://www.docker.com/products/docker-desktop/)  
2. Install [Tilt](https://docs.tilt.dev/install.html)  
2.1 Create a kubernetes cluster called `IronBar`.  eg: `kubectl config set-context ironbar`



# Deploying Ironbar to Azure

## Set up
https://medium.com/@bharatdwarkani/how-to-publish-asp-net-core-3-0-app-in-azure-linux-app-service-explained-from-scratch-6e45392ca256

## Setting start up
In azure app service, settings | configuration, go to general settings tab, add the following to `Startup Command`:  
`dotnet node.dll "/home/site/boot_node.config"`
 
Copy boot_node.config tp /home/site 
 
## Additional Notes
https://devblogs.microsoft.com/aspnet/grpc-web-for-net-now-available/

# Running locally with no docker or Kubernetes

1. Setup your environment.  Remember to changes these settings depending on if your running boot or child node. 

On Windows:  
```
SET IRONBAR_BOOT_SERVER=http://localhost:50051
SET IRONBAR_DATA_PATH=f:\temp\ironbar
SET IRONBAR_RPC_PORT=50055
SET IRONBAR_TYPE=child
```
2. run `node.exe`



