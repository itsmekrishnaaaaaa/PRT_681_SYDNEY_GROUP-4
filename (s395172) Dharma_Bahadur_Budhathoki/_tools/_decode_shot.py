import json
from pathlib import Path
import base64

src = Path(r"C:\Users\budha\.cursor\browser-logs\cdp-response-Page.captureScreenshot-2026-09-17T21-23-29-462Z.json")
dst = Path(r"C:\projects\clz\oraEarn\(s395172) Dharma_Bahadur_Budhathoki\figures\figure-shifts-ui.png")
raw = json.loads(src.read_text(encoding="utf-8"))
data = raw.get("data") or raw.get("result", {}).get("data")
if isinstance(raw, dict) and "data" not in raw:
    # maybe the whole thing is {result:{data:}}
    pass
# walk
def find_data(obj):
    if isinstance(obj, dict):
        if "data" in obj and isinstance(obj["data"], str) and len(obj["data"]) > 1000:
            return obj["data"]
        for v in obj.values():
            found = find_data(v)
            if found:
                return found
    return None

b64 = find_data(raw)
print("keys", list(raw)[:10], "b64", None if not b64 else len(b64))
dst.parent.mkdir(exist_ok=True)
dst.write_bytes(base64.b64decode(b64))
print("WROTE", dst, dst.stat().st_size)
