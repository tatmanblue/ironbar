# Nodes Architecture

In this distributed ledger system, there is one node to rule them all, so to speak.  It is called
the `boot node`.  All other nodes are called `child nodes`.

**Please keep in mind** that this document is part factual and part forward thinking.

For now, the child nodes are bit of dummy.  When they connect to the boot node, the index of all of the blocks
is shared with the child node.  And as new blocks are created, child nodes receive copies of these nodes.  This means
any given child node can have an imcomplete picture of the the ledger.  Thus the boot node is the deciding factor
in the ledger.

When blocks are created, child nodes will do validation against existing blocks.  In cases where a child is missing the 
blocks necesssary for validation, the child node will request these additional blocks before making the validation.