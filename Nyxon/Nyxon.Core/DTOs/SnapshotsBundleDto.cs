using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.DTOs
{
    public class SnapshotsBundleDto
    {
        public List<Snapshot> Sending { get; set; } = new();
        public List<Snapshot> Receiving { get; set; } = new();

        public SnapshotsBundleDto()
        {
            Sending = new();
            Receiving = new();
        }

        public SnapshotsBundleDto(List<Snapshot> sending, List<Snapshot> receiving)
        {
            Sending = sending;
            Receiving = receiving;
        }
    }
}