--[[
	classes.luaa
	07.08.2017 20:11:25
	Compiled using LuaAdv
]]

CA = {}
CA.__index = function(tbl, key)
    local meta = getmetatable(tbl)
    
    local field = meta[key]
    if field then
        return field
    end
    
    local base = meta.__baseclass
    if base then
        return base[key]
    end
end
CA.__type = "A"
CA.__baseclass = nil
CA.isType = function(self, type, metatable)
    if metatable then
        if metatable.__type == type then
            return true
        elseif metatable.__baseclass then
            return self:isType(type, metatable.__baseclass)
        else
            return false
        end
    elseif self.__type == type then
	    return true
	elseif getmetatable(self).__baseclass then
		return self:isType(type, getmetatable(self).__baseclass)
	else 
		return false
	end
end

function A(...)
	local tbl = { }
	setmetatable(tbl, CA)
	tbl:__this(...)
	return tbl
end

function CA:getSum()
	return self.definedVariable + self.undefinedVariable
end

function CA:__this(value)

	self.definedVariable = 123
	
	print("A constructor")
	self.undefinedVariable = value
end

local a = A(2)
assert((a).isType and (a):isType("A") or (type(a) == "A"))
assert(a:getSum() == 125)
CB = {}
CB.__index = function(tbl, key)
    local meta = getmetatable(tbl)
    
    local field = meta[key]
    if field then
        return field
    end
    
    local base = meta.__baseclass
    if base then
        return base[key]
    end
end
CB.__type = "B"
CB.__baseclass = nil
CB.isType = function(self, type, metatable)
    if metatable then
        if metatable.__type == type then
            return true
        elseif metatable.__baseclass then
            return self:isType(type, metatable.__baseclass)
        else
            return false
        end
    elseif self.__type == type then
	    return true
	elseif getmetatable(self).__baseclass then
		return self:isType(type, getmetatable(self).__baseclass)
	else 
		return false
	end
end

if not CA then error("Base class 'A' not found. Class 'B' could not be constructed.") end
CB.__baseclass = CA
function B(...)
	local tbl = { }
	setmetatable(tbl, CB)
	tbl:__this(...)
	return tbl
end

function CB:getSum()
	return getmetatable(self).__baseclass.getSum(self) / 2
end

function CB:getFunctionName()
	return "B:getFunctionName"
end

function CB:__this()
	getmetatable(self).__baseclass.__this(self, 7)
	print("B constructor")
end

local b = B()
assert((b).isType and (b):isType("A") or (type(b) == "A"))
assert((b).isType and (b):isType("B") or (type(b) == "B"))
assert(b:getSum() == 130 / 2)
assert(b:getFunctionName() == "B:getFunctionName")