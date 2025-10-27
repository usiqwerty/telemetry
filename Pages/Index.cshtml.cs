using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace telemetry.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly Metrics _metrics;

    public IndexModel(ILogger<IndexModel> logger, IMeterFactory meterFactory)
    {
        _logger = logger;
        _metrics = new Metrics(meterFactory);
    }

    public void OnGet()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < new Random().Next(0, 100); i++)
        {
            Console.Write("1");
        }
        sw.Stop();
        _metrics.RequestToIndex(sw.Elapsed);
    }
}
