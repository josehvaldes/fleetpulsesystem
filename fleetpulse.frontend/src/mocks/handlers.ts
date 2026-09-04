import { delay, http, HttpResponse } from 'msw';
import { mockAlerts as alerts } from './data';
import type { AlertWire } from '@/api/alerts/types';
import type { PageResponse } from '@/api/genericTypes';

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

    //Mock for login
    http.post(/\/api\/v1\/login$/i, async () => {
        console.log('Mocking login endpoint');
        return HttpResponse.json({ 
                accessToken: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTkxNzIzMi1iNGZiLTRhNDgtYjM0MS00ODhlMzhiNjU0MGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1OTE3MjMyLWI0ZmItNGE0OC1iMzQxLTQ4OGUzOGI2NTQwZCIsIm5hbWUiOiJhZG1pbiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJhZG1pbiIsImp0aSI6Ijk4OWZkOTcxLWFhMWEtNGE3Yy05ZDVlLTkzZGQ3ZTM3MGYwMCIsImlhdCI6MTc4NzU5NDk3MiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiYWRtaW4iLCJzY29wZSI6ImZsZWV0OnJlYWQiLCJuYmYiOjE3ODc1OTQ5NzIsImV4cCI6MTc4NzU5NTg3MiwiaXNzIjoiRmxlZXRQdWxzZSIsImF1ZCI6IkZsZWV0UHVsc2VBdWRpZW5jZSJ9.gI89LycuixgD94phjN5jKjmiudfuzh0OYn890kTAiK0',
                tokenType: 'Bearer',
                expiresIn: 900,
                username: 'admin',
        }, 
        { status: 200 });
    }),
    http.get(/\/api\/v1\/alerts$/i, async ({request}) => {
        console.log('Mocking alerts endpoint');
        await simulateLatency('products');
        const queryParams = new URLSearchParams(request.url.split('?')[1]);
        const pagenumber = parseInt(queryParams.get('pagenumber') || '1', 10);
        const pagesize = parseInt(queryParams.get('pagesize') || '10', 10);
        const riskLevel = queryParams.get('riskLevel');
        const status = queryParams.get('status');
        const fromDate = queryParams.get('fromDate');
        const toDate = queryParams.get('toDate');
        const filteredAlerts = alerts.filter( (alert) => {
            let matches = true  ;
            if (riskLevel && alert.riskLevel !== riskLevel) {
                matches = false;
            }
            if (status && alert.status !== status) {
                matches = false;
            }
            if (fromDate && new Date(alert.raisedAt) < new Date(fromDate)) {
                matches = false;
            }
            if (toDate && new Date(alert.raisedAt) > new Date(toDate)) {
                matches = false;
            }
            return matches;
        });

        const sliced = filteredAlerts.slice((pagenumber - 1) * pagesize, pagenumber * pagesize);
        const response:PageResponse<AlertWire> = {
            data: sliced,
            totalCount: filteredAlerts.length,
            totalPages: Math.ceil(filteredAlerts.length / pagesize),
            hasNextPage: pagenumber * pagesize < filteredAlerts.length,
            hasPreviousPage: pagenumber > 1,
            pageNumber: pagenumber,
            pageSize: pagesize,
        } ;

        return HttpResponse.json(response, { status: 200 });
    }),

    http.get(/\/health\/live$/i, async () => {
        await simulateLatency('health');
        return HttpResponse.text('Healthy');
    })   

];