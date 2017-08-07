Enum specification
=====

## How it works
Enum references are replaced to their values by the compiler.
They aren't visible in compiled Lua code.

## Construction

```
enum NAMED_VARIABLE {
    NAME = value,
    NAME = value,
    NAME = value,
}
```

```
// Index starts at 0 and increments
enum NAMED_VARIABLE {
    NAME, // 0
    NAME, // 1
    NAME, // 2
}
```

```
enum NAMED_VARIABLE = expression;
```