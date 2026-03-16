# Project

Iron Bar is a distributed ledger technology (DLT) implementation built on .NET 9.0 that provides a blockchain-based data persistence layer suitable for multiple domains including gaming applications, Self-Sovereign Identity (SSI) systems, and custom enterprise workflows. The system achieves distributed consensus through Byzantine Fault Tolerance (BFT) and supports flexible deployment models ranging from single-node development environments to multi-node cloud infrastructures.

The architecture prioritizes:

Cross-platform operation - Runs on any operating system supporting .NET 9.0
Storage flexibility - Abstract storage layer supporting local filesystem and Azure Blob Storage
Plugin extensibility - Domain-specific behaviors through IPlugin interface implementations
Simplicity - Clear nomenclature and implementation patterns for maintainability
Security - API key-based access control and cryptographic block validation


## Instructions for Claude

All changes must be approved before creating these changes.  Please prepare a plan of proposed changes and get confirmation before proceeding.


## Additional documentation
- [README.md](README.md)  
- [API Reference](docs/API.md)  
- [Node Design](docs/NODES_DESIGN.md)  
- [Plugin System](docs/PLUGINS.md)  
- [Terminology](docs/TERMS.md)  
- [DeepWiki](https://deepwiki.com/tatmanblue/ironbar)  

## Architecture
Iron Bar implements a hub-and-spoke distributed ledger architecture where a single boot node coordinates consensus among multiple child nodes. The boot node maintains authoritative control over block creation and validation orchestration, while child nodes participate in validation and maintain replicated ledger state.

Nodes are center piece of Iron Bar. They handle all of the block chain transactions.  There are two types of nodes: boot node and child nodes.   All nodes handle block chain creation and management.  The bootnode is responsible for delegating work to additional nodes as well as acting as the public endpoint to apps consuming Iron Bar. In a healthy system there is only one boot node and multiple child nodes.  All nodes communicate using GRPC.

One running node will be sufficient to use Iron Bar, but that is not the intended design. At this time, it has not been determined which configuration is optimal. The goal is several nodes working together with one node as the controller or boot node and the others functioning as creating block nodes and validation nodes.

The remaining nodes will be responsible for blockchain management: creating new chains, validating , and distributed storage of chains (distributed block chain ledger).

When a new block is created, the bootnode will assign validation to all nodes.

When there is only 1 node, the bootnode handles it all and there is no distributed block chain ledger.

Boot and child nodes can be run locally or on the cloud, or a combination of both. When running a node on the cloud, it recommended use the Azure Blob for storage. Nodes running on bare metal can use any storage (currently either file based or Azure Blob). Boot node and child nodes use a form of BFT for determining if a block is valid. The boot node creates the genesis block on ledger initialization and manages the child nodes interactions.

Key Architectural Principles:


| Principle	     | Implementation            | 
| -------------- | ------------------------- |  
| Single Binary Model | One executable configured via environment variables | 
| Configuration-Driven | All behavior controlled through IRONBAR_* environment variables | 
| Layered Design | Clear separation: Application → Service → Domain → Data Access → Infrastructure | 
| Interface-Based Abstraction | Storage (ILedgerReader/ILedgerWriter), plugins (IPlugin), configuration (IConfiguration) | 
| Dependency Injection | DI container for all component wiring | 
| Protocol Buffers | Type-safe gRPC communication via .proto definitions |

