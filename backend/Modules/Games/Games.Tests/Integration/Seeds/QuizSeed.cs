using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz DraftFortressQuiz = new("Tajne tvrđave", "Odgovori na pitanja o Petrovaradinskoj tvrđavi.",
    [
        new Question("Kako se zove čuveni sat na tvrđavi?", 100, [new AnswerOption("Sahat kula", true), new AnswerOption("Toranj sveta", false)]),
        new Question("Iznad koje reke se nalazi tvrđava?", 50, [new AnswerOption("Dunav", true), new AnswerOption("Sava", false), new AnswerOption("Tisa", false)]),
        new Question("Šta se nalazi ispod tvrđave?", 80, [new AnswerOption("Podzemne galerije", true), new AnswerOption("Rudnik zlata", false)]),
        new Question("Koji festivali se održavaju na tvrđavi?", 70, [new AnswerOption("Exit", true), new AnswerOption("Guča", false), new AnswerOption("Tvrđava teatar", true)])
    ], new Location(45.2517, 19.8628), 150);

    public static readonly Quiz DraftRiversideQuiz = new("Uz Dunav", "Prepoznaj mesta duž obale.",
    [
        new Question("Kako se zove gradska plaža?", 40, [new AnswerOption("Štrand", true), new AnswerOption("Ada", false)]),
        new Question("Koji most vodi ka Fruškoj gori?", 60, [new AnswerOption("Most slobode", true), new AnswerOption("Brankov most", false)]),
        new Question("Koje ostrvo je na Dunavu kod Novog Sada?", 30, [new AnswerOption("Ribarsko ostrvo", true), new AnswerOption("Veliko ratno ostrvo", false)]),
        new Question("Šta se vidi sa keja?", 20, [new AnswerOption("Tvrđava", true), new AnswerOption("More", false)])
    ], new Location(45.2441, 19.8508), 300);

    public static readonly Quiz PublishedMonasteriesQuiz;
    public static readonly Quiz RetiredOldTownQuiz;
    public static readonly Quiz ArchivedVineyardsQuiz;

    static QuizSeed()
    {
        PublishedMonasteriesQuiz = new("Manastiri Fruške gore", "Odgovori na pitanja o manastirima.",
        [
            new Question("Koji manastir je zadužbina Brankovića?", 100, [new AnswerOption("Krušedol", true), new AnswerOption("Studenica", false)]),
            new Question("Na kojoj planini su manastiri?", 50, [new AnswerOption("Fruška gora", true), new AnswerOption("Tara", false)]),
            new Question("Koji manastiri su na Fruškoj gori?", 80, [new AnswerOption("Grgeteg", true), new AnswerOption("Novo Hopovo", true), new AnswerOption("Žiča", false)]),
            new Question("Koliko je manastira sačuvano na Fruškoj gori?", 70, [new AnswerOption("Šesnaest", true), new AnswerOption("Dva", false)])
        ], new Location(45.1453, 19.8125), 200);
        PublishedMonasteriesQuiz.Publish();

        RetiredOldTownQuiz = new("Stari grad", "Prepoznaj mesta u centru grada.",
        [
            new Question("Kako se zove glavni gradski trg?", 60, [new AnswerOption("Trg slobode", true), new AnswerOption("Trg republike", false)]),
            new Question("Koja crkva stoji na glavnom trgu?", 70, [new AnswerOption("Crkva imena Marijinog", true), new AnswerOption("Saborna crkva", false)]),
            new Question("Kako se zove pešačka ulica od trga?", 40, [new AnswerOption("Zmaj Jovina", true), new AnswerOption("Knez Mihailova", false)]),
            new Question("Gde je gradska kuća?", 30, [new AnswerOption("Na Trgu slobode", true), new AnswerOption("Na keju", false)])
        ], new Location(45.2551, 19.8452), 100);
        RetiredOldTownQuiz.Publish();
        RetiredOldTownQuiz.Retire();

        ArchivedVineyardsQuiz = new("Karlovački vinogradi", "Odgovori na pitanja o vinu i vinogradima.",
        [
            new Question("Koje vino je poznato u Sremskim Karlovcima?", 90, [new AnswerOption("Bermet", true), new AnswerOption("Prokupac", false)]),
            new Question("Na obroncima koje planine su vinogradi?", 50, [new AnswerOption("Fruška gora", true), new AnswerOption("Kopaonik", false)]),
            new Question("Kako se zove izletište iznad Karlovaca?", 40, [new AnswerOption("Stražilovo", true), new AnswerOption("Iriški venac", false)]),
            new Question("Šta se čuva u podrumu?", 20, [new AnswerOption("Vino", true), new AnswerOption("Žito", false)])
        ], new Location(45.2025, 19.9330), 250);
        ArchivedVineyardsQuiz.Publish();
        ArchivedVineyardsQuiz.Retire();
        ArchivedVineyardsQuiz.Archive();
    }

    public static Quiz[] All => [DraftFortressQuiz, DraftRiversideQuiz, PublishedMonasteriesQuiz, RetiredOldTownQuiz, ArchivedVineyardsQuiz];
}
