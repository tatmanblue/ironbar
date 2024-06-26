﻿using System;
using System.Collections.Generic;

namespace core.Ledger
{

    /// <summary>
    /// 
    /// </summary>
    public interface ILedgerIndexManager
    {
        void Initialize();
        void InitializeFromSync(List<ILedgerIndex> rows);
        void Load();
        void Save();
        int Count();
        int GetNextBlockId();
        ILedgerIndex Add(string hash, DateTime TimeStamp, BlockStatus status);
        ILedgerIndex GetIndex(int id);
        List<ILedgerIndex> ListAllIndexes();

    }
}