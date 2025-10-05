from locust import HttpUser, task, between

class StatsUser(HttpUser):
    wait_time = between(0.01, 0.05)
    @task(5)
    def stats(self):
        self.client.get("/api/stats")
    @task(1)
    def history(self):
        self.client.get("/api/history?hours=1")