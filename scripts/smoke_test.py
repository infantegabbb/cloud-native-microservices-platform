import json
import sys
import time
import urllib.error
import urllib.request


def get(url):
    with urllib.request.urlopen(url, timeout=5) as response:
        return response.status, json.load(response)


