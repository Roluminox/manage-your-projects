# ===========================================
# Stage 1: Build
# ===========================================
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files
COPY client/package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY client/ ./

# Build production bundle
RUN npm run build -- --configuration=production

# ===========================================
# Stage 2: Runtime
# ===========================================
FROM nginx:alpine AS runtime

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Copy custom nginx config
COPY client/nginx.conf /etc/nginx/conf.d/default.conf

# Copy built assets from build stage
COPY --from=build /app/dist/myp-client/browser /usr/share/nginx/html

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost/health || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
