import { delay, http, HttpResponse } from 'msw';

const ENDPOINT_LATENCY_MS = {
    health: { base: 120, jitter: 80 },
    homepage: { base: 450, jitter: 300 },
    productDetails: { base: 800, jitter: 500 },
    products : { base: 500, jitter: 200 },
} as const;

async function simulateLatency(
    endpoint: keyof typeof ENDPOINT_LATENCY_MS
): Promise<void> {
    const { base, jitter } = ENDPOINT_LATENCY_MS[endpoint];
    const randomized = base + Math.floor(Math.random() * (jitter + 1));
    await delay(randomized);
}

export const handlers = [

    // Mock for login
    // http.post(/\/api\/v1\/login\/$/i, async () => {
    //     return HttpResponse.json({ 
    //             AccessToken: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTkxNzIzMi1iNGZiLTRhNDgtYjM0MS00ODhlMzhiNjU0MGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1OTE3MjMyLWI0ZmItNGE0OC1iMzQxLTQ4OGUzOGI2NTQwZCIsIm5hbWUiOiJhZG1pbiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJhZG1pbiIsImp0aSI6Ijk4OWZkOTcxLWFhMWEtNGE3Yy05ZDVlLTkzZGQ3ZTM3MGYwMCIsImlhdCI6MTc4NzU5NDk3MiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiYWRtaW4iLCJzY29wZSI6ImZsZWV0OnJlYWQiLCJuYmYiOjE3ODc1OTQ5NzIsImV4cCI6MTc4NzU5NTg3MiwiaXNzIjoiRmxlZXRQdWxzZSIsImF1ZCI6IkZsZWV0UHVsc2VBdWRpZW5jZSJ9.gI89LycuixgD94phjN5jKjmiudfuzh0OYn890kTAiK0',
    //             TokenType: 'Bearer',
    //             ExpiresIn: 900,
    //             Username: 'admin',
    //     }, 
    //     { status: 200 });
    // }),

    // http.get(/\/health\/live$/i, async () => {
    //     await simulateLatency('health');
    //     return HttpResponse.text('Healthy');
    // })   

];