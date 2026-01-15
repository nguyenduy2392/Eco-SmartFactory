# Stage 1: Build ứng dụng Angular
FROM node:20-alpine AS build-step
WORKDIR /app

# Thêm các gói hỗ trợ build
RUN apk add --no-cache python3 make g++
# Chỉ copy file quản lý thư viện trước để tối ưu cache của Docker
COPY package.json package-lock.json ./
RUN npm ci --legacy-peer-deps

# Copy toàn bộ mã nguồn và build
COPY . .
RUN npm run build -- --configuration=production

# Stage 2: Chạy ứng dụng bằng Nginx
FROM nginx:alpine

# Xóa trang Welcome mặc định của Nginx
RUN rm -rf /usr/share/nginx/html/*

# QUAN TRỌNG: Kiểm tra thư mục output trong angular.json (thường là dist/tên-project)
# Thay 'inspection-label' bằng tên chính xác trong thư mục dist của bạn
COPY --from=build-step /app/dist/inspection-label /usr/share/nginx/html

# Cấu hình để Angular Router hoạt động (tránh lỗi 404 khi F5 trang)
RUN echo 'server { \
    listen 80; \
    location / { \
        root /usr/share/nginx/html; \
        index index.html index.htm; \
        try_files $uri $uri/ /index.html; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]