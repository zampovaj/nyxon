using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.DTOs
{
    public class SnapshotsDto
    {
        List<Snapshot> Sending { get; set; } = new();
        List<Snapshot> Receiving { get; set; } = new();

        public SnapshotsDto()
        {
            Sending = new();
            Receiving = new();
        }

        public SnapshotsDto(List<Snapshot> sending, List<Snapshot> receiving)
        {
            Sending = sending;
            Receiving = receiving;
        }
    }
}