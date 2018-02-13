Goal : make believe that target is not debugged
	Each time IsDebuggerPresent API is called it return false, even a debugger is attached
	Notice : only IsDebuggerPresent API is changed, there are many other ways to detect if a process is debugged or not

--------------------------------------------------------------------
How to use :
1) Hook target with WinApiOverride
2) load the overriding dll with WinApiOverride
That's all