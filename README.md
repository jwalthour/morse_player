# morse_player
A player of morse code.

# Windows installation instructions
1. Click [here](https://github.com/jwalthour/morse_player/archive/master.zip) to download the morse_player files.  For the rest of the instructions we'll assume you put it in your Downloads directory.  If you're familiar with Git, clone this repository instead.  (then feel free to contribute and submit a pull request!)
* Click [here](https://repo.continuum.io/archive/Anaconda2-4.2.0-Windows-x86_64.exe) to download Anaconda.  (If that one doesn't work, go [here](https://www.continuum.io/downloads#windows) to select a different installer.  Wait for the download to complete, then run it to install Anaconda.  When it asks about "adding python to your path", say yes.  You may now find a python in your path.
* Go to your Downloads folder and unzip the file from step 1.  Enter that folder with File Explorer.  You should see a handful of files, including LICENSE.TXT, morse_player.py, and this file (README.md).
* Find the "address bar".  This is the part of File Explorer that shows which folder you're in - it should say something like `This Pc > Downloads > morse_player-master`.  Click on the address bar - it should be ready for you to type something into it.  (not to be confused with the search box).  Type "cmd" and press enter.  A black console window should appear, presenting a prompt like `C:\Users\YourName\Downloads\morse_player-master\>`.
* Type `python --version` and press enter.  It should say something about Anaconda.
* Type `python morse_player.py < much_ado_wrt_zed.txt`.  Shakespeare's "Much Ado About Nothing" should begin to play, in morse code.
* To adjust the playback speed, try adding `-w 5`, to change the speed to 5 words per minute.  `python morse_player.py -w 5 < much_ado_wrt_zed.txt`
* To adjust the volume, try adding `-v 20`, to change the volume to 20%.  `python morse_player.py -w 5 -v 20 < much_ado_wrt_zed.txt`
* If you find the tone grating, try adding `-f 220`, to change the pitch to 220Hz.  `python morse_player.py -w 5 -v 20 -f 220 < much_ado_wrt_zed.txt`
* If you wish to play a different text file, replace `much_ado_wrt_zed.txt` with another filename.  The file `spacey_text.txt` has its letters spaced out, which you may find more useful for learning.
