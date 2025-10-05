import asyncio, websockets, time, argparse
async def runner(uri, seconds):
    cnt=0; start=time.time()
    async with websockets.connect(uri, ping_interval=None) as ws:
        while time.time()-start < seconds:
            try:
                msg = await asyncio.wait_for(ws.recv(), timeout=5)
                cnt += 1
            except asyncio.TimeoutError:
                pass
    return cnt
async def main():
    p=argparse.ArgumentParser(); p.add_argument('--uri', default='wss://localhost:53937/stream'); p.add_argument('--clients', type=int, default=50); p.add_argument('--seconds', type=int, default=30)
    args=p.parse_args()
    tasks=[runner(args.uri, args.seconds) for _ in range(args.clients)]
    results=await asyncio.gather(*tasks)
    print(f"Total messages received: {sum(results)} across {args.clients} clients in {args.seconds}s")
if __name__=='__main__': import asyncio; asyncio.run(main())