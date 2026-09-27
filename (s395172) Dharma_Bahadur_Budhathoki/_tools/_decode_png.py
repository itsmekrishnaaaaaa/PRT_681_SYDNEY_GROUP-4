import json, base64, sys
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
raw = json.loads(src.read_text(encoding="utf-8"))
data = raw["data"] if "data" in raw else raw["result"]["data"]
dst.parent.mkdir(parents=True, exist_ok=True)
dst.write_bytes(base64.b64decode(data))
print(dst.name, dst.stat().st_size)
