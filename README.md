# Iron Bar

Iron is synonym for chain and bar is a synonym for block.  This project
is to develop an [SSI system](https://sovrin.org/faq/what-is-self-sovereign-identity/) using block chain protocol that can be used in authentication scenerios
like games.

It's not limited to games, just an example.

### Architectural Goals
1. Server components can run on any OS
2. Secure using latest encryption standards
3. Client connectivity from any operating system
4. New use cases can be added by plug-in pattern
5. Simple to understand both in nomenclature and implementation

# Status
Prototype/irregular updates

This project is being updated with new functionality.  Please review [work.md](https://github.com/tatmanblue/ironbar/blob/master/docs/WORK.md) 
for details on goals and progress towards those goals.  There is no regular release schedule in place.  Work is completed on an adhoc schedule.

## File Revision
2021.06.05

## Legal
If you have any questions about the content of the repository, please email [matt.raffel@gmail.com](mailto:matt.raffel@gmail.com). I can assure you all content has been purchased and licensed to me or is otherwise freely available. Proof will be made available on request. Repeated DCMA counterfit and harassment claims will result in counter suits per Section 512(f) of the DMCA penalties for _misrepresentation can include actual damages and attorney’s fees_.

## License
Please read [license](https://github.com/tatmanblue/ironbar/blob/master/LICENSE).

## build status
[![CircleCI](https://circleci.com/gh/circleci/circleci-docs.svg?style=shield)](https://app.circleci.com/pipelines/github/tatmanblue/ironbar)

# Development
You can use any editor.  Solution and project files are included for use with
Visual Studio (both PC and Mac).  If you chose to use a different IDE please
make sure the solution and projects files are correctly updated in your pull request.

> dotnet --version  
> 3.0.100

# Source Code Structure/Projects

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
It also serves as a one example to the community how to extend `Iron Bar` functionality.

### The plug-in architecture
Iron bar is designed to be extensible through plug-ins.  See the [plug-in doc](https://github.com/tatmanblue/ironbar/blob/master/docs/PLUGINS.md) for
more details.  

