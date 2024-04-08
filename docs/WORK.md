# Overview
updated: 2020.06.27

This document contains high level goals--think epic level.  
See the [project board](https://github.com/users/tatmanblue/projects/2) for more detailed plans--think story level.

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
2. Nodes talk to each other and know when they are up/down (1/2 done client informs boot)
3. Boot ledger reads/writes (done)


### To completion Steps
1. Node validate and approve


## Possible Extensions
1. Load Aries compatible agent as a plugin to ironbar with either [ping](https://github.com/hyperledger/aries-rfcs/tree/master/features/0048-trust-ping) or didcomm protocol working
