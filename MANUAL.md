# Linguistics Library: Comprehensive Manual

## Table of Contents
1.  [Introduction](#1-introduction)
2.  [Text Normalization & Diacritics](#2-text-normalization--diacritics)
3.  [Morphology & Root Extraction](#3-morphology--root-extraction)
4.  [Data Architecture](#4-data-architecture)
5.  [The TextUtils Facade](#5-the-textutils-facade)
6.  [Advanced Usage & Performance](#6-advanced-usage--performance)
7.  [FAQ & Troubleshooting](#7-faq--troubleshooting)

---

## 1. Introduction

The **Linguistics** library provides low-level, high-performance tools for processing Arabic text. It is designed to be the "engine" behind search engines or text analysis tools.

### The "Zero-Allocation" Philosophy
Most methods in this library accept `ReadOnlySpan<char>` and operate on buffers provided by the caller (or allocated on the stack). This ensures that processing millions of words does not flood the Heap with temporary string objects, keeping the Garbage Collector (GC) idle and application latency low.

---

## 2. Text Normalization & Diacritics

This module handles the cleaning and normalization of text using a dual-engine approach: **Bitmask Scanning** for speed and **Greedy Replacement** for accuracy.

### 2.1 `ArabicDiacritics` Class

#### `IsDiacritic(char c)`
*   **Description:** Checks if a character is a diacritic.
*   **Logic:** Uses a fast Bitmask for standard ranges (`0x064B`-`0x065F`) and a switch expression for extended Quranic marks (e.g., Small High Jeem, Rub El Hizb).
*   **Constants:** Relies on `ArabicConstantsChar` to avoid "magic numbers."
*   **Complexity:** O(1).

#### `RemoveDiacritics(string text)`
*   **Use Case:** Standard Arabic (MSA) where simple stripping is sufficient.
*   **Performance:** 
    *   Scans for the first diacritic. If none found, returns the original string (0 alloc).
    *   If found, allocates a buffer (Stack for <1KB, ArrayPool for larger), copies valid chars, and returns a new string.

#### `Normalize(string text, ArabicDiacriticsPattern[] patterns)`
*   **Use Case:** Quranic or complex text requiring substitution (e.g., `ٱ` $\to$ `ا`).
*   **Algorithm:** **Greedy Matching**. It iterates through the provided patterns (sorted by length descending).
    *   *Example:* It will match `Shadda + Fatha` as a single unit and remove it, rather than removing `Shadda` and leaving a dangling `Fatha`.
*   **Memory:** Uses `ArrayPool<char>` to handle buffer management efficiently.

### 2.2 `ArabicDiacriticsPatterns`
Contains the definitions for normalization.
*   **Patterns:** An array of `ArabicDiacriticsPattern` structs.
*   **Structure:** `{ Unicode, Replacement, IsQuran, IsSpecial }`.
*   **Optimization:** Each pattern caches its `Length` and `StartChar` to allow the replacement engine to "Fast Fail" without string comparison.

### 2.3 `TextPunctuation`
*   **`RemovePunctuation`:** Removes punctuation marks using a cached boolean lookup table for ASCII and a switch for Unicode Arabic punctuation (comma `،`, semicolon `؛`, etc.).

---

## 3. Morphology & Root Extraction

This module attempts to reduce an Arabic word to its 3-letter (Trilateral) or 4-letter (Quadrilateral) root.

### 3.1 `ArabicMorphologyHelper`
The orchestrator static class.

#### `FormatWord(string word, bool applyFuzzyNormalization)`
The main entry point. It executes the following pipeline:
1.  **Allocation:** Creates a `MorphologyResult` on the stack (`stackalloc`).
2.  **Cleaning:** Removes diacritics, punctuation, and non-letters.
3.  **Filtering:** Checks `ArabicStrange` (foreign words) and `ArabicStopWords`.
4.  **Stemming (`StemmingWord`):**
    *   **Length Check:** Dispatches to `IsTwoLetters`, `IsThreeLetters`, `IsFourLetters`.
    *   **Pattern Matching:** Checks against `ArabicTriPatterns` (Awzan).
    *   **Affix Stripping:** Recursively strips Definite Articles, Prefixes, and Suffixes.

### 3.2 `MorphologyResult` (Ref Struct)
A mutable wrapper around a `Span<char>`.
*   **Why `ref struct`?** It forces the data to stay on the Stack. It cannot be boxed or stored in a class field.
*   **Functionality:** Acts like a `StringBuilder` but for Spans. Supports `Append`, `Insert`, `TrimStart`, `TrimEnd`.

### 3.3 Root Handling Logic
The library handles specific linguistic challenges:

*   **Weak Letters (I'lal):**
    *   `ArabicFirstWeaks`: Handles verbs losing the first Waw (e.g., `صل` -> `وصل`).
    *   `ArabicMiddleWeaks`: Handles Hollow verbs (e.g., `قل` -> `قول`).
    *   `ArabicLastWeaks`: Handles Defective verbs (e.g., `دع` -> `دعو`).
*   **Gemination (Tad'if):**
    *   `ArabicDuplicates`: Handles doubled letters (e.g., `مد` -> `مدد`).
*   **Fuzzy Normalization:**
    *   If enabled, it attempts to normalize `ى` to `ي` and `ة` to `ه` during root validation if an exact match isn't found.

---

## 4. Data Architecture

The library enforces a strict separation between **Logic** and **Data**.

### 4.1 The `Linguistics.Data` Namespace
All linguistic datasets are isolated here. This reduces cognitive load in logic files and centralizes data management.

*   **`ArabicConstants` / `ArabicConstantsChar`:**
    *   The "Single Source of Truth" for Arabic characters.
    *   `ArabicConstants` provides `string` constants (for UI/Database compatibility).
    *   `ArabicConstantsChar` provides `char` constants (for high-performance Span processing).
*   **`ArabicDiacriticsPatterns`:** Contains the authoritative, greedy-sorted list of normalization rules.
*   **`RootsData` (Internal):** Stores the HashSets for Trilateral, Quadrilateral, and Geminated roots.
*   **`AffixesData` (Internal):** Stores Prefixes, Suffixes, and Definite Articles.

### 4.2 Compile-Time Optimization
Data is hardcoded into `static readonly` fields. We explicitly avoid loading data from text files at runtime to prevent:
1.  **IO Latency:** No disk reads during startup.
2.  **Memory Spikes:** No parsing overhead (String Splitting/Allocation) during initialization.
3.  **Deployment Risks:** The DLL is self-contained; no external assets are required.

---

## 5. The TextUtils Facade

Located in `COMN.Utils`, this class acts as a bridge between the high-performance `Linguistics` library and legacy application layers.

### Key Methods
*   **`RemoveTashkil`:** Smartly delegates to `ArabicDiacritics.RemoveDiacritics` (for standard text) or `ArabicDiacritics.Normalize` (for Quranic text).
*   **`RemoveNoneAlphaNum`:** A Regex-free implementation using `string.Create` to sanitize text 10x faster than standard Regex.
*   **`SplitByNewLines`:** Uses `Span.EnumerateLines` to split text without allocating intermediate strings for the split operation itself.

### Data Projection
`TextUtils` projects internal Linguistics data (like patterns) into public read-only fields (`TashkilPatternString`, `Arabic_Letters_TashkilPatterns`) to support SQL generation and Regex compilation in the Business Logic Layer (BLL).

---

## 6. Advanced Usage & Performance

### Thread Safety
All static data structures (`HashSet`, arrays) are read-only after static initialization. The library is fully thread-safe for concurrent use.

### Memory Considerations
*   **Stack Limit:** `ArabicMorphologyHelper` uses a buffer of 64 chars. This is sufficient for any valid Arabic word.
*   **ArrayPool:** `RemoveDiacritics` and `Normalize` use `ArrayPool<char>.Shared`. If you process extremely large strings (MBs in size), ensure you are not holding references to the results longer than necessary.

### Extending the Library
To add new roots or stop words:
1.  Modify the specific `*Data` class in `Linguistics.Data`.
2.  Recompile. The static constructors will automatically re-index and optimize the new data (e.g., packing roots into integers).

---

## 7. FAQ & Troubleshooting

### Q: What .NET versions does Linguistics support?
.NET 6.0, 7.0, 8.0, 9.0, and 10.0. The library has zero external NuGet dependencies and runs on Windows, Linux, and macOS.

### Q: Can I use Linguistics in a Blazor WebAssembly app?
Yes. The library targets `net6.0+` and uses no platform-specific APIs. All logic is pure managed code.

### Q: Does this library load external data files at runtime?
No. All linguistic data (roots, patterns, stop words) is compiled into the DLL as `static readonly` fields. There is zero IO during initialization.

### Q: Are all methods thread-safe?
Yes. All static data structures are read-only after static initialization. The library is fully safe for concurrent use from multiple threads.

### Q: `RemoveDiacritics` returns null or throws for empty strings?
`RemoveDiacritics` accepts `string` input. An empty string returns `string.Empty`. `null` input throws `ArgumentNullException`. Always pass valid strings or guard your input.

### Q: Root extraction returned an unexpected result. How can I debug it?
The pipeline executes: **Clean → Filter → Stem → Resolve Weak Letters → Resolve Gemination → Fuzzy Normalize**. If a word is not in the internal root dictionary, the algorithm falls through to affix stripping heuristics. Try setting `applyFuzzyNormalization: false` to isolate the issue. For unsupported words, add them via the [Extending the Library](#extending-the-library) process.

### Q: What does "Zero-Allocation" mean in practice?
"Zero heap allocation on the hot path" means that for many operations (e.g., checking if a char is a diacritic, removing punctuation, stripping simple marks), no managed heap memory is allocated. Methods that return a new string (like `RemoveDiacritics`) will allocate the result string on the heap — that is unavoidable in .NET. The library ensures no intermediate allocations occur during processing.

### Q: Does `FormatWord` modify the original string?
No. `FormatWord` operates on a `Span<char>` allocated on the stack via `stackalloc`. The original string is never modified.

### Q: How large is the library binary?
The compiled DLL is approximately 400 KB. This includes all embedded linguistic data.

### Q: The library is missing a root or stop word. How do I add it?
See [Extending the Library](#extending-the-library) above. Modify the corresponding `*Data` class and recompile. Submit a pull request on [GitHub](https://github.com/ahmadmdabit/Linguistics) to share your additions.

### Q: Can I contribute?
Yes. See the [Contributing](README.md#contributing) section in the README. Fork the repo, make your changes, and open a pull request.
