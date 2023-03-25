String interpolation
=====

## How it works
You can use a special syntax to construct strings from other values. It's similar to TypeScript's or C#'s string interpolation.
This feature enables you to quicker build strings dependent on other values.

## Example
```
var name = "Anders";
var question = `2 + 3 = ${2 + 3}, is that correct, ${name}?`;
```
The above will be compiled into:
```lua
local name = "Anders"
local question = ("2 + 3 = " .. 5 .. ", is that correct, " .. name .. "?")
```