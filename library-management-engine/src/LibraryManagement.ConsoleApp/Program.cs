using LibraryManagement.ConsoleApp;
using LibraryManagement.ConsoleApp.Time;
using LibraryManagement.Core.Services;

// Composition root: the only place concrete pieces are chosen and wired together.
var clock = new AdjustableClock(TimeProvider.System);
var library = LibraryService.CreateInMemory(clock);

if (!args.Contains("--empty", StringComparer.OrdinalIgnoreCase))
{
    SampleData.Load(library);
    Console.WriteLine("Loaded sample data: 7 books and 3 members (start with --empty to skip).");
}

return new LibraryApp(library, clock, Console.In, Console.Out).Run();
