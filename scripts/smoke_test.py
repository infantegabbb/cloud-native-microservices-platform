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


