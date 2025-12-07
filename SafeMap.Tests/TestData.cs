using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeMap.Tests
{
    public enum DealerTrialStatusType
    {
        Unknown = 0,
        Active = 1,
        Paused = 2,
        Expired = 3
    }

    public class TrialRecord
    {
        public DateTime? TrialEndAt { get; set; }
        public int? StatusId { get; set; }
    }
}