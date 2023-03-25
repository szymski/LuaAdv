Configuration
=====

## How it works
To start coding in LuaAdv, you need to create a configuration file. It's a simple JSON file that tells the `LuaAdv Watcher` what to compile and where to place it, including some other options.
The config's file name is preferably `.lua_adv`

When you have the file ready, just open it using `LuaAdv Watcher` executable.

## Example
```json
{
    // Optional comment to include on top of each file
	"comment": [
		"--[[",
		"	%filename%",
		"	%date% %time%",
		"	Compiled using LuaAdv",
		"	This file should not be modified.",
		"	",
		"	Copyright © %year% Your Name",
		"]]"
	],

    // The directory where the .luaa files are located
	"input_dir": ".",
    // Where to place the compiled .lua files
	"output_dir": "../lua/",
    // Optional: Where to place the documentation files if you generate them
	"docs_dir": "../docs/",

    // Optional: List of files to include globally, used mainly to make enums visible from every file
	"includes": [
		"global.luaa"
	],

    // Optional: Use code obfuscation techniques to make the compiled code harder to read and modify
    "obfuscate": true,
}
```