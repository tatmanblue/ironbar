# APIs

There are two classes of APIs:  public and internal.

### Public API
Public API is for clients to create, retrieve, and individually validate data on the blockchain ledger.
The public API is consumed through RESTful services.  All of the public APIs are accessed through the gateway.

### Internal API
The Internal API is for the different apps in Iron Bar.

Currently there are two channels for interprocess communication. A RESTful endpoint exists
for the gateway to display administrative data and functions.  A gRPC channel has been setup between 
child nodes and the master node.

TODO: Using a message queue might be better than using gRPC.   
