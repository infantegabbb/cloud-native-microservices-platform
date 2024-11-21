import json
import sys
import time
import urllib.error
import urllib.request


def get(url):
    with urllib.request.urlopen(url, timeout=5) as response:
        return response.status, json.load(response)


def post(url, payload):
    data = json.dumps(payload).encode()
    request = urllib.request.Request(
        url,
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(request, timeout=5) as response:
        return response.status, json.load(response)


def wait_for_health(name, url, timeout_seconds=60):
    deadline = time.time() + timeout_seconds
    last_error = None
    while time.time() < deadline:
        try:
            status, body = get(url)
            if 200 <= status < 300:
                print(f"[PASS] {name}: HTTP {status} {body}")
                return
            last_error = f"HTTP {status} {body}"
        except (urllib.error.URLError, TimeoutError, ConnectionError, json.JSONDecodeError) as err:
            last_error = err
        time.sleep(1)
    print(f"[FAIL] {name}: not reachable at {url} ({last_error})")
    print("Start the stack first: docker compose up --build -d")
    sys.exit(1)


def check(name, status, body):
    ok = 200 <= status < 300
    print(f"[{'PASS' if ok else 'FAIL'}] {name}: HTTP {status} {body}")
    if not ok:
        sys.exit(1)


wait_for_health("user health", "http://localhost:8080/health")
wait_for_health("order health", "http://localhost:8081/health")

try:
    status, user = post("http://localhost:8080/users", {"name": "Test User", "email": "test@example.com"})
    check("create user", status, user)
    status, order = post(
        "http://localhost:8081/orders",
        {"userId": user["id"], "product": "Cloud Book", "quantity": 2},
    )
    check("create order", status, order)
except urllib.error.HTTPError as err:
    print(f"[FAIL] API call: HTTP {err.code} {err.read().decode(errors='replace')}")
    sys.exit(1)
except (urllib.error.URLError, TimeoutError, ConnectionError, json.JSONDecodeError, KeyError) as err:
    print(f"[FAIL] API call: {err}")
    sys.exit(1)

print("All smoke tests passed.")
