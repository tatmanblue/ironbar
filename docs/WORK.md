# Overview
updated: 2025.08.11

Boot and child nodes can be run locally or on the cloud, or a combination of both.  When running a node on the cloud, it recommended the run
the boot nodes locally due to the system using file system for storage.  If you want to run the boot nodes on the cloud, you will need to make sure
the virtual machine has a persistent disk mounted to the file system.  Boot node and child nodes use a form of BFT for determining
if a block is valid.  The boot node creates the genesis block on ledger initialization and manages the child nodes interactions.

## Goal:  flexible storage
1. Extract out interfaces for storage (done)
2. Support for object storage.  Considering using [minio](https://github.com/minio/minio-dotnet) since it supports S3, Azure, and Google Cloud Storage.
3. Easy configuration of storage via json or environment variables (done)  
4. Clean up the intefaces around Ledger, LedgerIndex and construction of objects

## Goal: Configuration and Operation
1. A blazor dashboard to see state and manage the nodes 
2. Extensions to the AspNet Core interface to allow for configuration and creation of nodes via a web interface

## Goal: Finish and expand block validation checks
1. Add more validation checks to the block validation process

## Possible Extensions
1. Load Aries compatible agent as a plugin to ironbar with either [ping](https://github.com/hyperledger/aries-rfcs/tree/master/features/0048-trust-ping) or didcomm protocol working
2. Integrate with [Arcus](https://github.com/tatmanblue/Arcus)  
