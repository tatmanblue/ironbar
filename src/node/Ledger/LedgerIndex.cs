using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using core.Ledger;
using core.Utility;

namespace node.Ledger
{
    [Serializable]
    public class LedgerIndex
    {
        /// <summary>
        /// Ledger.Id
        /// </summary>
        public int BlockId { get; internal set; }
        public string Hash { get; internal set; }
        public DateTime Created { get; internal set; }

        public override string ToString()
        {
            return $"{BlockId}:{Hash}:{Created.ToFileDateTime()}";
        }

        public static LedgerIndex FromString(string data)
        {
            string[] elements = data.Split(":");
            LedgerIndex index = new LedgerIndex();
            index.BlockId = Convert.ToInt32(elements[0]);
            index.Hash = elements[1];
            DateTime expectedDate;
            if (false == DateTime.TryParseExact(elements[2], "dd MMMM yyyy HHmmss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out expectedDate))
                throw new LedgerNotValidException($"index is not valid");

            index.Created = expectedDate;
            return index;
        }
    }
}
