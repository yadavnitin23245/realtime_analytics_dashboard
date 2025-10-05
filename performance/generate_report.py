import datetime, subprocess, re, os

print("Running Locust tests...")
locust_cmd = "python -m locust -f locustfile.py --headless -u 200 -r 50 -t 30s -H https://localhost:53937 --only-summary"
locust_output = subprocess.getoutput(locust_cmd)

print("Running WebSocket test...")
ws_output = subprocess.getoutput("python ws_load.py --clients 100 --seconds 30 --uri wss://localhost:53937/stream")

report_path = "../docs/PerformanceReport.md"
now = datetime.datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S UTC")

with open(report_path, "w") as f:
    f.write(f"# Performance Report (Generated)\nTest date: {now}\n\n")
    f.write("## Locust Output\n```\n" + locust_output + "\n```\n\n")
    f.write("## WebSocket Output\n```\n" + ws_output + "\n```\n")

print(f"✅ Report generated at {report_path}")
