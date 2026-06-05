using ErmineGames.Utils;
using System.Collections.Generic;

namespace ErmineGames.Features
{
    public class DebugMessageJournal
    {
        private const int DefaultRecordsLimit = 1000;
        
        private LimitedSizeDictionary<int, JournalRecord> records;
        
        public int RecordsLimit { get; private set; }

        public DebugMessageJournal(int recordLimit = DefaultRecordsLimit)
        {
            RecordsLimit = recordLimit;
            records = new LimitedSizeDictionary<int, JournalRecord>(recordLimit);
        }

        public void AddRecord(int id, JournalRecord record)
        {
            records.Add(id, record);
        }

        public bool TryGetRecord(int id, out JournalRecord record)
        {
            return records.TryGetValue(id, out record);
        }

        public Dictionary<int, JournalRecord>.ValueCollection GetRecords()
        {
            return records.Values;
        }
    }
}
