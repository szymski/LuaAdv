------------------------------------

CTest = { }
CTest.__index = CTest
CTest.__baseclass = nil

function CTest:__this()
    self.a = 1
    self.b = 2

    print("Constructor")
end

function CTest:getSum()
    return self.a + self.b
end

function Test()
    local tbl = { }
    setmetatable(tbl, CTest)
    tbl:__this()
    return tbl
end

------------------------------------

function inherit(class, base)
    assert(not class.__baseclass, "Base class already set.")
    class.__baseclass = base
end

CChild = { }
CChild.__index = function(tbl, key)
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
CChild.__baseclass = nil

inherit(CChild, CTest)

function CChild:__this()
    getmetatable(self).__baseclass.__this(self)

    self.a = 11

    print("Constructor 2")
end

function CChild:getSum()
    return getmetatable(self).__baseclass.getSum(self) - 1 -- return super() - 1;
end

function CChild:getSumSquared()
    return self:getSum() ^ 2
end

function Child()
    local tbl = { }
    setmetatable(tbl, CChild)
    tbl:__this()
    return tbl
end

------------------------------------

local test = Test()
print(test:getSum())

local child = Child()
print(child:getSum())
print(child:getSumSquared())