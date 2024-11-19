# telemetry

Блок по телеметрии: OpenTelemetry, Prometheus, метрики.

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
dotnet add telemetry.csproj package OpenTelemetry.Exporter.Console
dotnet add telemetry.csproj package OpenTelemetry.Exporter.Prometheus.AspNetCore --prerelease
```

Добавь в Program.cs следующий код:

```csharp
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

...
    
app.UseOpenTelemetryPrometheusScrapingEndpoint();
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

```yaml
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

- описали сервисы prometheus и grafana;
- для каждого их них указали образ, из которого они поднимаются, и маппинг портов;
- подключили endpoint для скрейпинга метрик.

Чуть позже мы ещё донастроим prometheus, но сейчас подними приложения с помощью командый
`docker compose up`. После того, как всё скачается и поднимется, перейди в браузере по адресам
`http://localhost:9090` (Prometheus) и `http://localhost:3000` (Grafana). Всё должно открываться.

### 1.4 Настройки скрейпинга Prometheus

Для того, чтобы Prometheus знал, откуда ему брать метрики, нужно ему указать приложения в конфиге.
Создай рядом с `compose.yaml` файл `prometheus.yml` с содержимым

```yaml
scrape_configs:
 - job_name: "prometheus"
  # Override the global default and scrape targets from this job every 5 seconds.
   scrape_interval: 5s
   static_configs:
      - targets: ["host.docker.internal:5149"]
```

Что здесь происходит:
- `job_name` - метка сервиса (tag), с которой метрики будут приходить в Prometheus. Можно задать любой, но лучше сделать её
равной имени сервиса.
- `scrape_interval` - как часто Prometheus будет ходить в эндпойнт `metrics` нашего приложения.
- `static_configs` - описание эндпойнтов сервиса.
Поскольку мы ходим в `localhost` внутри или снаружи сети Docker'а, то localhost'ы везде будет разными,
поэтому нужно указать `host.docker.internal` - единый алиас для хоста.

Вообще говоря, можно указать Prometheus'у адреса других Prometheus'ов, это может быть актуально для больших наборов сервисов,
например, внутри Kubernetes. 

Теперь добавим в `compose.yaml` конфиг Prometheus:
```yaml
services:
  prometheus:
    image: "prom/prometheus"
    ports:
      - "9090:9090"
    volumes:
      - "./prometheus.yml:/etc/prometheus/prometheus.yml"

  grafana:
    image: "grafana/grafana-oss"
    ports:
      - "3000:3000"
```
Что мы сделали: мы примонтировали файл из файловой системы хоста в файловую систему образа. Теперь этот файл доступен внутри контейнера.
Почему выбрали такой путь в ФС образа? Дело в том, что по умолчанию конфиг Prometheus лежит именно по такому пути.

### 1.5 Запросы в Prometheus

Открой `http://localhost:9090`, перед тобой откроется интерфейс Prometheus.
Подними `WeatherApp`, потыкай в разные ссылки и попробуй запросить в интерфейсе Prometheus какую-нибудь метрику, например,
`kestrel_connection_duration_seconds_bucket`. Посмотри разные режимы отображения.

### 1.6 Графики в Grafana

Открой `http://localhost:3000`, перед тобой откроется Grafana.

Чтобы получить данные из Prometheus, надо его добавить как Data Source. Для этого в левой панели выбери пункт
`New connections` -> `Add connection`, в списке выбери Prometheus и укажи адрес `http://host.docker.internal:9090`,
после чего нажим на сохранение.

Теперь попробуй построить по имеющимся метрикам графикам. Для этого зайди в раздел `Dashboards`, создай новый
и нажми на `Add visualization`.

Возьми для примера ту же метрику, что для экспериментов с Prometheus
(она, кстати, показывает распределение длительности запросов) и попробуй её сгруппировать для сервиса. Подсказка:
воспользуйся функциями из раздела `Aggregate`.

### 1.7 Кастомные метрики

Попробуем добавить свою метрику. Для этого создай класс `Metrics` с примерным содержимым:
```csharp
using System.Diagnostics.Metrics;

namespace telemetry;

public class Metrics
{
    private readonly Counter<int> indexRequestsCount;
    private readonly Histogram<double> indexRequestsTime;

    public Metrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(nameof(Metrics));
        indexRequestsCount = meter.CreateCounter<int>("requests.index.count", "pcs", "Количество запросов");
        indexRequestsTime = meter.CreateHistogram<double>("requests.index.time", "ms", "Время запроса к index");
    }

    public void RequestToIndex(TimeSpan elapsed)
    {
        indexRequestsCount.Add(1);
        indexRequestsTime.Record(elapsed.TotalMilliseconds);
    }
}
```
Посмотри, что здесь происходит, как именно создаются метрики.
Counter - счётчик;
Histogram - гистограмма (распределение).
Для создания лучше использовать `IMeterFactory` вместо `new Meter()`, чтобы метрики попадали в общий endpoint.
Кстати, для этого добавить к настройкам метрик в Program (там, где `WithMetrics`, после `AddConsoleExporter`) опцию
```csharp
.AddMeter(nameof(Metrics))
```
Название должно совпадать с названием meter в `Metrics`, поэтому удобно сделать в них константу с названием meter.

Теперь добавь в `Index.cshtml.cs` получение `Metrics` через конструктор, а в `OnGet` код для примера:
```csharp
var sw = Stopwatch.StartNew();
for (var i = 0; i < new Random().Next(0, 100); i++)
{
    Console.Write("1");
}
sw.Stop();
_metrics.RequestToIndex(sw.Elapsed);
```
Запусти приложение и посмотри в endpoint'е `/metrics` свои метрики.