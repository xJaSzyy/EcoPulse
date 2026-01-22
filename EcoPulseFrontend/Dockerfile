FROM node:20-alpine
WORKDIR /app
COPY . .
RUN npm ci && npm run build && npm install -g serve
EXPOSE 3000
CMD ["serve", "-s", "dist", "-l", "3000"]