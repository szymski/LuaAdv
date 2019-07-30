Decorators
=====

## How it works
Functions and classes can be modified using decorator functions.

## Construction
```
// A decorator function must be declared.
// It takes decorator parameters and returns a new function, usually a wrapper.
function decorator(message) {
    return (func, ...) => {
        var result = func(...);
        print(message .. result);
        return result * 10;
    };
}

function add(a, b) {
    return a + b;
}

// Decorate the function by placing a decorator call preceded by @, before function declaration.
// If decorator takes no parameters, parentheses are redundant.
@decorator("Original result: ")
function decoratedAdd(a, b) {
    return a + b;
}

assert(add(1, 2) == 3);
assert(decoratedAdd(1, 2) == 30); // Prints the original result and returns a modified one
```