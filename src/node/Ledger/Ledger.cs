﻿using System;
using System.IO;
using Microsoft.Extensions.Logging;
using core.Ledger;

namespace node.Ledger
{
    /// <summary>
    /// A ledger, which consistents of blockchain entries
    /// There is at least 1 ledger, most likely multiple.  The
    /// ledger with ID=1 is the master ledger.
    /// 
    /// A ledger consists of an index, which is the list of all
    /// blocks in the ledger, and blocks, which is the data of the ledger.
    ///
    /// A block has two parts.  The block itself and a consenus entry which
    /// confirms all nodes running at the time agreed to the block.  The
    /// block and the consens entry are written to separate files.
    /// </summary>
    public class Ledger : ILedger
    {
        private readonly ILogger<Ledger> _logger;
        
        /// <summary>
        /// Ledger ID.  1 is master ledger.  It is controlled by the nodes
        /// with the master bootnode overall "overseer"
        /// </summary>
        public int Id { get; private set; } = 0;
        public string Name { get; private set; }
        /// <summary>
        /// root directory/folder where the ledgers are saved.  
        /// </summary>
        public string RootDataPath { get; private set; }
        public LedgerState State { get; private set; } = LedgerState.Nonfunctional;

        public ILedgerWriter Writer { get; private set; }
        
        public ILedgerReader Reader { get; private set; }

        private string LedgerPath => System.IO.Path.Combine(this.RootDataPath, this.Name);

        private string LedgerIndexFileName => System.IO.Path.Combine(LedgerPath, "index.txt");

        private LedgerIndexManager Indexes { get; set; }

        public Ledger(ILogger<Ledger> logger, int id, string name, string path)
        {
            _logger = logger;
            Id = id;
            Name = name;
            RootDataPath = path;
            Writer = new DefaultTextFileLedgerWriter(LedgerPath);
            Reader =  new DefaultTextFileLedgerReader(LedgerPath);
            Indexes = new LedgerIndexManager(Name, LedgerIndexFileName);
        }

        public void Initialize()
        {
            if (false == Directory.Exists(LedgerPath))
                Directory.CreateDirectory(LedgerPath);

            Indexes.Initialize();
            PhysicalBlock block = new PhysicalBlock
            {
                Id = Indexes.GetNextBlockId(),
                LedgerId = Id,
                TransactionData = System.Text.Encoding.ASCII.GetBytes("ledger initialized"),
                SignBlock = new SignBlock()
            };
            block.ComputeHash();

            LedgerIndex index = Indexes.Add(block.Hash, block.TimeStamp);

            if (block.Id != index.BlockId)
                throw new LedgerException(Name, $"ID mismatch {block.Id}/{index.BlockId}");

            // TODO: these next two steps are technically a transaction, need to determine what happens when
            // one step fails.
           _logger.LogInformation($"saving indexes to {LedgerIndexFileName}");
            // step 1 save the block
            // TODO: need rollback if Indexes.Save() fails
            Writer.SaveBlock(block);
            // step 2 save the index
            Indexes.Save();
        }

        public void Validate()
        {
            State = LedgerState.StartingUp;

            if (false == File.Exists(LedgerIndexFileName))
                throw new LedgerNotFoundException(LedgerIndexFileName);

            try
            {
                Indexes.Load();

                // Validation Rule #1:  the number of records in the index file should match
                // the number of blocks saved on disk
                if (Reader.CountBlocks() != Indexes.Count())
                    throw new LedgerNotValidException($"{Reader.CountBlocks()} != {Indexes.Count()}");

                // Validation Rule #2:  the boot record (which is the very first record) 
                // should be valid
                PhysicalBlock rootBlock = Reader.GetLedgerPhysicalBlock(1, (data) => {
                    return PhysicalBlock.FromString(data);
                }) as PhysicalBlock;

                LedgerIndex rootIndex = Indexes.GetIndex(1);

                if (rootBlock.ComputeHash() != rootIndex.Hash)
                    throw new LedgerNotValidException($"block {rootBlock.Id}");

                // Validation Rule #3: the last record should validate

                // Validation Rule #4: verify a few other blocks
                // TODO: validate some of the remaining blocks.
                // TODO: have an option that requires all blocks to be validated

                State = LedgerState.Available;
            }
            catch(Exception e)
            {
                State = LedgerState.Nonfunctional;
                throw new LedgerException(Name, e);
            }
            
        }
    }
}
