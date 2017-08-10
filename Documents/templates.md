Template specification
=====

## How it works
NOT IMPLEMENTED YET

## Construction
```
template NAME(arg1, arg2, arg3, ...) {
    mixin("var " .. arg1 .. " = " .. arg2);
    print(mixin(arg1));
}
```

For call "NAME("derp", 123)" Will result in this: 
```
var derp = 123;
print(derp);
```