using ErmineGames.Utils;
using System;
using System.Collections.Generic;

namespace ErmineGames.Features
{
    public class JournalRecord
    {
        public int RecordId { get; set; }
        public string DeclaringType { get; set; }
        public JournalContentType ContentType { get; set; }
        public ReflectionUtils.ReflectedData ReflectedData { get; set; }
        
        public DateTime RecordTime { get; set; }
        public DateTime RequestCreationTime { get; set; }
        public DateTime RequestProcessedTime { get; set; }
        
        public string CreatedByFeature { get; set; }
        public string ProcessedByFeature { get; set; }
        public List<string> ReadByFeatures { get; set; }
    }
}
