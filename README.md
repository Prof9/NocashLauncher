NocashLauncher
==============


What
----
Here's a quick and shoddy tool I wrote to alleviate a rather specific problem I was having with No$gba, a popular GBA/NDSi emulator. If you're on Windows 10, Skype is open, and the debug font is not set to 'large', then closing the debugger version of No$gba will actually cause the process to hang until Skype is closed. This happens because No$gba makes the following system call:
```
SendMessageA(HWND_BROADCAST, WM_FONTCHANGE, 0, 0)
```
This syscall notifies all running programs that the system fonts have changed and waits until all programs have handled the message. But for whatever friggin' reason Skype blocks on this syscall. And this is a problem because No$gba does not save its breakpoint/opcode/value input history, recent files and some other changed settings until after this syscall finishes.

Because Martin Korth is a goddamn wizard and the No$gba executable is compressed/encrypted, patching the executable is not a very easy thing to do. So instead, I wrote a thin launcher/trainer that starts No$gba and then patches the executable in memory after it has finished loading. Essentially, it just changes the SendMessageA syscall to a PostMessageA syscall which is basically the same thing but doesn't wait for other programs to handle it. What the fuck Microsoft.

So this tool is just a compatibility layer between Windows 10/Skype and No$gba. That's all it does for now, but like the framework is there so if needed other patches could be added. I guess. It's also an exercise in how not to write a proper console application.


Compatibility
-------------
NocashLauncher currently supports the following versions of the No$gba Windows debugger:
```
v2.6a
v2.7c - v2.7d
v2.8 - v2.8c
```


Usage
-----
There are three ways to tell NocashLauncher which executable to use. They are listed below in the order that they are used. All three are tried until No$gba is launched.

1. Pass the (absolute or relative) path of the executable as the first argument. For example:
```
NocashLauncher.exe "No$gba v2.6a.exe"
```
2. Rename NocashLauncher.exe to the name of No$gba executable without extension and append "Launch(er).exe", with optional '-', '@', '_' or '.' in between. Yep, it's that simple. Here are some examples that all launch No$gba.exe:
```
No$gba Launcher.exe
No$gbaLaunch.exe
No$gba - Launch.exe
No$gba_Launcher.exe
No$gba @Launch.exe
No$gba.launch.exe
No$gbaLauncher.test.old.exe
```
Any whitespace around the name of the No$gba executable and Launch(er).exe are ignored.

3. Don't provide anything. NocashLauncher will try to start the executable named NO$GBA.EXE.

For methods 2 and 3, NocashLauncher will also search any folder named 'NO$GBA', 'NOCASH' or 'bin' for the specified No$gba executable. If you want to override this, use method 1.

Any command line arguments (except the No$gba executable path for method 1) are also passed to No$gba, so you can drag-and-drop ROMs on top of NocashLauncher no problem!


Contact
-------
The GitHub repository for this program is at https://github.com/Prof9/NocashLauncher -- go there if you want to report any bugs.

You can also send me tweets: https://twitter.com/Prof9


Credits
-------
None of you suckers did anything, this was all me baby!

Mad props to Martin Korth for his awesome emulator.


License
-------
This is free and unencumbered software released into the public domain. See `License.txt` for more details. No$gba is (c) Martin Korth.