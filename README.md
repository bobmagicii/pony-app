## WTF

This is an app that makes a pony walk across my taskbar. I got these
GIFs from the Desktop Ponies application (CC-BY-NC-SA-3) but the app
was too... busy... for what I wanted. I also didn't want ponies all
over my bloody desktop. I just want the one to traverse across the
taskbar.

 * Video: http://www.youtube.com/watch?v=fdITEc4gmpo&hd=1
 * Downloads: http://catch404.net/files/ponyapp/
 * Questions, Comments, Suggestions: Email bob at catch404 dot net

Eventually I want to make her do work for me, like if I click her and
just type "google whatever" she starts up chrome and does it.

## Application Features

 * Pony
 
## Feature Detail

 * She wanders across the taskbar as she wants to.
 * You can right click and tell her to stand off to the left or right
   side of the screen. She'll trot over there and be pretty off in
   the corner.
 * If you pet her too much (double click) she becomes clingy...

## TODO

 * Find someone who can remake the same GIFs in high detail instead of
   lol pixel art.
 * Some ponies need more animations... Like Rarity has no sleep
   animation even though Rainbow Dash and some others do. Its just a
   pile of GIF files from that other project *shrug*
 * Figure out why WpfAnimatedGif has what appear to be issues loading
   GIF files from a directory. Only got it working with embedded
   resources atm.
 * After that, config files to add more ponies.
 
## Technical Blah Blah Blah

This is a Visual C# Project. I don't know jack about what I am doing.
Therefore I COMMITED ALL THE THINGS to the Githubs including that
solution and options file and whatever else is in this cluster folder.
If that was wrong oh well. I don't even know WTF a solution is. Where
I come from the solution is `gcc app.c -o app` and you're done. So.

I have never written a line of C# in my life until now. This could be
the worst application ever written in that language I have no clue.
This is my learning introduction project. My knowledge of the language
comes purely from reading what Visual Studio bitches about when I do
something dumb, and my experience in real languages like straight C.

rimshot.wav

That joke aside, it actually is an interesting language. So far I am
enjoying myself... but I'm also getting a pony out of it as a reward
so that might just be a novelty feeling.

The only reason I went with C# was because GTK still does not support
RGBA colormaps on Windows. Durr. This would have been a PHP-GTK app
if not for that. Windows WPF was the only thing that would render a
transparent window without me having to do 400 lines of code so here
it is.

## Legal Blah Blah Blah

Not affiliated with Hasbro... duh.

I did not make the artwork. They are animated GIF's from the project
on http://desktopponies.com/ - And they are awesome.

My code and all that is 2-Clause BSD. That should be liberal enough
to comply with the project I lifted the GIFs from yeah?

## Licence

Copyright (c) 2012 All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

 * Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.

 * Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the
   distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
