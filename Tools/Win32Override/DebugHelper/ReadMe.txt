Goal : make believe that target is not debugged
	Each time IsDebuggerPresent API is called it return false, even a debugger is attached
	Notice : only IsDebuggerPresent API is changed, there are many other ways to detect if a process is debugged or not

--------------------------------------------------------------------
How to use :
1) Load HookedOnlyDebugHelper.txt as inclusion list
2) Load the overriding dll with WinApiOverride
3) Hook target with WinApiOverride with attach to all new processes (with corresponding filter).
