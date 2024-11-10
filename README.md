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
dotnet add telemetry.csproj package OpenTelemetry.Exporter.Prometheus.AspNetCore --prerelease
```

Добавь в Program.cs следующий код:

```cs
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService("TelemetryExample"))
        .AddConsoleExporter();
});
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("TelemetryExample"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddPrometheusExporter()
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());
```
Что здесь сделали:
- добавили создали новый билдер для телеметрии;
- добавили трассировки и их запись в консоль;
- добавили метрики с экспортом в Prometheus и их запись в консоль.

После этого можно запустить приложение и открыть Swagger или просто открыть страницу приложения и попереходить по ссылкам.
Выполни любой запрос.

После этого открой в браузере http://localhost:5149/metrics.

Изучи формат, посмотри на то, как выглядят пути в метриках. Это пригодится при настройке мониторинга.

### 1.2 Grafana

Самое популярное решение для просмотра метрик - Grafana. Она позволяет использовать разные источники метрник, будь то Graphite, ClickHouse (это не система мониторинга, а СУБД,
но в ней часто хранятся разные метрики), Prometheus. Сегодня мы её настроим.

### 1.3 Docker Compose

Для скрейпинга (вытаскивания) метрик и их просмотр нам понадобится запускать Prometheus и Grafana одновременно. Удобнее всего это делать с помощью механизма Docker Compose.
Он позволяет поднимать несколько сервисов сразу, достаточно описать их в файле `compose.yaml` (или `compose.yml`).

Если у вас установлен Docker Desktop, то docker compose уже есть.

Создай в любом удобном месте файл `compose.yaml` и напиши в нём следующее содержимое:
```
services:
  prometheus:
    image: "prom/prometheus"
    ports:
      - "9090:9090"

  grafana:
    image: "grafana/grafana-oss"
    ports:
      - "3000:3000"
```

Что здесь происходит:
- описали сервисы prometheus и grafana
- для каждого их них указали образ, из которого они поднимаются, и маппинг портов.

Чуть позже мы ещё донастроим prometheus, но сейчас подними приложения с помощью командый
`docker compose up`. После того, как всё скачается и поднимется, перейди в браузере по адресам
`http://localhost:9090` (Prometheus) и `http://localhost:3000` (Grafana). Всё должно открываться.
