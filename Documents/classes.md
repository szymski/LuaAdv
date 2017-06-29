Classes
=====

## How it works
Object-oriented programming applied to Lua by using metatables.
When a class is preceded with ``local`` keyword, its constructor is a local function.

## Local classes
A local class creates its metatable and constructor as local variables.

## Partial classes
A partial class can be defined in one than more files. All methods and fields will be joined together.

## Special class fields
__name - Name of the class
__base - A table of base classes

## Construction
```
[local] [partial] class A {
    var NumA, NumB = 1, 2;
    
    function this() {
        print("A Constructor");
    }

    function GetSum() => this.NumA + this.NumB;
}

class B : A {
    function this() {
        super(); // Prints 'A Constructor'
        print("B Constructor");
    }

    function GetSquaredSum() => this:GetSum() ^^ 2;
}

var a = A();
assert(a is A);

var b = B();
assert(b is A);
assert(b is B);
assert(b:GetSquaredSum() == a:GetSum() ^^ 2);
```