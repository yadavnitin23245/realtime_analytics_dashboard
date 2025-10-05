using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace RealtimeAnalytics.Api.Data
{
    public class ReadingStore : IDisposable
    {
        private readonly ConcurrentQueue<Reading> _queue = new();
        private readonly int _max;
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<Reading> _col;
        private readonly Timer _batchTimer;
        private readonly List<Reading> _batchBuffer = new();
        private readonly object _batchLock = new();

        public ReadingStore(IConfiguration cfg)
        {
            _max = cfg.GetSection("Retention").GetValue<int>("InMemoryMax", 100_000);
            var file = cfg.GetSection("LiteDb").GetValue<string>("FilePath") ?? "App_Data/realtime.db";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file)!);
            _db = new LiteDatabase($"Filename={file};Connection=shared");
            _col = _db.GetCollection<Reading>("readings");
            _col.EnsureIndex(x => x.Timestamp);

            // batch flush to DB every second
            _batchTimer = new Timer(_ => FlushBatch(), null, 1000, 1000);
        }

        public void Add(Reading r)
        {
            _queue.Enqueue(r);
            lock (_batchLock)
            {
                _batchBuffer.Add(r);
                if (_batchBuffer.Count >= 500)
                {
                    var toInsert = _batchBuffer.ToArray();
                    _batchBuffer.Clear();
                    _col.InsertBulk(toInsert);
                }
            }
            while (_queue.Count > _max && _queue.TryDequeue(out _)) { }
        }

        private void FlushBatch()
        {
            lock (_batchLock)
            {
                if (_batchBuffer.Count > 0)
                {
                    var toInsert = _batchBuffer.ToArray();
                    _batchBuffer.Clear();
                    _col.InsertBulk(toInsert);
                }
            }
        }

        public IEnumerable<Reading> Snapshot() => _queue.ToArray();

        public IEnumerable<Reading> GetSince(DateTimeOffset since)
        {
            var arr = _queue.ToArray();
            var newestTs = arr.LastOrDefault()?.Timestamp ?? DateTimeOffset.MinValue;
            if (newestTs - since <= TimeSpan.FromHours(1))
            {
                foreach (var r in arr.Where(x => x.Timestamp >= since)) yield return r;
                yield break;
            }

            var results = _col.Query()
                .Where(x => x.Timestamp >= since)
                .OrderBy(x => x.Timestamp)
                .ToEnumerable();
            foreach (var r in results) yield return r;
        }

        public int PurgeOlderThan(DateTimeOffset cutoff)
        {
            var deleted = _col.DeleteMany(x => x.Timestamp < cutoff);
            var removed = 0;
            while (_queue.TryPeek(out var r) && r.Timestamp < cutoff)
            {
                if (_queue.TryDequeue(out _)) removed++;
            }
            return deleted + removed;
        }

        public void Dispose()
        {
            _batchTimer?.Dispose();
            _db?.Dispose();
        }
    }
}