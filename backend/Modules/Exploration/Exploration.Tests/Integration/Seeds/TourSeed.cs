using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"], new RecommendedGroupSize(1, 10));
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"], new RecommendedGroupSize(1, 10));
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"], new RecommendedGroupSize(1, 10));
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishableQuayWithExistingProduct;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour PublishedVineyardsByBicycleEasy;
    public static readonly Tour PublishedWineAndHeritageWalk;
    public static readonly Tour PublishedFortressWithHiddenFirstKeyPoint;
    public static readonly Tour ArchivedOldTownWalk;
    public static readonly Tour DraftWithKeyPoints;

    static TourSeed()
    {
        DraftWithKeyPoints = new(WellKnownUsers.Explorer, "Od stanice do tvrđave", "Kratak opis ture sa ključnim tačkama.", TourDifficulty.Easy, ["grad"], new RecommendedGroupSize(1, 10));
        DraftWithKeyPoints.AddKeyPoint(new KeyPoint("Železnička stanica", "Polazna tačka ture.", "/images/seed-stanica.jpg", null, new Location(45.2655, 19.8293), true), 0);
        DraftWithKeyPoints.AddKeyPoint(new KeyPoint("Trg slobode", "Srce grada.", "/images/seed-trg.jpg", "Ispod spomenika je stara kapsula.", new Location(45.2551, 19.8452), false), 1.9);

        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new RecommendedGroupSize(1, 10));
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);
        PublishableRiverside.AddKeyPoint(new KeyPoint("Ribarsko ostrvo", "Početak staze uz Dunav.", "/images/seed-publishableriverside-1.jpg", null, new Location(45.238, 19.877), true), 0);
        PublishableRiverside.AddKeyPoint(new KeyPoint("Most slobode", "Kraj staze kod mosta.", "/images/seed-publishableriverside-2.jpg", null, new Location(45.241, 19.859), true), 1.5);

        PublishableQuayWithExistingProduct = new(WellKnownUsers.Explorer, "Šetnja kejom", "Šetnja počinje kod Varadinskog mosta, prati Sunčani kej pored Dunavskog parka i završava se kod Kamenjara, uz pogled na Petrovaradinsku tvrđavu.", TourDifficulty.Easy, ["priroda", "grad"], new RecommendedGroupSize(1, 10));
        PublishableQuayWithExistingProduct.AddTransportTime(TransportMode.Walking, 50);
        PublishableQuayWithExistingProduct.AddKeyPoint(new KeyPoint("Varadinski most", "Početak šetnje kejom.", "/images/seed-publishablequay-1.jpg", null, new Location(45.251, 19.862), true), 0);
        PublishableQuayWithExistingProduct.AddKeyPoint(new KeyPoint("Kamenjar", "Kraj šetnje kejom.", "/images/seed-publishablequay-2.jpg", null, new Location(45.244, 19.873), true), 1.2);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new RecommendedGroupSize(1, 10));
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.AddKeyPoint(new KeyPoint("Porodični podrum", "Prvi podrum na turi.", "/images/seed-publishedvineyards-1.jpg", "Podrum je star dva veka.", new Location(45.201, 19.935), true), 0);
        PublishedVineyards.AddKeyPoint(new KeyPoint("Degustacija bermeta", "Završetak u Sremskim Karlovcima.", "/images/seed-publishedvineyards-2.jpg", "Recept bermeta čuva jedna porodica.", new Location(45.203, 19.933), false), 0.3);
        PublishedVineyards.Publish(new TourPricing(TourPricingType.Paid, 30));

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new RecommendedGroupSize(1, 10));
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.AddKeyPoint(new KeyPoint("Krušedol", "Prvi manastir na turi.", "/images/seed-publishedmonasteries-1.jpg", null, new Location(45.121, 19.944), true), 0);
        PublishedMonasteries.AddKeyPoint(new KeyPoint("Novo Hopovo", "Poslednji manastir na turi.", "/images/seed-publishedmonasteries-2.jpg", null, new Location(45.15, 19.864), true), 7.2);
        PublishedMonasteries.Publish(new TourPricing(TourPricingType.Free, null));

        PublishedVineyardsByBicycleEasy = new(WellKnownUsers.SecondExplorer, "Laka vožnja kroz vinograde", "Laka vožnja biciklom vodi ravnim putem kroz vinograde podno Fruške gore, uz kratko zadržavanje kod vidikovca i povratak istim putem do polazne tačke.", TourDifficulty.Easy, ["vino", "priroda"], new RecommendedGroupSize(1, 10));
        PublishedVineyardsByBicycleEasy.AddTransportTime(TransportMode.Bicycle, 40);
        PublishedVineyardsByBicycleEasy.AddKeyPoint(new KeyPoint("Polazna tačka", "Početak vožnje kroz vinograde.", "/images/seed-publishedvineyardsbybicycleeasy-1.jpg", "Prva loza je zasađena pre sto godina.", new Location(45.199, 19.93), true), 0);
        PublishedVineyardsByBicycleEasy.AddKeyPoint(new KeyPoint("Stari podrum", "Kratak predah kod starog podruma.", "/images/seed-publishedvineyardsbybicycleeasy-2.jpg", "Ključ podruma visi iza vrata.", new Location(45.195, 19.925), true), 0.7);
        PublishedVineyardsByBicycleEasy.AddKeyPoint(new KeyPoint("Vidikovac", "Kratko zadržavanje kod vidikovca.", "/images/seed-publishedvineyardsbybicycleeasy-3.jpg", "Sa vidikovca se po vedrom danu vidi Beograd.", new Location(45.19, 19.92), true), 1.3);
        PublishedVineyardsByBicycleEasy.Publish(new TourPricing(TourPricingType.Paid, 60));

        PublishedWineAndHeritageWalk = new(WellKnownUsers.SecondExplorer, "Vino i nasleđe Karlovaca", "Šetnja vodi od Saborne crkve i Patrijaršijskog dvora do Stražilova, uz obilazak starog podruma i priču o istoriji vinarstva u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "istorija"], new RecommendedGroupSize(1, 10));
        PublishedWineAndHeritageWalk.AddTransportTime(TransportMode.Walking, 120);
        PublishedWineAndHeritageWalk.AddKeyPoint(new KeyPoint("Saborna crkva", "Početak šetnje.", "/images/seed-publishedwineandheritagewalk-1.jpg", null, new Location(45.2025, 19.933), true), 0);
        PublishedWineAndHeritageWalk.AddKeyPoint(new KeyPoint("Stražilovo", "Kraj šetnje.", "/images/seed-publishedwineandheritagewalk-2.jpg", null, new Location(45.189, 19.915), true), 2.1);
        PublishedWineAndHeritageWalk.Publish(new TourPricing(TourPricingType.Free, null));

        PublishedFortressWithHiddenFirstKeyPoint = new(WellKnownUsers.SecondExplorer, "Tajne Petrovaradina", "Tura počinje u podzemnim galerijama Petrovaradinske tvrđave, čija se tačna lokacija otkriva tek posle kupovine, a završava se šetnjom kroz Dunavski park.", TourDifficulty.Moderate, ["istorija"], new RecommendedGroupSize(1, 10));
        PublishedFortressWithHiddenFirstKeyPoint.AddTransportTime(TransportMode.Walking, 60);
        PublishedFortressWithHiddenFirstKeyPoint.AddKeyPoint(new KeyPoint("Podzemne galerije", "Skriveni početak ture.", "/images/seed-publishedfortresswithhiddenfirstkeypoint-1.jpg", "Ulaz je iza treće kapije.", new Location(45.2523, 19.8616), false), 0);
        PublishedFortressWithHiddenFirstKeyPoint.AddKeyPoint(new KeyPoint("Dunavski park", "Kraj ture u parku.", "/images/seed-publishedfortresswithhiddenfirstkeypoint-2.jpg", null, new Location(45.2556, 19.8494), true), 1.0);
        PublishedFortressWithHiddenFirstKeyPoint.Publish(new TourPricing(TourPricingType.Paid, 40));

        ArchivedOldTownWalk = new(WellKnownUsers.Explorer, "Stari grad zimi", "Zimska šetnja vodi od Trga slobode kroz Dunavsku i Zmaj Jovinu ulicu do Katoličke porte, uz priču o starim zanatskim radnjama i gradskim kafanama.", TourDifficulty.Easy, ["grad", "istorija"], new RecommendedGroupSize(1, 10));
        ArchivedOldTownWalk.AddTransportTime(TransportMode.Walking, 75);
        ArchivedOldTownWalk.AddKeyPoint(new KeyPoint("Trg slobode", "Početak zimske šetnje.", "/images/seed-archivedoldtownwalk-1.jpg", null, new Location(45.2551, 19.8452), true), 0);
        ArchivedOldTownWalk.AddKeyPoint(new KeyPoint("Katolička porta", "Kraj zimske šetnje.", "/images/seed-archivedoldtownwalk-2.jpg", null, new Location(45.256, 19.844), true), 0.2);
        ArchivedOldTownWalk.Publish(new TourPricing(TourPricingType.Paid, 20));
        ArchivedOldTownWalk.Archive();
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishableQuayWithExistingProduct, PublishedVineyards, PublishedMonasteries, PublishedVineyardsByBicycleEasy, PublishedWineAndHeritageWalk, PublishedFortressWithHiddenFirstKeyPoint, ArchivedOldTownWalk, DraftWithKeyPoints];
}
