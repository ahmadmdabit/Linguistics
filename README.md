# Linguistics: High-Performance Arabic NLP Library

[![NuGet](https://img.shields.io/nuget/v/Linguistics)](https://nuget.org/packages/Linguistics)
[![CI](https://github.com/ahmadmdabit/Linguistics/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/ahmadmdabit/Linguistics/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-6.0_|_7.0_|_8.0_|_9.0_|_10.0-purple)](https://dotnet.microsoft.com/)
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

```sh
dotnet add package Linguistics
```

---

## Key Features

- **Zero-Allocation Architecture:** Built on `Span<T>`, `ref struct`, and `stackalloc` — minimal GC pressure even at millions of words/second.
- **Data/Logic Isolation:** Linguistic data (roots, patterns, stop words) is decoupled from logic and compiled into the DLL — no runtime IO.
- **Advanced Diacritic Engine:**
  - **Bitmask Scanning:** O(1) detection for standard Arabic diacritics.
  - **Greedy Normalization:** Handles complex Quranic rules (e.g., `Hamza` + `Fatha` + `Alef` → `Alef Medda`) before stripping simple marks.
- **Morphological Analysis (Root Extraction):**
  - Trilateral (3-letter) and Quadrilateral (4-letter) roots.
  - Weak letter handling (I'lal) for First (`وصل` → `صل`), Middle (`قول` → `قل`), and Last (`دعو` → `دع`).
  - Geminated root resolution (Mudha'af: `مدد` → `مد`).
  - **Fuzzy Normalization:** Optional `ى`↔`ي` and `ة`↔`ه` normalization for orthographic variation tolerance.
- **Regex-Free Sanitization:** Optimized filters for non-alphanumeric removal — 10x faster than `Regex.Replace`.

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

> ⚠️ **Exception handling:** Methods throw `ArgumentException` on null/empty input. All public methods are thread-safe.

---

## Performance

This library avoids `string.Replace` and `Regex` on hot paths:

| Operation | Technique | Complexity |
|-----------|-----------|------------|
| Diacritic detection | Bitmask scan | O(1) per char |
| Root lookup | `ulong`-packed hashset | O(1) |
| Text buffers | `stackalloc` / `ArrayPool<char>` | Zero heap alloc |
| Data loading | Compiled into DLL (static fields) | Zero IO at runtime |

For detailed performance characteristics, buffer sizes, and memory profiles, see [MANUAL.md → Advanced Usage](src/Linguistics/MANUAL.md#6-advanced-usage--performance).

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

```sh
dotnet restore src/Linguistics.slnx
dotnet build src/Linguistics.slnx
```

---

## License

[MIT License](LICENSE).