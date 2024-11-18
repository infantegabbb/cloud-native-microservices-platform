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


