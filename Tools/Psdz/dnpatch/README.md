# dnpatch
[WIP] .NET Patcher library using dnlib.

*If you have questions feel free to ask me via Gitter! I'm glad to help you out!*

[![Build status](https://ci.appveyor.com/api/projects/status/39jhu0noimfkgfw2?svg=true)](https://ci.appveyor.com/project/ioncodes/dnpatch)
[![Github All Releases](https://img.shields.io/github/downloads/ioncodes/dnpatch/total.svg)]
(https://github.com/ioncodes/dnpatch/releases)
[![Join the chat at https://gitter.im/dnpatch/Lobby](https://badges.gitter.im/dnpatch/Lobby.svg)](https://gitter.im/dnpatch/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Patching
The constructor takes the filename of the assembly.
```cs
Patcher patcher = new Patcher("Test.exe");
```
If you want to keep the old maxstack (for example for obfuscated assemblies) use the overload:
```cs
Patcher patcher = new Patcher("Test.exe", true);
```

### Targeting Methods
All methods take an object called Target as argument. The object is defined as follows:
```cs
public string Namespace { get; set; } // needed
public string Class { get; set; } // needed
public string Method { get; set; } // needed

/* If you want to patch multiple indexes in the method */
public int[] Indexes { get; set; }
public Instruction[] Instructions { get; set; }

/* If you want to patch 1 index in the method */
public int Index { get; set; } = -1;
public Instruction Instruction { get; set; }

/* If the path to the method has more than 1 nested class use this */
public string[] NestedClasses { get; set; }

/* If the path to the method has 1 nested class use this */
public string NestedClass { get; set; }
```
Please make sure that you don't assign inconsistent values, e.g.
```cs
var target = new Target
{
    Instructions = ...
    Instruction = ...
}
```

If you want to patch multiple methods create a Target[] and pass it to the functions, it is accepted by the most of them.

### Creating Instructions
Reference dnlib and create an Instruction[] or Instruction with your Instruction(s) and assign Indexes (int[]) or Index with the indexes where the Instructions are. You can find them by reverse engineering your assembly via dnSpy or some other decompiler.

Small Example:
```cs
Instruction[] opCodes = {
    Instruction.Create(OpCodes.Ldstr, "Hello Sir 1"),
    Instruction.Create(OpCodes.Ldstr, "Hello Sir 2")
};
int[] indexes = {
    0, // index of Instruction
    2
};
Target target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "Print",
    Instructions = opCodes,
    Indexes = indexes
};
```

### Patch the whole methodbody
To clear the whole methodbody and write your instructions, make sure that you don't assign the Indexes or Index property.

Here is an example:
```cs
Instruction[] opCodes = {
    Instruction.Create(OpCodes.Ldstr, "Hello Sir"), // String to print
    Instruction.Create(OpCodes.Call, p.BuildMemberRef("System", "Console", "WriteLine")), // Console.WriteLine call -> BUILDMEMBERREF IS ONLY FOR CONSOLE.WRITELINE
    Instruction.Create(OpCodes.Ret) // Alaway return smth
};
Target target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "Print",
    Instructions = opCodes
};
```

### Apply the patch
To apply your modified instructions you can call the method 'Patch':
```cs
patcher.Patch(Target);
```
or
```cs
patcher.Patch(Target[]);
```

### Finding an instruction
In some cases it might be useful to have find an instruction within a method, for example if the method got updated.
```cs
Instruction opCode = Instruction.Create(OpCodes.Ldstr, "TheTrain");
Instruction toFind = Instruction.Create(OpCodes.Ldstr, "TheWord");
Target target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "FindMe",
    Instruction = opCode // you can also set it later
};
target.Index = p.FindInstruction(target, toFind);
// now you have the full Target object
```

Let's say there are multiple identical instructions. What now, baoss? Well, it's simple. There's an overload that takes and int which is the occurence of the instruction which you'd like to find.
```cs
Instruction opCode = Instruction.Create(OpCodes.Ldstr, "TheTrain");
Instruction toFind = Instruction.Create(OpCodes.Ldstr, "TheWord");
Target target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "FindMe",
    Instruction = opCode // you can also set it later
};
target.Index = p.FindInstruction(target, toFind, 2); // Sir, find the second occurence!
```

### Replacing instructions
In some cases it might be easier to just replace an instruction. At this point of development, it doesn't make much sense, but the features will come soon.
```cs
Instruction opCode = Instruction.Create(OpCodes.Ldstr, "I love kittens");
Target target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "ReplaceMe",
    Instruction = opCode,
    Index = 0
};
p.ReplaceInstruction(target);
```

### Removing instructions
Let's say you want to remove instructions... Well it's simple as this:
```cs
Target target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "RemoveMe",
    Indexes = new[]{0,1} // the indexes, you can also just use 'Index'
};
p.RemoveInstruction(target);
```

### Patching operands
Hmmm.... What if you find the console output offending? You can modify the Ldstr without even creating an instruction :)
```cs
Target target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "PrintAlot",
    Index = 0
};
p.PatchOperand(target, "PatchedOperand"); // pass the Target and a string to replace
```
or incase you need to modify an int:
```cs
p.PatchOperand(target, 1337);
```
It is also able to patch multiple operands in the same method by using int[] or string[].

### Returning true/false
If you want to overwrite the methodbody with a return true/false statement you can do this:
```cs
target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "VerifyMe"
};
p.WriteReturnBody(target, bool); // bool represents the return value
```

### Clearing methodbodies
If you just want to empty a methodbody, use this amigo:
```cs
target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "WriteLog"
};
p.WriteEmptyBody(target);
```

### Getting instructions from target
Simply do this if you want to get instructions of the Target object:
```cs
target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "WriteLog"
};
Instruction[] instructions = p.GetInstructions(target);
```

### Writing return bodies
If you want to overwrite the body with a return true/false do this:
```cs
target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "WriteLog"
};
p.WriteReturnBody(target, bool);
// bool is the return value, e.g. true will return true ;)
```
If you want to remove the body simply call this:
```cs
target = new Target()
{
    Namespace = "Test",
    Class = "Program",
    Method = "WriteLog"
};
p.WriteEmptyBody(target);
```

### Find methods
If you want to find a method, you can simply scan the whole file by 2 ways:
```cs
p.FindInstructionsByOperand(string[]);
// or p.FindInstructionsByOperand(int[]);
// string[] with all operands in the method, if there are multiple identical operands, make sure to have the same amount as in the method.

// or do this via opcodes:
p.FindInstructionsByOpcode(OpCode[]);
```
Both ways return an Target[] which contains all targets pointing to the findings.

#### Find instructions in methods or classes
If you want to find the instructions and you know the class and optionally the method you can let this method return a Target[] with the pathes and indexes.
```cs
p.FindInstructionsByOperand(Target,int[],bool);
// int[]: the operands
// bool: if true it will search for the operands once, it will delete the index if the index was found

// for opcodes:
p.FindInstructionsByOpcode(Target,int[],bool);
```
mbo
### Building calls
To build calls like "Console.WriteLine()" you can use this method:
```cs
p.BuildMemberRef(string, string, string, Patcher.MemberRefType);
/* 
 * string 1 -> namespace, e.g. "System"
 * string 2 -> class, e.g. "Console"
 * string 3 -> method, e.g. "WriteLine"
 * MemberRefType -> the reference type, e.g. Static
 */
```
MemberRefType is defined as follows:
```cs
public enum MemberRefType
{
    Static,
    Instance
}
```
Here is an IL example for Console.WriteLine:
```cs
Instruction.Create(OpCodes.Call, p.BuildMemberRef("System", "Console", "WriteLine", Patcher.MemberRefType.Static));
```

### Saving the patched assembly
If you want to safe the assembly under a different name use this:
```cs
patcher.Save(String); // filename here
```
Or if you want to replace the original file:
```cs
patcher.Save(bool); // if true it will create a backup first (filename.bak)
```