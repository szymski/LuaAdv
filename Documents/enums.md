Enum specification
=====

## How it works
Enum references are replaced to their values by the compiler.
They aren't visible in compiled Lua code.

## Construction

NOT IMPLEMENTED YET
```
enum NAMED_VARIABLE {
    NAME = value,
    NAME = value,
    NAME = value,
}
```

NOT IMPLEMENTED YET
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