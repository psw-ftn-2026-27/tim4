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
    ]);

    public static readonly Quiz DraftRiversideQuiz = new("Uz Dunav", "Prepoznaj mesta duž obale.",
    [
        new Question("Kako se zove gradska plaža?", 40, [new AnswerOption("Štrand", true), new AnswerOption("Ada", false)]),
        new Question("Koji most vodi ka Fruškoj gori?", 60, [new AnswerOption("Most slobode", true), new AnswerOption("Brankov most", false)]),
        new Question("Koje ostrvo je na Dunavu kod Novog Sada?", 30, [new AnswerOption("Ribarsko ostrvo", true), new AnswerOption("Veliko ratno ostrvo", false)]),
        new Question("Šta se vidi sa keja?", 20, [new AnswerOption("Tvrđava", true), new AnswerOption("More", false)])
    ]);

    public static Quiz[] All => [DraftFortressQuiz, DraftRiversideQuiz];
}
