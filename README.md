# Linguistics: High-Performance Arabic NLP Library

[![NuGet](https://img.shields.io/nuget/v/Linguistics)](https://nuget.org/packages/Linguistics)
[![CI](https://github.com/ahmadmdabit/Linguistics/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/ahmadmdabit/Linguistics/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-6.0_•_7.0_•_8.0_•_9.0_•_10.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-blue)](LICENSE)

**Linguistics** is a specialized .NET library for Arabic text processing, morphology analysis, and root extraction. Engineered for high-throughput scenarios (search engines, indexing pipelines) where allocation and CPU cycles are critical.

> 📖 **Full API reference and architecture deep-dive: [MANUAL.md](src/Linguistics/MANUAL.md)**

---

## Table of Contents

- [Requirements](#requirements)
- [Installation](#installation)
- [Key Features](#key-features)
- [Quick Start](#quick-start)
- [Performance](#performance)
- [Architecture](#architecture)
- [Acknowledgments](#acknowledgments)
- [Contributing](#contributing)
- [License](#license)

---

## Requirements

| Target | Minimum Version |
|--------|----------------|
| .NET | 6.0, 7.0, 8.0, 9.0, or 10.0 |
| OS | Windows, Linux, macOS (any) |
| Dependencies | None (zero external NuGet dependencies) |

---

## Installation

```bash
dotnet add package Linguistics
```

---

## Key Features

- **Zero-Allocation Architecture:** Built on `Span<T>`, `ref struct`, and `stackalloc` — minimal GC pressure even at millions of words/second.
- **Data/Logic Isolation:** Linguistic data (roots, patterns, stop words, weak letters, strange words) is decoupled from logic and compiled into the DLL — no runtime IO.
- **Advanced Diacritic Engine:**
  - **Bitmask Scanning:** O(1) detection for standard Arabic diacritics.
  - **Greedy Normalization:** Handles complex Quranic rules (e.g., `Hamza` + `Fatha` + `Alef` → `Alef Medda`) before stripping simple marks.
  - **Three Diacritic Categories:** Common (Fatha/Damma/Kasra/Shadda/Sukun/Tanwin), Quranic (Maddah/Hamza Above/Below/Subscript Alef), and Rare/Extended marks.
- **Morphological Analysis (Root Extraction):**
  - Trilateral (3-letter) and Quadrilateral (4-letter) roots.
  - Weak letter handling (I'lal) for First (`وصل` → `صل`), Middle (`قول` → `قل`), and Last (`دعو` → `دع`).
  - Geminated root resolution (Mudha'af: `مدد` → `مد`).
  - Hamzated root handling (المهموز) for Hamza in start, middle, and end positions.
  - Pattern-based root extraction via 40+ morphological templates (Awzan).
  - Sun and Moon letter definite article assimilation.
  - Foreign/strange word filtering (e.g., `مانديلا`, `فرنسا`).
  - **Fuzzy Normalization:** Optional `ى`↔`ي` and `ة`↔`ه` normalization for orthographic variation tolerance.
- **Regex-Free Sanitization:** Optimized filters for non-alphanumeric removal — 10x faster than `Regex.Replace`.
- **Integer-Packed Lookups:** Roots and weak letters packed into `ulong`/`uint` for O(1) hash-based lookups with zero string allocation.

---

## Quick Start

### 1. Removing Diacritics (Hot Path)

```csharp
using Linguistics;

string text = "بِسْمِ اللَّهِ الرَّحْمَنِ الرَّحِيمِ";

// Returns original string if no diacritics found (zero allocation)
string clean = ArabicDiacritics.RemoveDiacritics(text);
// Output: "بسم الله الرحمن الرحيم"
```

### 2. Quranic Normalization (Replacement Engine)

```csharp
using Linguistics;
using Linguistics.Data;

string quranText = "ٱلْحَمْدُ"; // Contains Alef Wasla (\u0671)

// Greedy matching replaces compound symbols before stripping remaining marks
string normalized = ArabicDiacritics.Normalize(quranText, ArabicDiacriticsPatterns.Patterns);
// Output: "الحمد"
```

### 3. Root Extraction (Stemming)

```csharp
using Linguistics;

string word = "يكتبون"; // "They are writing"

// Runs full pipeline: Clean → Filter → Stem → Resolve
string root = ArabicMorphologyHelper.FormatWord(word, applyFuzzyNormalization: true);
// Output: "كتب" (K-T-B root)
```

### 4. Text Sanitization (via Facade)

```csharp
using COMN.Utils;

string dirty = "Hello! @تَجرُبَة#";

// Removes diacritics AND non-alphanumeric symbols — no regex allocations
string clean = TextUtils.RemoveTashkil(dirty, removeNoneAlphaNum: true);
// Output: "Helloتجربة"
```

### 5. Zero-Allocation Root Extraction

```csharp
using Linguistics;

ReadOnlySpan<char> input = "الكتاب";
Span<char> outputBuffer = stackalloc char[64];

int length = ArabicMorphologyHelper.FormatWord(input, outputBuffer, applyFuzzyNormalization: alse);
string root = new string(outputBuffer.Slice(0, length));
// Output: "كتب" — zero heap allocations for processing
```

> ⚠️ **Exception handling:** Methods throw `ArgumentException` on null/empty input. All public methods are thread-safe.

---

## Performance

This library avoids `string.Replace` and `Regex` on hot paths:

| Operation | Technique | Complexity |
|-----------|-----------|------------|
| Diacritic detection | Bitmask scan (3 category masks) | O(1) per char |
| Root lookup | `ulong`-packed hashset | O(1) |
| Weak letter lookup | `uint`-packed hashset | O(1) |
| Geminated root lookup | `uint`-packed hashset | O(1) |
| Stop word lookup (Span) | Length-bucketed linear scan | O(k), k = bucket size |
| Pattern matching | Length-bucketed with pre-calculated root indices | O(p), p = patterns per bucket |
| Text buffers | `stackalloc` / `ArrayPool<char>` | Zero heap alloc |
| Data loading | Compiled into DLL (static fields) | Zero IO at runtime |

For detailed performance characteristics, buffer sizes, and memory profiles, see [MANUAL.md → Advanced Usage](src/Linguistics/MANUAL.md#6-advanced-usage--performance).
 
---

## Architecture

The library follows a **SOLID architecture** with clear separation of concerns:

```
Linguistics (namespace)
├── ArabicDiacritics          — Diacritic detection, removal, normalization
├── ArabicDiacriticsPattern   — Compiled pattern struct for normalization rules
├── ArabicMorphologyHelper    — Main orchestrator (FormatWord pipeline)
├── MorphologyResult          — Ref struct for zero-allocation word mutation
├── ArabicRootExtractor       — Root extraction logic (geminated, hamzated, weak)
├── ArabicAffixStripper       — Affix stripping (articles, prefixes, suffixes)
├── ArabicTriRoots            — Trilateral root validation (ulong-packed)
├── ArabicQuadRoots           — Quadrilateral root validation (ulong-packed)
├── ArabicTriPatterns         — Pattern-based root extraction (Awzan)
├── ArabicDuplicates          — Geminated root detection (uint-packed)
├── ArabicFirstWeaks          — First-position weak letter detection
├── ArabicMiddleWeaks         — Middle-position weak letter detection
├── ArabicLastWeaks           — Last-position weak letter detection
├── ArabicStopWords           — Stop word filtering (length-bucketed)
├── ArabicStrange             — Foreign word filtering (length-bucketed)
├── TextPunctuation           — Punctuation detection and removal
│
└── Data (namespace)
    ├── ArabicConstants       — String constants for Arabic characters
    ├── ArabicConstantsChar   — Char constants for high-performance processing
    ├── ArabicRootsData       — Trilateral, Quadrilateral, Geminated root sets
    ├── ArabicAffixesData     — Prefixes, Suffixes, Definite Articles
    ├── ArabicStopWordsData   — Stop word list
    ├── ArabicStrangeData     — Foreign word list
    ├── ArabicWeaksData       — Weak letter data (by position)
    └── ArabicPatternsData    — Morphology templates + Diacritic normalization patterns
```

---

## Acknowledgments

Built upon **Khoja's Arabic Stemmer** (Khoja & Garside, 1999), extended and optimized for:

- Zero-allocation design with modern .NET (`Span<T>`, `ref struct`)
- SOLID architecture (`ArabicRootExtractor`, `ArabicAffixStripper`)
- Enhanced accuracy: Pattern X geminated root handling, priority-based weak root resolution
- Production validation: 97.9% test coverage

**Reference:**
Khoja, S., & Garside, R. (1999). *Stemming Arabic Text*. Lancaster, UK: Computing Department, Lancaster University.

---

## Contributing

Contributions are welcome! Please open an issue or pull request on [GitHub](https://github.com/ahmadmdabit/Linguistics). For local development:

```bash
dotnet restore src/Linguistics.slnx
dotnet build src/Linguistics.slnx
dotnet test src/Linguistics.slnx
dotnet test src/Linguistics.slnx --settings src/codecoverage.runsettings --collect "Code Coverage;Format=cobertura"

Test summary: total: 191, failed: 0, succeeded: 191, skipped: 0, duration: 5s
```

**Bonus:**

Install [DotCov](https://github.com/ANcpLua/dotcov) which is a toolkit streams Cobertura XML coverage — zero-dependency parser and dotnet global tool. Handles 50 MB+ reports without loading the DOM.

```bash
dotnet tool install -g DotCov.Tool

dotcov report src/Linguistics.Tests/TestResults/f1c4408e-8eec-4c51-9965-73219968d341/coverage-2026-05-24.20-30-36.cobertura.xml

File                                                                                 Lines    Line %    Branches  Branch %
--------------------------------------------------------------------------------------------------------------------------
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicRootExtractor.cs          100/147     68.0%      69/102     67.6%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicStrange.cs                  25/34     73.5%       12/16     75.0%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicStopWords.cs                32/37     86.5%       14/16     87.5%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicDiacritics.cs             161/172     93.6%       60/68     88.2%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicTriPatterns.cs              75/80     93.8%       47/50     94.0%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicFirstWeaks.cs               17/18     94.4%         5/6     83.3%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicMorphologyHelper.cs       129/136     94.9%       75/82     91.5%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicAffixStripper.cs            57/59     96.6%       25/30     83.3%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\TextPunctuation.cs              100/102     98.0%       43/44     97.7%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicDefiniteArticle.cs            6/6    100.0%           -         -
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicDiacriticsPattern.cs        14/14    100.0%         2/2    100.0%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicDuplicates.cs               13/13    100.0%         5/6     83.3%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicLastWeaks.cs                22/22    100.0%         5/6     83.3%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicMiddleWeaks.cs              18/18    100.0%         5/6     83.3%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicPrefixes.cs                   6/6    100.0%           -         -
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicQuadRoots.cs                25/25    100.0%         5/6     83.3%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicSuffixes.cs                   6/6    100.0%           -         -
D:\dev-projects\dotnet\Linguistics\src\Linguistics\ArabicTriRoots.cs                 23/23    100.0%         5/6     83.3%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\MorphologyResult.cs               64/64    100.0%       33/34     97.1%
D:\dev-projects\dotnet\Linguistics\src\Linguistics\Data\ArabicAffixesData.cs         12/12    100.0%           -         -
D:\dev-projects\dotnet\Linguistics\src\Linguistics\Data\ArabicPatternsData.cs      105/105    100.0%           -         -
D:\dev-projects\dotnet\Linguistics\src\Linguistics\Data\ArabicRootsData.cs         392/392    100.0%           -         -
D:\dev-projects\dotnet\Linguistics\src\Linguistics\Data\ArabicStopWordsData.cs       30/30    100.0%           -         -
D:\dev-projects\dotnet\Linguistics\src\Linguistics\Data\ArabicStrangeData.cs           4/4    100.0%           -         -
D:\dev-projects\dotnet\Linguistics\src\Linguistics\Data\ArabicWeaksData.cs         243/243    100.0%           -         -
--------------------------------------------------------------------------------------------------------------------------
TOTAL                                                                            1679/1768     95.0%     410/480     85.4%
```

The test suite uses **NUnit** with **FluentAssertions** and runs with parallel fixture execution for fast feedback. Tests cover:
- All public API methods across every class
- Morphological phenomena (geminated, hamzated, weak, pattern-based)
- Data integrity (sorting, non-empty, consistency between String and Span overloads)
- Edge cases (buffer overflow, sun/moon letters, complex affix combinations)

---

## License

[MIT License](LICENSE).
