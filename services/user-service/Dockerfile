FROM golang:1.22-alpine AS build
WORKDIR /src
COPY go.mod .
COPY main.go .
RUN CGO_ENABLED=0 GOOS=linux go build -o /out/user-service .

FROM alpine:3.24
RUN apk add --no-cache wget \
    && adduser -D -u 10001 appuser
USER appuser
COPY --from=build /out/user-service /user-service
EXPOSE 8080
ENTRYPOINT ["/user-service"]
