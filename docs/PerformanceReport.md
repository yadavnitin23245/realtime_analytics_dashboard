# Performance Report (Generated)
Test date: 2025-10-05 09:20:58 UTC

## Locust Output
```
[2025-10-05 14:50:26,067] JAYCHAUHAN/INFO/locust.main: Starting Locust 2.41.3
[2025-10-05 14:50:26,069] JAYCHAUHAN/INFO/locust.main: Run time limit set to 30 seconds
[2025-10-05 14:50:26,072] JAYCHAUHAN/INFO/locust.runners: Ramping to 200 users at a rate of 50.00 per second
[2025-10-05 14:50:29,245] JAYCHAUHAN/INFO/locust.runners: All users spawned: {"StatsUser": 200} (200 total users)
[2025-10-05 14:50:46,275] JAYCHAUHAN/WARNING/root: CPU usage above 90%! This may constrain your throughput and may even give inconsistent response time measurements! See https://docs.locust.io/en/stable/running-distributed.html for how to distribute the load over multiple CPU cores or machines
[2025-10-05 14:50:53,688] JAYCHAUHAN/INFO/locust.main: --run-time limit reached, shutting down
[2025-10-05 14:50:54,323] JAYCHAUHAN/WARNING/locust.runners: CPU usage was too high at some point during the test! See https://docs.locust.io/en/stable/running-distributed.html for how to distribute the load over multiple CPU cores or machines
[2025-10-05 14:50:54,324] JAYCHAUHAN/INFO/locust.main: Shutting down (exit code 1)
Type     Name                                                                          # reqs      # fails |    Avg     Min     Max    Med |   req/s  failures/s
--------|----------------------------------------------------------------------------|-------|-------------|-------|-------|-------|-------|--------|-----------
GET      /api/history?hours=1                                                             957 957(100.00%) |    657     100   13711    140 |   34.01       34.01
GET      /api/stats                                                                      4620 4620(100.00%) |    703     104   13912    140 |  164.17      164.17
--------|----------------------------------------------------------------------------|-------|-------------|-------|-------|-------|-------|--------|-----------
         Aggregated                                                                      5577 5577(100.00%) |    695     100   13912    140 |  198.18      198.18

Response time percentiles (approximated)
Type     Name                                                                                  50%    66%    75%    80%    90%    95%    98%    99%  99.9% 99.99%   100% # reqs
--------|--------------------------------------------------------------------------------|--------|------|------|------|------|------|------|------|------|------|------|------
GET      /api/history?hours=1                                                                  140    150    150    160    210   3300  12000  13000  14000  14000  14000    957
GET      /api/stats                                                                            140    150    150    160    220   3700  12000  13000  14000  14000  14000   4620
--------|--------------------------------------------------------------------------------|--------|------|------|------|------|------|------|------|------|------|------|------
         Aggregated                                                                            140    150    150    160    220   3400  12000  13000  14000  14000  14000   5577

Error report
# occurrences      Error                                                                                               
------------------|---------------------------------------------------------------------------------------------------------------------------------------------
4620               GET /api/stats: SSLError(SSLCertVerificationError(1, '[SSL: CERTIFICATE_VERIFY_FAILED] certificate verify failed: self signed certificate (_ssl.c:992)'))
957                GET /api/history?hours=1: SSLError(SSLCertVerificationError(1, '[SSL: CERTIFICATE_VERIFY_FAILED] certificate verify failed: self signed certificate (_ssl.c:992)'))
------------------|---------------------------------------------------------------------------------------------------------------------------------------------

```

## WebSocket Output
```
Traceback (most recent call last):
  File "F:\Projects\NITIN_YADAV\realtime-analytics-dashboard-complete\performance\ws_load.py", line 18, in <module>
    if __name__=='__main__': import asyncio; asyncio.run(main())
                                             ^^^^^^^^^^^^^^^^^^^
  File "C:\Program Files\Python311\Lib\asyncio\runners.py", line 190, in run
    return runner.run(main)
           ^^^^^^^^^^^^^^^^
  File "C:\Program Files\Python311\Lib\asyncio\runners.py", line 118, in run
    return self._loop.run_until_complete(task)
           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
  File "C:\Program Files\Python311\Lib\asyncio\base_events.py", line 650, in run_until_complete
    return future.result()
           ^^^^^^^^^^^^^^^
  File "F:\Projects\NITIN_YADAV\realtime-analytics-dashboard-complete\performance\ws_load.py", line 16, in main
    results=await asyncio.gather(*tasks)
            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
  File "F:\Projects\NITIN_YADAV\realtime-analytics-dashboard-complete\performance\ws_load.py", line 4, in runner
    async with websockets.connect(uri, ping_interval=None) as ws:
  File "C:\Users\jay\AppData\Roaming\Python\Python311\site-packages\websockets\asyncio\client.py", line 587, in __aenter__
    return await self
           ^^^^^^^^^^
  File "C:\Users\jay\AppData\Roaming\Python\Python311\site-packages\websockets\asyncio\client.py", line 541, in __await_impl__
    self.connection = await self.create_connection()
                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
  File "C:\Users\jay\AppData\Roaming\Python\Python311\site-packages\websockets\asyncio\client.py", line 467, in create_connection
    _, connection = await loop.create_connection(factory, **kwargs)
                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
  File "C:\Program Files\Python311\Lib\asyncio\base_events.py", line 1098, in create_connection
    transport, protocol = await self._create_connection_transport(
                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
  File "C:\Program Files\Python311\Lib\asyncio\base_events.py", line 1131, in _create_connection_transport
    await waiter
  File "C:\Program Files\Python311\Lib\asyncio\sslproto.py", line 577, in _on_handshake_complete
    raise handshake_exc
  File "C:\Program Files\Python311\Lib\asyncio\sslproto.py", line 559, in _do_handshake
    self._sslobj.do_handshake()
  File "C:\Program Files\Python311\Lib\ssl.py", line 979, in do_handshake
    self._sslobj.do_handshake()
ssl.SSLCertVerificationError: [SSL: CERTIFICATE_VERIFY_FAILED] certificate verify failed: self signed certificate (_ssl.c:992)
```
