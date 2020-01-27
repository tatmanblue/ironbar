# Iron Bar

Iron is synonym for chain and bar is a synonym for block.  This project
is to develop a block chain protocol that can be used in authentication scenerios
like games.

It's not limited to games, just an example.


# Development
You can use any editor.  Solution and project files are included for use with
Visual Studio (both PC and Mac).  If you chose to use a different IDE please
make sure the solution and projects files are correctly updated in your pull request.

> dotnet --version  
> 3.0.100


# Design
### console project
The console project is obsolete and will be deleted soon.

### Nodes
Nodes are center piece of `Iron Bar`.  They handle all of the block chain transactions.

One running node will be sufficient to use `Iron Bar`, but that is not recommended.  At a minimum,
4 nodes should be running.   

In a 4+ node setup, one node works as the bootnode.  The bootnode is responsible for delegating
work to additional nodes as well as acting as the public endpoint to apps consuming `Iron Bar`.

The remaining nodes will be responsible for blockchain management:  creating new chains, validating 
(ak proofs), and distributed storage of chains (distributed block chain ledger).

When a new block is created, the bootnode will randomly assign creation to 3+ nodes and randomly assign
validation to one or more nodes.   

When there is only 1 node, the bootnode handles it all and there is no distributed block chain ledger.

### Gateway
For now, Gateway app is the web interface (web pages and restful API) to consuming Iron Bar.  

We would like to move the webapi into the bootnode if we can figure it out.  

### Credentials
The credentials project is an injectable service used by `nodes` to provide revokable credential services to `Iron Bar`.
It also serves as an example to the community how to extend `Iron Bar` functionality.

# Status
Prototype/irregular updates

IronBar is functional but functionality is limited.  Updates are made and project is active.

## Revision
2020.01.26
