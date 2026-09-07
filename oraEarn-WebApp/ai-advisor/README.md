# OraEarn Aura (Python)

This folder is **Aura** — OraEarn's standalone Python microservice for in-app AI advice.

## How it works (architecture)

```
React UI  →  .NET API (auth + loads user data)  →  Python FastAPI (AI logic)  →  reply
```

1. **Frontend** (`AuraWidget.tsx`) sends the user's question to `POST /api/savingsgoals/advisor/chat`.
2. **.NET API** (`SavingsAdvisorService.cs`) loads that user's analytics + visa compliance from **PostgreSQL** (via EF Core), builds a JSON **context** object, and forwards it to Aura (Python).
3. **Python** (`advisor.py`) applies rule-based business logic (visa limits, savings goals, notifications) and returns `{ reply, suggestions }`.
4. **Frontend** displays the answer and clickable suggestion chips.

No OpenAI key is required — this is **rule-based AI** (if/else on your real data). You can swap `advisor.py` for an LLM later without changing the UI.

## Run

From this folder:

```powershell
.\run.ps1
```

Or:

```powershell
python -m venv .venv
.\.venv\Scripts\pip install -r requirements.txt
.\.venv\Scripts\uvicorn.exe main:app --reload --port 8000
```

Start the **backend** as well — it is the only service that calls Aura. The frontend never talks to Python directly.

In production Aura is internal-only: disable `/docs` with `AURA_ENV=production`, and set `AURA_API_KEY` so the API must send `X-Aura-Key`. Do not publish port 8000 to the internet.

Test Python directly (Development, when no API key is set):

```bash
curl -X POST http://localhost:8000/api/advisor/chat ^
  -H "Content-Type: application/json" ^
  -d "{\"message\":\"Am I over my visa limit?\",\"context\":{\"visa\":{\"applies\":true,\"visa_status\":\"Student\",\"fortnight_hours_worked\":100,\"fortnight_hour_limit\":48,\"is_over_limit\":true}}}"
```

## Key files

| File | Purpose |
|------|---------|
| `main.py` | FastAPI app + HTTP routes |
| `models.py` | Pydantic schemas (request/response) |
| `advisor.py` | **All AI logic** — edit this to change behaviour |

## Add this pattern to other stacks

### Node / Express backend

```js
// After loading user analytics from your DB:
const res = await fetch('http://localhost:8000/api/advisor/chat', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ message, context }),
});
return res.json();
```

### Django

```python
import requests
def advisor_view(request):
    payload = {"message": request.data["message"], "context": build_context(request.user)}
    r = requests.post("http://localhost:8000/api/advisor/chat", json=payload, timeout=10)
    return JsonResponse(r.json())
```

### Call Python from mobile (Flutter / React Native)

Same JSON POST to your backend proxy — **never** call Python directly from the app; always go through your API so auth stays secure.

### Swap in OpenAI later

Replace the body of `chat()` in `advisor.py`:

```python
# context JSON becomes the "system" prompt; message is the user prompt
response = openai.chat.completions.create(
    model="gpt-4o-mini",
    messages=[
        {"role": "system", "content": f"User data: {ctx.model_dump_json()}"},
        {"role": "user", "content": message},
    ],
)
```

Keep the same `ChatResponse` shape so React/.NET do not need changes.

## Configuration (.NET)

`appsettings.Development.json`:

```json
"AiAdvisor": {
  "BaseUrl": "http://localhost:8000"
}
```
