# Iron Bar

Iron is synonym for chain and bar is a synonym for block.  This project
is to develop a [distributed ledger technologies](https://www.investopedia.com/terms/d/distributed-ledger-technology-dlt.asp) that can be used in various scenerios
like games, or [SSI system](https://sovrin.org/faq/what-is-self-sovereign-identity/) and so on.  Given there will be a plug-in model for extending core behaviors, 
the use cases are more varied.

### Architectural Goals
1. Server components can run on any OS
2. Secure using latest encryption standards
3. Client connectivity from any operating system
4. New use cases can be added by plug-in pattern
5. Simple to understand both in nomenclature and implementation

# Status
Prototype/irregular updates

### 2024.03.30 Update
Reopening this project to add CI/CD deployment to AWS or Azure.

This project is being updated with new functionality on an irregular basis.  Please review [work.md](https://github.com/tatmanblue/ironbar/blob/master/docs/WORK.md) 
for details on goals and progress towards those goals.  There is no regular release schedule in place.  Work is completed on an adhoc schedule.

## File Revision
2024.03.30


# License
The [license](LICENSE.md) included applies only to the files in this repo.   As the documentation states in the repo [readme.md](README.md), specific 3rd party assets are required to build and run
the libraries and demos in this project.  The license here does not apply to 3rd party assets. You agree to purchase those assets and abide by their licensing terms. 

```
   Copyright 2024 Matthew Raffel 

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
```

## Legal
If you have any questions about the content of the repository, please email [matt.raffel@gmail.com](mailto:matt.raffel@gmail.com). I can assure you all content has been developed by me or purchased and licensed to me. Proof will be made available on request. Repeated DCMA counterfit and harassment claims will result in counter suits per Section 512(f) of the DMCA penalties for _misrepresentation can include actual damages and attorney’s fees_.


## build status
[![CircleCI](https://circleci.com/gh/circleci/circleci-docs.svg?style=shield)](https://app.circleci.com/pipelines/circleci/6jcGFZ8866N3tDJ2DZL5q9/R4Wms16RDc8pLMdjPv3P8w)

# Development
You can use any editor.  Solution and project files are included for use with
Visual Studio (both PC and Mac).  If you chose to use a different IDE please
make sure the solution and projects files are correctly updated in your pull request.

> dotnet --version  
> 8.0.200

# Source Code Structure/Projects

### Nodes
Nodes are center piece of `Iron Bar`.  They handle all of the block chain transactions.

One running node will be sufficient to use `Iron Bar`, but that is not the intended design.  At this time, it has not 
been determined which configuration is optimal.  The goal is several nodes working together with one node
as the controller or boot node and the others functioning as creating block nodes and validation nodes.

The bootnode is responsible for delegating work to additional nodes as well as acting as the public endpoint to apps consuming `Iron Bar`.

The remaining nodes will be responsible for blockchain management:  creating new chains, validating , and distributed storage of chains (distributed block chain ledger).

When a new block is created, the bootnode will assign validation to all nodes.   

When there is only 1 node, the bootnode handles it all and there is no distributed block chain ledger.

### Consuming Iron Bar services

This is to be worked out, if the project is to cointue.  

1. gRPC clients should be able to interact with iron bar directly.  
2. A separate webapp with both UI and RESTful api that will also allow for consuming iron bar services.


### The plug-in architecture
Iron bar is designed to be extensible through plug-ins.  See the [plug-in doc](https://github.com/tatmanblue/ironbar/blob/master/docs/PLUGINS.md) for
more details.  

