# AI Conversation Log (summary)

- Disagreement 1: Rejected SQL in hot path; used LiteDB + in-memory store.
- Disagreement 2: Rejected REST polling; used WebSocket push.
- Disagreement 3: Rejected sending full 100k to frontend; used windowing.
- Performance issue rejection: Rejected per-reading WS broadcast; implemented batching + pre-serialization.