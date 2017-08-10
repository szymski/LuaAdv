--[[
	functions.luaa
	07.08.2017 19:55:46
	Compiled using LuaAdv
]]

function test1()
	print("Hello world!")
end

function test2()
	return print("Hello world!")
end

function getSum(a, b)
	return a + b
end

assert(getSum(2, 3) == 5)
local func1 = function(a, b)
	return a + b
end
local func2 = function(a, b)
	return a + b
end
local func3 = function(a, b)
	return a + b
end
assert(func1(2, 3), 5)
assert(func2(2, 3), 5)
assert(func3(2, 3), 5)