# telemetry

Блок по телеметрии: OpenTelemetry, Prometheus, логирование

# 0. Prerequisites

Для блока понадобятся: Prometheus, Grafana. Их можно скачать в виде Docker-образов с DockerHub.

# 1. OpenTelemetry и Prometheus

OpenTelemetry (Otel) - OpenSource'ный набор библиотек, API и инструментов для сбора телеметрии. Телеметрия = логи, метрики, трассировки (сквозные идентификаторы запросов).
Prometheus - OpenSource система сбора метрик со своим протоколом и форматом метрик.
Де-факто Otel и Prometheus сейчас - стандарт в области телеметрии. Почему не какой-нибудь Graphite? Prometheus современнее, имеет больше возможностей и по умолчанию встроен в современные фреймворки для разработки бэкенда.

В ASP.NET есть реализации Otel, а ещё он подготовлен для сбора метрик в формате, который понимает Prometheus.

### 1.1 Метрики

В проект нужно установить следующие пакеты:

```
dotnet add OpenTelemetry
dotnet add OpenTelemetry.Instrumentation.AspNetCore
dotnet add OpenTelemetry.Extensions.Hosting
dotnet add OpenTelemetry.Instrumentation.Http
dotnet add OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add OpenTelemetry.Exporter.Prometheus.AspNetCore
```

После этого можно запустить приложение и открыть Swagger. Выполни любой запрос.

После этого открой в браузере http://localhost:5001/metrics.
