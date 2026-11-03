
import { delay, http, HttpResponse } from 'msw';
import { mockAlerts as alerts } from './data';

import type { PagedResponseOfAlertResponse } from '@/generated/api/types.gen';

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
            if (fromDate && new Date(alert.raisedAt??0) < new Date(fromDate)) {
                matches = false;
            }
            if (toDate && new Date(alert.raisedAt??0) > new Date(toDate)) {
                matches = false;
            }
            return matches;
        });

        const sliced = filteredAlerts.slice((pagenumber - 1) * pagesize, pagenumber * pagesize);
        const response:PagedResponseOfAlertResponse = {
            items: sliced,
            totalCount: filteredAlerts.length,
            totalPages: Math.ceil(filteredAlerts.length / pagesize),
            hasNextPage: pagenumber * pagesize < filteredAlerts.length,
            hasPreviousPage: pagenumber > 1,
            pageNumber: pagenumber,
            pageSize: pagesize,
        } ;

        return HttpResponse.json(response, { status: 200 });
    })

];