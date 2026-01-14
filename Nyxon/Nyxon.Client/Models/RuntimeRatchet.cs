using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class RuntimeRatchet
    {
        public SessionState Session { get; set; }
        public SortedList<int, byte[]> SnapshotsHistory { get; private set; } = new();

        public RuntimeRatchet(SessionState session)
        {
            Session = session;
        }

        public RuntimeRatchet(RatchetState ratchet)
        {
            Session = ratchet.Session;
            MergeHistory(ratchet.Snapshots);
        }

        public void MergeHistory(IEnumerable<Snapshot> snapshots)
        {
            foreach (var snapshot in snapshots)
            {
                if (! SnapshotsHistory.ContainsKey(snapshot.RotationIndex))
                {
                     SnapshotsHistory.Add(snapshot.RotationIndex, snapshot.EncryptedSessionKey);
                }
            }
        }
        public void AddSnapshot(Snapshot snapshot)
        {
             SnapshotsHistory.Add(snapshot.RotationIndex, snapshot.EncryptedSessionKey);
        }
    }
}