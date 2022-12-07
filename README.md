# MDPatcher 1.0

MDPatcher patches SEGA Saturn games in order to make them compatible with SEGA Megadrive/Genesis controllers.

It is a little known fact but the SEGA Saturn was designed to also accept Megadrive controllers - but no official adapter was ever released.

It is possible to connect one to a Saturn by making a custom adapter using the following pin assignments (from the SMPC manual):

| Saturn | MD/Genesis |
|--------|------------|
| 1 VCC  | 5          |
| 2 D1   | 2          |
| 3 D0   | 1          |
| 4 S0   | 7          |
| 5 S1   | 9          |
| 6 5V   | 6          |
| 7 D3   | 4          |
| 8 D2   | 3          |
| 9 GND  | 8          |

Not much Saturn software works with the Megadrive controller out of the box, but luckily the Saturn BIOS does and most software developed by using the SGL library appears to, as well. 
The rule of thumb is to first test whether the game works without any modification, and if it does not, then attempt to patch it.
MDPatcher automates the process but it does not work with all games; it works by finding known code in the games and by patching it tricking the games into believing that a normal Saturn controller is being used.
There is still a lot of work to do to add more code patterns to be detected by this program.

(Incomplete) list of games that can be successfully patched:
- Astal
- BreakThru
- Bug TOO!
- Burning Rangers (*)
- Duke Nukem 3D
- Manx TT Superbike
- Mega Man 8
- Mizubaku
- Pandemonium
- Sonic 3D Blast / Flickies' Island
- Sonic Jam
- Sonic R

(*) Burning Rangers is unplayable, though, as using buttons not available on the Megadrive controller is necessary.

***************
To run MDPatcher you will need to have the .NET Runtime installed if you are on Windows, or Mono on other operating systems.

The program accepts ISO images and also images in raw .bin format in mode1 and/or mode2 formats.

To patch a game image, simply open the disc image file. MDPatcher will do its work and report the result. If it can't patch your game, you can let me know by writing me an email and I will look into it.
***************

Giuseppe Gatta/nextvolume

Web: http://unhaut.x10host.com/mdpatcher

Email: tails92@gmail.com
