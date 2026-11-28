using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationModuleInitializer : IHostedService
{
    private static readonly Guid FirstExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public ExplorationModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ExplorationDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment() && !await dbContext.Tours.AnyAsync())
        {
            dbContext.Tours.AddRange(CreateDemoTours());
            await dbContext.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<Tour> CreateDemoTours()
    {
        var fortress = new Tour(FirstExplorer, "Šetnja Petrovaradinskom tvrđavom",
            "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav i stari deo Novog Sada.",
            TourDifficulty.Easy, ["istorija", "priroda"], new RecommendedGroupSize(1, 10));
        fortress.AddTransportTime(TransportMode.Walking, 120);
        fortress.AddKeyPoint(new KeyPoint("Gornji plato", "Početak šetnje kod Sahat kule.",
            "/images/demo-gornji-plato.jpg", "Sahat kula ima obrnute kazaljke.", new Location(45.2525, 19.8625), true), 0);
        fortress.AddKeyPoint(new KeyPoint("Podzemne galerije", "Ulaz u vojne galerije tvrđave.",
            "/images/demo-galerije.jpg", "Galerije imaju četiri nivoa.", new Location(45.2508, 19.8641), false), 0.25);
        fortress.Publish(new TourPricing(TourPricingType.Free, null));

        var fruskaGora = new Tour(FirstExplorer, "Biciklom preko Fruške gore",
            "Ruta vodi od Sremske Kamenice preko Popovice do Iriškog venca, pored manastira Beočin i vidikovaca sa kojih se vidi ceo Srem, uz nekoliko dužih uspona.",
            TourDifficulty.Hard, ["biciklizam", "planina"], new RecommendedGroupSize(1, 10));
        fruskaGora.AddTransportTime(TransportMode.Bicycle, 240);
        fruskaGora.AddKeyPoint(new KeyPoint("Popovica", "Prvi uspon na ruti.",
            "/images/demo-popovica.jpg", null, new Location(45.1990, 19.8460), true), 0);
        fruskaGora.AddKeyPoint(new KeyPoint("Iriški venac", "Najviša tačka rute.",
            "/images/demo-iriski-venac.jpg", "Sa tornja se vidi ceo Srem.", new Location(45.1530, 19.8640), false), 5.1);
        fruskaGora.Publish(new TourPricing(TourPricingType.Free, null));

        var cityCenter = new Tour(FirstExplorer, "Obilazak centra grada",
            "Kratak obilazak Trga slobode, Dunavske ulice i Dunavskog parka.",
            TourDifficulty.Moderate, ["grad"], new RecommendedGroupSize(1, 10));

        return [fortress, fruskaGora, cityCenter];
    }
}
