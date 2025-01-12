# FF8_AngeloSearchHelper
App that helps with the button counting on the PC version of Final Fantasy 8 when doing manual Angelo Searching. It is based off the work done and documented by greater minds (https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/25194) regarding Angelo Search. Read their FAQ on how Angelo Search works, and then return here to make counting easier.

NOTE: This app has keyboard hook builtin and may be seen by antivirus software as a keylogger. 

How my app works: 
1. Change the options in Options.xml to specify which keys are the Increment key and Reset key. The Increment Key is the one used to skip turns whilst in battle, and this is the important key.
2. The Reset key does not have to be a key mapped to an action within the game, but I tested it with the Toggle Display button and it works great
3. Start your Angelo search quest, as per the FAQ
4. Click the Enable Hook button
5. Wait, until Angelo has found something, and at the moment the game says what was found, click the Reset button to start the timer, and then use the list in my app to select the item that was received
6. Depending on the item chosen, the list will be shortened, and the Possible Clicks will display 1 or more numbers.
7. Click the Incrememnt key until the "Game Value" reads the lowest value, then stop clicking. I recommend stopping 1 or 2 short, until you get the hang of it.
8. If Angelo appears, repeat from step 5 (click Reset, select item, start Incrementing to lowest value)
9. If Angelo does not appear and there are higher numbers in the Possible Clicks box, tap the Increment button until the Game Value matches
10. Repeat step 9 if Angelo still hasn't appeared.

The list of items will reduce down to 6 (or 5 in some cases) while the app is trying to figure out which row you are on.

To restart the entire process (for example, if you had to step away and don't know what item was received), click the Reset button and start at point 5.
Note: Holding down the Increment button will increment the count in the app very quickly, but the game will only register it as a single click.

Version history: 
-----------------------------------------------
v0.1
2025-01-12
----------
* Initial release
* Expect bugs
-----------------------------------------------