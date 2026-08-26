# Architecture Decision Records 0001

**Frontend Architecture: Why a Pure SPA?**

A common question is why this project uses Vite + React instead of Next.js (which was used in the companion e-commerce project).

Stale Data Problem: Next.js Server Components render HTML on the server. By the time the HTML reaches the browser, the GPS coordinates have already changed. SSR provides zero value for live moving objects.
Connection State: WebSockets require long-lived, stateful connections. Next.js Serverless functions (Vercel) aggressively terminate idle connections and enforce strict timeouts.
Performance: Vite's Hot Module Replacement (HMR) is nearly instantaneous, which is critical when iterating on complex map animations and real-time chart state.