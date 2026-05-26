# Cache Eviction Algorithms / Алгоритми витіснення кешу

[English](#english) | [Українська](#українська)

---

## English

Implementation and comparative analysis of cache eviction algorithms in C# / .NET 10.

### Overview

This project implements several classic cache eviction strategies and provides a benchmark harness for comparing their performance under different synthetic workloads. Developed as a coursework project for the *Programming / OOP / Algorithms* course.

### Implemented algorithms

- **FIFO** — First-In-First-Out
- **LRU** — Least Recently Used
- **LFU** — Least Frequently Used
- **MRU** — Most Recently Used

### Project structure

| Project | Purpose |
| --- | --- |
| `CacheAlgorithms.Core` | Library containing cache strategy implementations |
| `CacheAlgorithms.Benchmark` | Console application for running experiments and producing plots |
| `CacheAlgorithms.Tests` | xUnit test suite |

### Technology stack

- **.NET 10** (LTS) with **C# 14**
- **xUnit** for unit testing
- **ScottPlot** for benchmark visualization

### Build and run

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
# Clone the repository
git clone https://github.com/g1olog1c/cache-algorithms-coursework.git
cd cache-algorithms-coursework

# Restore dependencies and build
dotnet restore
dotnet build

# Run the benchmark suite
dotnet run --project CacheAlgorithms.Benchmark

# Run unit tests
dotnet test
```

### Experimental methodology

Each algorithm is evaluated against synthetic request streams generated under two distributions:

- **Uniform** — keys drawn with equal probability from the key space
- **Zipfian** — keys drawn with power-law distribution, modeling realistic access patterns where a small subset of keys receives disproportionate traffic

Cache sizes tested: 50, 100, and 500 entries. Total requests per run: 10,000.

Metrics collected: hit rate, miss rate, average lookup time, number of evictions.

### License

This project is developed for educational purposes.

---

## Українська

Реалізація та порівняльний аналіз алгоритмів витіснення кешу мовою C# на платформі .NET 10.

### Загальний опис

Проєкт містить реалізації кількох класичних стратегій витіснення кешу та інструменти для їх експериментального порівняння на синтетичних навантаженнях різного типу. Розроблено як курсова робота з дисципліни *«Програмування / ООП / Алгоритми»*.

### Реалізовані алгоритми

- **FIFO** — First-In-First-Out (першим прийшов — першим вийшов)
- **LRU** — Least Recently Used (давно не використовуваний)
- **LFU** — Least Frequently Used (найрідше використовуваний)
- **MRU** — Most Recently Used (нещодавно використовуваний)

### Структура проєкту

| Проєкт | Призначення |
| --- | --- |
| `CacheAlgorithms.Core` | Бібліотека з реалізаціями алгоритмів кешування |
| `CacheAlgorithms.Benchmark` | Консольний застосунок для проведення експериментів і побудови графіків |
| `CacheAlgorithms.Tests` | Юніт-тести на основі xUnit |

### Технологічний стек

- **.NET 10** (LTS) та **C# 14**
- **xUnit** для модульного тестування
- **ScottPlot** для візуалізації результатів експериментів

### Збірка та запуск

Передумова: встановлений [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
# Клонування репозиторію
git clone https://github.com/g1olog1c/cache-algorithms-coursework.git
cd cache-algorithms-coursework

# Відновлення залежностей та збірка
dotnet restore
dotnet build

# Запуск бенчмарків
dotnet run --project CacheAlgorithms.Benchmark

# Запуск юніт-тестів
dotnet test
```

### Методика експериментів

Кожен алгоритм досліджується на синтетичних потоках запитів, згенерованих за двома розподілами:

- **Рівномірний** — ключі обираються з однаковою ймовірністю з усього простору
- **Розподіл Зіпфа** — ключі обираються за степеневим законом, що моделює реалістичні патерни доступу, де невелика підмножина ключів отримує непропорційно велику частку звертань

Тестовані розміри кешу: 50, 100 та 500 елементів. Кількість запитів на один прогін: 10 000.

Збираються метрики: hit rate, miss rate, середній час пошуку, кількість витіснень.

### Ліцензія

Проєкт розроблено в навчальних цілях.