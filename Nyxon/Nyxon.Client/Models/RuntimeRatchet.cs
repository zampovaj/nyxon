using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class RuntimeRatchet
    {
        public SessionState Session { get; set; }
        private readonly SortedList<int, byte[]> _snapshotsHistory = new();

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
                if (!_snapshotsHistory.ContainsKey(snapshot.RotationIndex))
                {
                    _snapshotsHistory.Add(snapshot.RotationIndex, snapshot.EncryptedSessionKey);
                }
            }
        }
        public void AddSnapshot(Snapshot snapshot)
        {
            _snapshotsHistory.Add(snapshot.RotationIndex, snapshot.EncryptedSessionKey);
        }
    }
}