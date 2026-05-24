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

### Supported Arabic Linguistic Phenomena
The library handles the full spectrum of Arabic morphological phenomena including: **Trilateral** (3-letter) and **Quadrilateral** (4-letter) roots, **Geminated** roots (المضعف, e.g., `مد` → `مدد`), **Hamzated** roots (المهموز, containing Hamza), **Weak** roots (المعتل) — categorized as **Mithal** (first weak, e.g., `وصل` → `صل`), **Ajwaf** (middle weak/hollow, e.g., `قول` → `قل`), and **Naqis** (last weak/defective, e.g., `دعو` → `دع`), **Pattern-based** root extraction via Awzan (أوزان), **Sun and Moon letter** definite article assimilation, **Quranic orthography** normalization (Alef Wasla, Small Alef, etc.), and **Fuzzy normalization** for orthographic variations (`ى`↔`ي`, `ة`↔`ه`).

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
*   **Null Handling:** Returns the original string (including `null`) without throwing.

#### `RemoveDiacritics(ReadOnlySpan<char> source, Span<char> destination)`
*   **Use Case:** Zero-allocation diacritic removal with caller-provided buffer. Supports in-place modification (source and destination can be the same buffer).
*   **Returns:** Number of characters written to destination.
   **Throws:** `ArgumentException` if destination buffer is too small.
 
#### `Normalize(string text, ArabicDiacriticsPattern[] patterns)`
*   **Use Case:** Quranic or complex text requiring substitution (e.g., `ٱ` $\to$ `ا`).
*   **Algorithm:** **Greedy Matching**. It iterates through the provided patterns (sorted by length descending).
    *   *Example:* It will match `Shadda + Fatha` as a single unit and remove it, rather than removing `Shadda` and leaving a dangling `Fatha`.
*   **Memory:** Uses `ArrayPool<char>` to handle buffer management efficiently.

#### `IsCommonDiacritic(char c)`
*   **Description:** Checks if a character is a common diacritic (Fatha, Damma, Kasra, Shadda, Sukun, Tanwin variants).
*   **Complexity:** O(1) — separate bitmask for common marks.

#### `IsQuranicOrMark(char c)`
*   **Description:** Checks if a character is a Quranic-specific mark (Maddah, Hamza Above/Below, Subscript Alef, etc.).
*   **Complexity:** O(1) — separate bitmask for Quranic marks.

#### `IsRareOrExtended(char c)`
*   **Description:** Checks if a character is a rare/extended diacritic (Fatha with Two Dots, Wavy Hamza, Vertical Zigzag Fatha, etc.).
*   **Complexity:** O(1) — separate bitmask for rare marks.

### 2.2 `ArabicDiacriticsPatterns`
Contains the definitions for normalization.
*   **Patterns:** An array of `ArabicDiacriticsPattern` structs.
*   **Structure:** `{ Unicode, Replacement, IsQuran, IsSpecial }`.
*   **Optimization:** Each pattern caches its `Length` and `StartChar` to allow the replacement engine to "Fast Fail" without string comparison.
*   **Source:** `ArabicPatternsData.DiacriticNormalization` provides the authoritative, greedy-sorted array.

### 2.3 `ArabicDiacriticsPattern` Struct
A compiled pattern for diacritic normalization or removal.
*   **Properties:** `Unicode` (the match string), `Replacement` (replacement string, empty = remove), `IsQuran`, `IsSpecial`.
*   **Optimization:** Caches `Length` and `StartChar` at construction time for O(1) fast-fail during scanning.
*   **Struct Type:** Value type — zero-allocation when passed around.

### 2.4 `TextPunctuation`
*   **`RemovePunctuation`:** Removes punctuation marks using a cached boolean lookup table for ASCII and a switch for Unicode Arabic punctuation (comma `،`, semicolon `؛`, etc.).
*   **`RemovePunctuation(ReadOnlySpan, Span)`:** Zero-allocation overload with caller-provided buffer. Supports in-place modification.
*   **`IsPunctuation(char)`:** O(1) check using 128-byte ASCII lookup table + switch expression for Unicode.
*   **Covers:** Standard ASCII symbols, Arabic-specific punctuation (`،`, `؛`, `¿`), and extended Unicode (`¡`, `¿`, `÷`, `×`, `º`, `ø`).

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

#### `FormatWord(ReadOnlySpan<char> inputWord, Span<char> outputBuffer, bool applyFuzzyNormalization)`
*   **Description:** Zero-allocation overload. Processes the word and writes the result to the caller-provided `outputBuffer`.
*   **Returns:** Number of characters written (0 if input is empty/whitespace or output buffer is too small).
*   **Buffer Requirement:** `outputBuffer` must be at least 64 chars (the internal working buffer size).
*   **Use Case:** Hot-path processing where even the result string allocation must be avoided.

### 3.2 `MorphologyResult` (Ref Struct)
A mutable wrapper around a `Span<char>`.
*   **Why `ref struct`?** It forces the data to stay on the Stack. It cannot be boxed or stored in a class field.
*   **Functionality:** Acts like a `StringBuilder` but for Spans. Supports `Append`, `Insert`, `TrimStart`, `TrimEnd`.
*   **Additional Members:** `RawBuffer` (exposes the full capacity buffer for external `CopyTo` operations), `UpdateLength(int)` (updates the length after writing to `RawBuffer`), `SetChar(int, char)` (modifies a character at a specific index).
*   **State Flags:** `IsRootFound`, `IsStopWord`, `IsStrangeWord`, `IsPatternFound`, `IsProcessingSuffixes`.
*   **Computed Property:** `IsFinished` returns `true` if any terminal flag is set.
*   **Capacity:** 64 chars by default (sufficient for any valid Arabic word).

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

### 3.4 `ArabicRootExtractor` Class
Responsible for extracting roots from already-cleaned Arabic words. Handles geminated, hamzated, and weak roots.
*   **`ExtractRoot(ref MorphologyResult)`:** Dispatches to `IsTwoLetters`, `IsThreeLetters`, or `IsFourLetters` based on word length.
*   **Two-letter words:** Treated as geminated roots (e.g., `مد` → `مدд`). Also attempts First/Last/Middle weak resolution.
*   **Three-letter words:** The core trilateral root logic:
    1. Normalizes first-character Hamza/Alif variants.
    2. Handles weak last letter (Alef, Waw, Ya, Alef Maqsura, Hamza).
    3. Handles weak middle letter (Waw, Ya, Alef).
    4. Handles Hamza on Waw/Ya in middle position.
    5. Direct root lookup via `ArabicTriRoots`.
    6. Fuzzy normalization fallback (`ى`→`ي`, `ة`→`ه`).
*   **Four-letter words:** Direct quadrilateral root lookup via `ArabicQuadRoots`.

### 3.5 `ArabicAffixStripper` Class
Responsible for stripping affixes (prefixes, suffixes, definite articles) from Arabic words.
*   **`CheckDefiniteArticle`:** Removes definite articles (e.g., `ال`, `وال`, `بال`, `كال`, `فال`). Uses greedy matching (longest first). Falls back to the stripped candidate if no root is found and candidate length > 3.
*   **`CheckPrefixes`:** Removes prefixes (e.g., `ل`, `ب`, `س`, `است`). Greedy matching.
*   **`CheckSuffixes`:** Removes suffixes (e.g., `هم`, `ات`, `ون`). Greedy matching.
*   **`CheckPrefixWaw`:** Handles the conjunction `و` (and) prefix for words longer than 3 characters.
*   **Recursion:** Each method calls `ArabicMorphologyHelper.AnalyzeCandidate` to recursively validate the stripped candidate.

### 3.6 `ArabicTriPatterns` Class
High-performance pattern matcher for Arabic morphological templates (Awzan).
*   **`Pattern` struct:** Pre-compiled pattern with cached root letter indices (R1, R2, R3 for ف, ع, ل).
*   **Length Bucketing:** Patterns are grouped by length for O(1) bucket selection.
*   **`TryExtractRoot`:** Attempts to match the word against known patterns and extract a 3-letter root. Validates the extracted root against `ArabicTriRoots`. Supports fuzzy normalization for the extracted root.

### 3.7 `ArabicDuplicates` Class
Handles geminated (doubled) roots — 2-letter representations of 3-letter roots where the last two letters are identical.
*   **Optimization:** Packs 2 chars into a `uint` for O(1) integer lookup.
*   **Example:** `مد` → `مدد`, `رد` → `ردد`.

### 3.8 `ArabicStrange` Class
Filters out foreign/strange words that should not undergo root extraction.
*   **Optimization:** Uses length-bucketed arrays for zero-allocation Span lookups, plus a `HashSet<string>` for string lookups.
*   **Examples:** `خوجة`, `بلورة`, `تلفزة`, `مانديلا`, `فرنسا`.

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
*   **`ArabicRootsData`:** Stores the `HashSet<string>` for Trilateral, Quadrilateral, and Geminated roots.
*   **`ArabicAffixesData`:** Stores Prefixes, Suffixes, and Definite Articles.
*   **`ArabicStopWordsData`:** Stores the curated list of Arabic stop words (prepositions, pronouns, conjunctions, adverbs, months, numbers).
*   **`ArabicStrangeData`:** Stores the list of foreign/strange words to exclude from root extraction.
*   **`ArabicWeaksData`:** Stores categorized weak root data — `FirstWaw`, `FirstYah`, `MidWaw`, `MidYah`, `LastAlif`, `LastHamza`, `LastMaksoura`, `LastYah`.
*   **`ArabicPatternsData`:** Stores `MorphologyTemplates` (HashSet of pattern strings like `فعّل`, `فاعل`) and `DiacriticNormalization` (the greedy-sorted `ArabicDiacriticsPattern[]` array).

### 4.2 Compile-Time Optimization
Data is hardcoded into `static readonly` fields. We explicitly avoid loading data from text files at runtime to prevent:
1.  **IO Latency:** No disk reads during startup.
2.  **Memory Spikes:** No parsing overhead (String Splitting/Allocation) during initialization.
3.  **Deployment Risks:** The DLL is self-contained; no external assets are required.
 
### 4.3 Integer Packing Optimization
Root and weak-letter lookups use integer packing for O(1) hash-based lookups without string allocation:
*   **Trilateral roots:** 3 chars packed into a `ulong` (48 bits). Stored in `HashSet<ulong>`.
*   **Quadrilateral roots:** 4 chars packed into a `ulong` (64 bits). Stored in `HashSet<ulong>`.
*   **Geminated roots:** 2 chars packed into a `uint` (32 bits). Stored in `HashSet<uint>`.
*   **Weak letter pairs:** 2 chars packed into a `uint` (32 bits). Stored in `HashSet<uint>`.
*   **Benefit:** Zero-allocation lookups — no string objects created during root validation.

### 4.4 Length-Bucketed Lookups
`ArabicStopWords` and `ArabicStrange` use a secondary storage pattern for Span-based zero-allocation lookups:
*   Words are grouped by length into arrays (`string[][]` where index = word length).
   *   For each lookup, the correct bucket is selected by length, then a linear scan with `Span.SequenceEqual` is performed.
   *   Since buckets are small (typically ~30 words), this is faster than allocating a string for `HashSet` lookup.

---

## 5. The TextUtils Facade

Located in `COMN.Utils`, this class acts as a bridge between the high-performance `Linguistics` library and legacy application layers.

### Key Methods
*   **`RemoveTashkil`:** Smartly delegates to `ArabicDiacritics.RemoveDiacritics` (for standard text) or `ArabicDiacritics.Normalize` (for Quranic text).
*   **`RemoveNoneAlphaNum`:** A Regex-free implementation using `string.Create` to sanitize text 10x faster than standard Regex.
*   **`SplitByNewLines`:** Uses `Span.EnumerateLines` to split text without allocating intermediate strings for the split operation itself.

> **Note:** `TextUtils` resides in the `COMN.Utils` namespace and acts as a bridge between the high-performance `Linguistics` library and legacy application layers.

### Data Projection
`TextUtils` projects internal Linguistics data (like patterns) into public read-only fields (`TashkilPatternString`, `Arabic_Letters_TashkilPatterns`) to support SQL generation and Regex compilation in the Business Logic Layer (BLL).

---

## 6. Advanced Usage & Performance

### Thread Safety
All static data structures (`HashSet`, arrays) are read-only after static initialization. The library is fully thread-safe for concurrent use.

### Memory Considerations
*   **Stack Limit:** `ArabicMorphologyHelper` uses a buffer of 64 chars. This is sufficient for any valid Arabic word.
  *   `stackalloc char[64]` is used internally. For words exceeding 64 chars, `FormatWord(string)` throws `ArgumentException`.
*   **ArrayPool:** `RemoveDiacritics` and `Normalize` use `ArrayPool<char>.Shared`. If you process extremely large strings (MBs in size), ensure you are not holding references to the results longer than necessary.
  *   Stack allocation threshold is 1024 chars (2KB) for `RemoveDiacritics` and `RemovePunctuation`. Larger strings use `ArrayPool`.

### Extending the Library
To add new roots or stop words:
1.  Modify the specific `*Data` class in `Linguistics.Data`.
2.  Recompile. The static constructors will automatically re-index and optimize the new data (e.g., packing roots into integers).

### Pattern X vs Pattern VIII Ambiguity Resolution
The library handles the ambiguity between Pattern X (`استفعل`) and Pattern VIII (`افتعل`) when the remainder after stripping `است` is a geminated root. For example, `استمر` correctly resolves to root `مرر` (Pattern X) rather than incorrectly stripping to `سمر` (Pattern VIII). This is handled in `ArabicMorphologyHelper.StemmingWord`.

---

## 7. FAQ & Troubleshooting

### Q: What .NET versions does Linguistics support?
.NET 6.0, 7.0, 8.0, 9.0, and 10.0. The library has zero external NuGet dependencies and runs on Windows, Linux, and macOS.

### Q: What is the maximum word length the library can handle?
The internal working buffer is 64 characters. Any valid Arabic word will fit within this limit. `FormatWord(string)` throws `ArgumentException` for inputs exceeding 64 chars. The `FormatWord(ReadOnlySpan, Span)` overload requires the output buffer to be at least 64 chars.

### Q: Can I use Linguistics in a Blazor WebAssembly app?
Yes. The library targets `net6.0+` and uses no platform-specific APIs. All logic is pure managed code.

### Q: Does this library load external data files at runtime?
No. All linguistic data (roots, patterns, stop words) is compiled into the DLL as `static readonly` fields. There is zero IO during initialization.

### Q: Are all methods thread-safe?
Yes. All static data structures are read-only after static initialization. The library is fully safe for concurrent use from multiple threads.

### Q: Does the library handle Sun and Moon letter assimilation?
Yes. The definite article stripping logic (`CheckDefiniteArticle`) handles both Sun letters (e.g., `الشمس` → `شمس`) and Moon letters (e.g., `القمر` → `قمر`). The `ال` article is simply stripped; the root dictionary contains the correct forms.

### Q: `RemoveDiacritics` returns null or throws for empty strings?
`RemoveDiacritics` accepts `string` input. An empty string returns `string.Empty`. `null` input throws `ArgumentNullException`. Always pass valid strings or guard your input.

### Q: Root extraction returned an unexpected result. How can I debug it?
The pipeline executes: **Clean → Filter → Stem → Resolve Weak Letters → Resolve Gemination → Fuzzy Normalize**. If a word is not in the internal root dictionary, the algorithm falls through to affix stripping heuristics. Try setting `applyFuzzyNormalization: false` to isolate the issue. For unsupported words, add them via the [Extending the Library](#extending-the-library) process.

### Q: What does "Zero-Allocation" mean in practice?
"Zero heap allocation on the hot path" means that for many operations (e.g., checking if a char is a diacritic, removing punctuation, stripping simple marks), no managed heap memory is allocated. Methods that return a new string (like `RemoveDiacritics`) will allocate the result string on the heap — that is unavoidable in .NET. The library ensures no intermediate allocations occur during processing.

### Q: How does the library handle the conjunction prefix `و` (Waw)?
`CheckPrefixWaw` only activates for words longer than 3 characters. For 3-letter words like `وعد` (root W-A-D), the Waw is treated as part of the root and not stripped. This prevents false stripping of roots that naturally begin with Waw.

### Q: Does `FormatWord` modify the original string?
No. `FormatWord` operates on a `Span<char>` allocated on the stack via `stackalloc`. The original string is never modified.

### Q: How large is the library binary?
The compiled DLL is approximately 175 KB. This includes all embedded linguistic data.

### Q: The library is missing a root or stop word. How do I add it?
See [Extending the Library](#extending-the-library) above. Modify the corresponding `*Data` class and recompile. Submit a pull request on [GitHub](https://github.com/ahmadmdabit/Linguistics) to share your additions.

### Q: How do I normalize Quranic text with Alef Wasla and other special marks?
Use `ArabicDiacritics.Normalize(text, ArabicPatternsData.DiacriticNormalization)`. The `DiacriticNormalization` array is pre-sorted by length descending for greedy matching and handles complex combinations like `Hamza + Fatha + Alef → Alef Madda`, `Alef Wasla → Alef`, `Superscript Alef → Alef`, and Shadda+diacritic combinations.

### Q: What is the `IsSpecial` flag on `ArabicDiacriticsPattern`?
The `IsSpecial` flag marks patterns that involve character replacement (not just removal). For example, `Alef Wasla → Alef` is marked `IsSpecial = true`, while simple diacritic removal (e.g., `Fatha → empty`) is `IsSpecial = false`. This flag can be used by tooling to distinguish between pure stripping and normalization operations.

### Q: Can I contribute?
Yes. See the [Contributing](README.md#contributing) section in the README. Fork the repo, make your changes, and open a pull request.
