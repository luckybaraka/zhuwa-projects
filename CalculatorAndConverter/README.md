# Calculator & Unit Converter

A console application in C# that works as a simple calculator and unit converter.
It can do basic arithmetic, convert distance and temperature units, and convert
Ghanaian Cedi (GHS) to US Dollars (USD) using an exchange rate you type in.

## Features

- **Calculator** — add, subtract, multiply and divide
- **Unit converter**
  - Kilometres ⇄ Miles
  - Celsius ⇄ Fahrenheit
- **Currency converter** — GHS → USD using a rate you provide
- **Safe input handling** — invalid input is rejected with a clear message and
  the program simply asks again instead of crashing

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later

Check your version with:

```bash
dotnet --version
```

## How to run

From the root of the project:

```bash
dotnet run --project src/CalculatorAndConverter
```

Then follow the on-screen menu. Example session:

```
Main menu
1. Calculator
2. Unit converter
3. Currency converter (GHS to USD)
4. Exit
Choose an option: 1

Calculator
1. Add
2. Subtract
3. Multiply
4. Divide
5. Back
Choose an operation: 1
Enter the first number: 12
Enter the second number: 8
Result: 20
```

## How to run the tests

```bash
dotnet test
```

This runs the automated test suite (28 tests covering every calculation and
conversion, including error cases such as divide-by-zero and invalid rates).

## Project structure

```
CalculatorAndConverter.slnx
├── src/
│   └── CalculatorAndConverter/        Console application
│       ├── Program.cs                 Entry point
│       ├── App/                       User interface (menus, input reading)
│       │   ├── ConsoleApp.cs
│       │   ├── InputReader.cs
│       │   └── InputCancelledException.cs
│       └── Core/                      Pure calculation logic (no console I/O)
│           ├── Calculator.cs
│           ├── DistanceConverter.cs
│           ├── TemperatureConverter.cs
│           └── CurrencyConverter.cs
└── tests/
    └── CalculatorAndConverter.Tests/  xUnit test project
```

## Architecture and design decisions

The code is split into two clear layers:

- **Core** holds the actual maths. These classes take numbers in and return
  numbers out — they never touch the console. Because they are pure, they are
  easy to read and easy to test in isolation.
- **App** holds everything to do with the screen and the keyboard: drawing the
  menus and reading input safely. All input goes through `InputReader`, which
  keeps asking until it gets something valid, so a typo can never crash the app.

Keeping calculation separate from input/output is the main design idea. It means
the logic can be unit-tested directly, and either layer can change without
breaking the other.

Other decisions:

- **`double` for maths and measurements, `decimal` for money.** `decimal`
  avoids the tiny rounding errors of floating point, which matters for currency.
- **Divide-by-zero throws a `DivideByZeroException`** in the Core layer and is
  caught in the App layer. The logic stays honest about the error while the user
  still gets a friendly message instead of a crash.
- **One kilometre = 0.621371 miles** is kept in a single named constant so the
  two distance conversions can never disagree with each other.

## Assumptions

- The GHS → USD rate is entered as **how many USD one GHS is worth**. You can
  type your own rate, or press Enter to use the built-in current rate of
  **1 GHS = 0.0859 USD** (mid-market rate as of 6 November 2026, source
  [Wise](https://wise.com/us/currency-converter/ghs-to-usd-rate)). Amount and
  rate must both be greater than zero.
- Numbers are read using the machine's current culture (so `.` or `,` as the
  decimal separator depends on your system settings).
- Converted results are displayed rounded to four decimal places; money is shown
  to two decimal places. The underlying calculations keep full precision.

## Known limitations

- The built-in exchange rate is a fixed value documented above; it is not fetched
  live, so it will drift from the real market rate over time. Type your own rate
  to override it with an up-to-date figure.
- Only the unit pairs listed above are supported.
- The calculator works on two numbers at a time; it does not evaluate full
  expressions such as `2 + 3 * 4`.
```
