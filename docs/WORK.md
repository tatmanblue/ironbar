# Overview
updated: 2020.06.27

This document contains high level goals--think epic level.  Currently, you can run 1 boot node and several child nodes.  Blocks are copied and validated
between boot and child nodes.  There is a public grpc based API for creating and retrieving blocks (this is not documented at this time however).

This document hightlights two parts:
1. current goal
2. longer term

The main focus of this document is #1 above and notes for what will follow completing #1.

## CI/CD Updates
1. Update dotnet Version
2. Using Tilt and Helm Charts for running in docker which will allow for deployment in cloud services like AWS

## Version 0.0.3.0 Goal
Version 0.0.3.0  
To be able to deploy one or more interoperable nodes to Azure, AWS and locally 

### Immeditate Steps:
1. Fix interface mess.  Some things have interfaces some do not and those same "somes" are injected or not
2. Nodes talk to each other and know when they are up/down (done)
3. Boot ledger reads/writes (done)  
4. Child nodes copy ledger and validate blocks on create (done)


### To completion Steps
1. Nodes validate and approve ledger (partially done)



## Possible Extensions
1. Load Aries compatible agent as a plugin to ironbar with either [ping](https://github.com/hyperledger/aries-rfcs/tree/master/features/0048-trust-ping) or didcomm protocol working
