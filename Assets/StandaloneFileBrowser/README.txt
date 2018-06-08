6/2018 Stauffer added

StandaloneFileBrowser (SFB)
https://github.com/gkngkc/UnityStandaloneFileBrowser

**.NET 2.0 compatibility level Setting **
This project uses Ookii.dialogs, which uses some .NET 2.0 stuff.
When ran debugger in VS, was getting error about ookii.dialogs ref’ing system.drawing version 2.0.0.0.
Looking on SFB github, says it needs .NET 2.0 compatibility, and onissues page, it said the unity settings (https://github.com/gkngkc/UnityStandaloneFileBrowser/issues/17)

Solution:

REQUIRES Unity “Api Compatibility Level” of “.NET 2.0” (and not ".NET 2.0 Subset")
In Unity player settings (Edit | Project Settings | Player)
