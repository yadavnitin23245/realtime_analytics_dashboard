# Decision Document

## Architecture
- Backend (.NET 8): BackgroundService simulator producing ~1k readings/sec, in-memory ConcurrentQueue (cap 100k), LiteDB for 24h persistence, WebSocket hub for broadcasts, REST for stats/history.
- Frontend: React + Vite + Recharts (windowed chart, latest 1k), WebSocket client, stats cards, anomalies highlighted.

## Key Decisions
- Use WebSocket for real-time push (lower latency).
- Keep hot data in memory for fast reads; persist to LiteDB for history.
- Batch WebSocket broadcasts at 20Hz to reduce serialization cost.
- Use Welford algorithm for streaming stats.