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
