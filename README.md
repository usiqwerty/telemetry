# telemetry

Блок по телеметрии: OpenTelemetry, Prometheus, логирование

# 0. Prerequisites

Для блока понадобятся: Prometheus, Grafana. Их можно скачать в виде Docker-образов с DockerHub.

# 1. OpenTelemetry и Prometheus

OpenTelemetry (Otel) - OpenSource'ный набор библиотек, API и инструментов для сбора телеметрии. Телеметрия = логи, метрики, трассировки (сквозные идентификаторы запросов).
Prometheus - OpenSource система сбора метрик со своим протоколом и форматом метрик.
Де-факто Otel и Prometheus сейчас - стандарт в области телеметрии. Почему не какой-нибудь Graphite? Prometheus современнее, имеет больше возможностей и по умолчанию встроен
в современные фреймворки для разработки бэкенда.

В ASP.NET есть реализации Otel, а ещё он подготовлен для сбора метрик в формате, который понимает Prometheus.

### 1.1 Метрики

В проект нужно установить следующие пакеты:

```
dotnet add telemetry.csproj package OpenTelemetry
dotnet add telemetry.csproj package OpenTelemetry.Instrumentation.AspNetCore
dotnet add telemetry.csproj package OpenTelemetry.Extensions.Hosting
dotnet add telemetry.csproj package OpenTelemetry.Instrumentation.Http
dotnet add telemetry.csproj package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add telemetry.csproj package OpenTelemetry.Exporter.Prometheus.AspNetCore
```

После этого можно запустить приложение и открыть Swagger. Выполни любой запрос.

После этого открой в браузере http://localhost:5001/metrics.

Изучи формат, посмотри на то, как выглядят пути в метриках. Это пригодится при настройке мониторинга.

### 1.2 Grafana

Самое популярное решение для просмотра метрик - Grafana. Она позволяет использовать разные источники метрник, будь то Graphite, ClickHouse (это не система мониторинга, а СУБД,
но в ней часто хранятся разные метрики), Prometheus. Сегодня мы её настроим.

### 1.3 Docker Compose

Для скрейпинга (вытаскивания) метрик и их просмотр нам понадобится запускать Prometheus и Grafana одновременно. Удобнее всего это делать с помощью механизма Docker Compose.
Он позволяет поднимать несколько сервисов сразу, достаточно описать их в файле `compose.yaml` (или `compose.yml`).

Если у вас установлен Docker Desktop, то docker compose уже есть.
