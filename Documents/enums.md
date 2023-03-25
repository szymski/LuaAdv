Enum specification
=====

## How it works
Enum references are replaced to their values by the compiler.
There are two types of enums:
- key-value enum
- single variable enum

## Construction - Key-value enum
You just reference the enum by its name and a dot followed by one of its keys.
```
enum MY_ENUM {
    ITEM1 = "a",
    ITEM2 = "b",
    ITEM3 = "c",
}

// Index starts at 0 and increments
enum AUTO_ENUM {
    ZERO, // 0
    FIRST, // 1
    SECOND, // 2
}

var a = MY_ENUM.ITEM1;
assert(a == "a")

var first = AUTO_ENUM.FIRST;
assert(first == 1);
```

## Construction - Single variable enum
This type of enum can only hold a single value, but don't lose your interest yet - it's far more powerful than it might sound at first.

```
enum NAME = expression;
```

You can think of it as sort of a preprocessor define (like in C), except LuaAdv doesn't have a preprocessor. Here static ifs enter the stage.
Example of usage:
```
enum DEBUG = true;
enum VERSION = 1;

static if(VERSION == 1) {
    print("This feature is only available in version 1");
}
else if(VERSION == 2) {
    print("And this only in version 2...");
}

static if(DEBUG) {
    print(`[Debug] This message is only visible in debug mode! Version: ${VERSION}`);
}
else {
    print("Welcome to release mode!");
}
```
The above code after compilation to Lua will look like this:
```lua
print("This feature is only available in version 1")
print(("[Debug] This message is only visible in debug mode! Version: " .. 1))
```
Or like this if `VERSION = 2` and `DEBUG = false`:
```lua
print("And this only in version 2...")
print("Welcome to release mode!")
```
As you can see, the enums themselves aren't present in the compiled code, but all references to them are replaced with their values.
Static ifs are if statements that are evaluated at compile time. You can use them to control which code paths are included in the compiled code.
Keep in mind that while you can use anything as an enum's value, even identifiers to global variables, only primitive types (numbers, strings, booleans) whose values are known at compile time are allowed in static if conditions.

One use case of static ifs is to include debug code only in debug builds, while working on the project and not in release builds.
They can also be used to produce demo builds with some of the features missing, without possibility of enabling them by editing the demo build.
Besides above, there are still many other use cases, but I'll leave it up to you to find them.

## Configuration
It's best to keep enums in a separate global file, so that they are visible from every file in the project. For that purpose, you can add the name of the global file into the `includes` array in the `.lua_adv` file.
Example:
```json
{
    /* ... */
	"includes": [
		"global.luaa"
	],
    /* ... */
} 
```