# Assignment 02 — Number Guessing Game & Grade Tracker

Two small C# console applications built for Week 2:

1. **Number Guessing Game** — guess a secret number within a limited number of tries, with higher/lower hints.
2. **Grade Tracker** — record student scores in an array and report the average, highest and lowest score plus a letter grade per student.

Both apps are menu-driven, loop until the user quits, and keep their logic in small methods (not one long `Main`). The game and grade rules live in separate class libraries so they can be unit tested without any console input.

---

## Features

### Number Guessing Game
- Secret number between **1 and 100**.
- Three difficulty levels: easy (10 tries), normal (7 tries), hard (5 tries).
- "Too high" / "too low" hints after each guess.
- Rejects non-numeric and out-of-range guesses and re-prompts.
- Reveals the answer if you run out of tries, then returns to the menu.

### Grade Tracker
- Stores students (name + score) and exposes their scores as an **array**.
- Reports **average**, **highest** and **lowest** score for the class.
- Assigns each student a **letter grade** (A–F).
- Validates score range (0–100) and rejects blank names.
- Menu repeats until you choose Quit.

---

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or newer (`dotnet --version` to check).

---

## Getting started

Clone the repository and move into this folder:

```bash
git clone https://github.com/luckybaraka/zhuwa-projects.git
cd zhuwa-projects/Assignment02-GuessingGame-GradeTracker
```

Build everything:

```bash
dotnet build
```

### Run the Number Guessing Game

```bash
dotnet run --project NumberGuessingGame
```

### Run the Grade Tracker

```bash
dotnet run --project GradeTracker
```

### Run the automated tests

```bash
dotnet test
```

---

## Architecture & design

The solution separates **pure logic** from **console I/O**. Each app's console project only reads input and prints output; all rules and calculations live in a class library.

```
Assignment02-GuessingGame-GradeTracker/
├── Assignment02.slnx          Solution file
├── Directory.Build.props      Shared, strict build settings
├── GameLogic/                 Guessing-game rules (no console code)
│   ├── GuessingGame.cs        Target, attempts, win/over state, Guess()
│   └── GuessOutcome.cs        TooLow / TooHigh / Correct
├── NumberGuessingGame/        Console UI + menu for the game
│   └── Program.cs
├── GradeLogic/                Grade rules & statistics (no console code)
│   ├── Student.cs             Name + Score, with a Grade property
│   ├── GradeCalculator.cs     Average / Highest / Lowest / LetterGrade
│   └── GradeBook.cs           Holds students, exposes scores as an array
├── GradeTracker/              Console UI + menu for the tracker
│   └── Program.cs
└── Tests/                     xUnit tests for GameLogic and GradeLogic
```

### Key decisions

- **Logic in libraries, UI in console apps.** Because `GuessingGame`, `GradeCalculator` and `GradeBook` have no `Console` dependency, they can be unit tested directly and reused. This is what makes meaningful automated testing possible.
- **`Main` split into small methods.** Each console app breaks its flow into focused methods (`RunMenu`, `PlayRound`, `ReadGuess`, `AddStudent`, `ListStudents`, `ShowSummary`, `ReadScore`) rather than one long method.
- **Deterministic randomness.** `GuessingGame.CreateRandom` takes a `Random` instance, so tests can pass a fixed seed and assert behaviour reliably.
- **Validation at the boundary.** The libraries validate their own inputs (range, empty sets, blank names) and throw clear exceptions; the console layer catches bad user input early and re-prompts.
- **Scores stored in an array.** `GradeBook.Scores()` returns an `int[]`, and all statistics operate on that array, as the assignment requires.

---

## Testing

Automated tests live in the `Tests` project (xUnit) and cover the library logic, not the console I/O:

- **GuessingGame** — correct/too-low/too-high outcomes, win and game-over state, attempt tracking, guessing after the game ends, constructor validation, and that `CreateRandom` stays within range.
- **GradeCalculator** — average (including non-integer means), highest, lowest, single-score and empty-array cases, and the valid-score range check.
- **Letter grades & GradeBook** — every A–F boundary, out-of-range failures, insertion order, name trimming, blank-name/invalid-score rejection, and that statistics reflect the added students.

Run them with:

```bash
dotnet test
```

All **43 tests** pass.

---

## Assumptions

- Scores are whole numbers from **0 to 100**.
- Each student has a **single** score; the "grade for each student" is derived from that score.
- The guessing-game range is fixed at **1–100**; difficulty only changes the number of tries.
- The letter-grade scale is the common one: **A** 90+, **B** 80+, **C** 70+, **D** 60+, **F** below 60.
- Data is held in memory for the duration of a run; there is no file or database persistence.

## Known limitations

- **No persistence.** Students and game results are lost when the app exits.
- **Single score per student.** The tracker does not keep multiple assessments per student.
- **English, console-only UI.** No localisation or graphical interface.
- **Fixed guessing range.** The 1–100 range is not user-configurable (only the number of tries is).

---

## Sample runs

### Number Guessing Game

```
=== Number Guessing Game ===

1) Play (easy   - 10 tries)
2) Play (normal -  7 tries)
3) Play (hard   -  5 tries)
4) Quit
Choose an option: 2

I'm thinking of a number between 1 and 100.
You have 7 tries. Good luck!
[7 left] Your guess (1-100): 50
Too high. Aim lower.
[6 left] Your guess (1-100): 25
Too low. Aim higher.
[5 left] Your guess (1-100): 37
Too high. Aim lower.
[4 left] Your guess (1-100): 31
Correct! You got it in 4 tries. 🎉

... (menu repeats) ...
Choose an option: 4
Thanks for playing. Goodbye!
```

### Grade Tracker

```
=== Grade Tracker ===

1) Add a student score
2) List students and grades
3) Show summary (average / highest / lowest)
4) Quit
Choose an option: 1
Student name: Alice
Score (0-100): 95
Added Alice with score 95 (grade A).

... (add Bob 72, Carol 58) ...

Choose an option: 2

Name                   Score   Grade
------------------------------------
Alice                     95       A
Bob                       72       C
Carol                     58       F

Choose an option: 3

Students : 3
Average  : 75.00
Highest  : 95
Lowest   : 58

Choose an option: 4
Goodbye!
```
