using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"], new RecommendedGroupSize(1, 10));
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"], new RecommendedGroupSize(1, 10));
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"], new RecommendedGroupSize(1, 10));
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour PublishedVineyardsByBicycleEasy;
    public static readonly Tour PublishedWineAndHeritageWalk;
    public static readonly Tour ArchivedOldTownWalk;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new RecommendedGroupSize(1, 10));
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new RecommendedGroupSize(1, 10));
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.Publish(new TourPricing(TourPricingType.Free, null));

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new RecommendedGroupSize(1, 10));
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.Publish(new TourPricing(TourPricingType.Free, null));

        PublishedVineyardsByBicycleEasy = new(WellKnownUsers.SecondExplorer, "Laka vožnja kroz vinograde", "Laka vožnja biciklom vodi ravnim putem kroz vinograde podno Fruške gore, uz kratko zadržavanje kod vidikovca i povratak istim putem do polazne tačke.", TourDifficulty.Easy, ["vino", "priroda"], new RecommendedGroupSize(1, 10));
        PublishedVineyardsByBicycleEasy.AddTransportTime(TransportMode.Bicycle, 40);
        PublishedVineyardsByBicycleEasy.Publish(new TourPricing(TourPricingType.Free, null));

        PublishedWineAndHeritageWalk = new(WellKnownUsers.SecondExplorer, "Vino i nasleđe Karlovaca", "Šetnja vodi od Saborne crkve i Patrijaršijskog dvora do Stražilova, uz obilazak starog podruma i priču o istoriji vinarstva u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "istorija"], new RecommendedGroupSize(1, 10));
        PublishedWineAndHeritageWalk.AddTransportTime(TransportMode.Walking, 120);
        PublishedWineAndHeritageWalk.Publish(new TourPricing(TourPricingType.Free, null));

        ArchivedOldTownWalk = new(WellKnownUsers.Explorer, "Stari grad zimi", "Zimska šetnja vodi od Trga slobode kroz Dunavsku i Zmaj Jovinu ulicu do Katoličke porte, uz priču o starim zanatskim radnjama i gradskim kafanama.", TourDifficulty.Easy, ["grad", "istorija"], new RecommendedGroupSize(1, 10));
        ArchivedOldTownWalk.AddTransportTime(TransportMode.Walking, 75);
        ArchivedOldTownWalk.Publish(new TourPricing(TourPricingType.Free, null));
        ArchivedOldTownWalk.Archive();
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, PublishedVineyardsByBicycleEasy, PublishedWineAndHeritageWalk, ArchivedOldTownWalk];
}
