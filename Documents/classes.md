Classes
=====

## How it works
Object-oriented programming applied to Lua by using metatables.
When a class is preceded with ``local`` keyword, its constructor is a local function.
A child-class code must be run after the parent-class's code has been evaluated - the parent's metatable is required.

## Local classes
A local class creates its metatable and constructor as local variables.

## Partial classes
A partial class can be defined in one than more files. All methods and fields will be joined together.

## Special class fields
- **__type** - Name of the class.
- **__baseclass** - The metatable of the base class.
- **isType** - function used by is node to determine if the class is of specified type.

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