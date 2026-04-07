# API Smoke Test

This is a lightweight regression check for the portfolio chat endpoint.

It uses the Tonal LinkedIn-style job description fixture and verifies:

- `/api/chat` responds successfully
- `answer` is not empty
- `sources` is not empty
- obvious LinkedIn noise like `show more options` or `resume match` does not appear in the answer
- the answer still contains at least one high-signal requirement term

## Run

Start the backend first:

```bash
dotnet run --project backend/PersonalWeb.Api
```

Then run the smoke test:

```bash
dotnet run --project backend/PersonalWeb.Api.SmokeTests
```

If your backend is running on a different URL:

```bash
PERSONALWEB_API_BASE_URL=http://localhost:9999 dotnet run --project backend/PersonalWeb.Api.SmokeTests
```
