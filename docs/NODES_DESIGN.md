# Nodes Architecture

In this distributed ledger system, there is one node to rule them all, so to speak.  It is called
the `boot node`.  All other nodes are called `child nodes`.

**Please keep in mind** that this document is part factual and part forward thinking.

For now, the child nodes are bit of dummy.  When they connect to the boot node, the index of all of the blocks
is shared with the child node.  And as new blocks are created, child nodes receive copies of these nodes.  This means
any given child node can have an incomplete picture of the the ledger.  Thus the boot node is the deciding factor
in the ledger.

When blocks are created, child nodes will do validation against existing blocks.  In cases where a child is missing the 
blocks necesssary for validation, the child node will request these additional blocks before making the validation.

## Start up

Boot node and child nodes start up in a similar manner.  See `LedgerManager.Start()` see [src](https://github.com/tatmanblue/ironbar/blob/main/src/node/Ledger/LedgerManager.cs) for code. 
The `Start()` method tries to be smart about what to do based on the state of the local store as well as node type.  It starts
by assuming the ledger data exists and if errors occur, then assumes the ledger data does not exist and tries to create it.

At a hight level, the start up is the same regardless of storage type (file based or object based).  The following
table summarizes the key functions that are called during the start up process.


| Function   | File Based                                                                                                                               | Object Based                                             |
|------------|------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------|
| Check      | Checks that directory structures exist and index file exists.  Either of these missing means assumes first time run and no ledger exists | Checks if the containers exist and the index.file exists |
| Validate   | For now, some simple checks that some key blocks and index values match                                                                  |                                                          |
| Initialize | Creates the directory structure, index file and initial starting block                                                                   | Creates the containers, index file and initial starting block |



When a child node starts up, it connects to the boot node.  The boot node shares the index of all blocks with the child node.
The child node then requests any blocks it does not have.
The boot node sends these blocks to the child node.
The child node validates these blocks and adds them to its local store.
The child node is now in sync with the boot node.