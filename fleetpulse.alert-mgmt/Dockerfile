FROM node:24-alpine AS builder
WORKDIR /app

# Install dependencies first to maximize build cache reuse.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.29-alpine AS runtime
WORKDIR /usr/share/nginx/html

# Replace default nginx static files with Vite build output.
RUN rm -rf ./*
COPY --from=builder /app/dist .
COPY nginx.conf /etc/nginx/templates/default.conf.template
ENV CORS_ALLOWED_ORIGIN=http://localhost:5173

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
