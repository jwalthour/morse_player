import argparse
import winsound
import sys
import time

class MorsePlayer:
	f = 880
	wpm = 10 # The number of "PARIS" transmissions per minute
	_DITS_PER_WORD = 50 # The representative word ("PARIS") takes 50 dots
	
	_MORSE_KEY = {
		'A':['.', '-'],
		'B':['-', '.', '.', '.'],
		'C':['-', '.', '-', '.'],
		'D':['-', '.', '.'],
		'E':['.'],
		'F':['.', '.', '-', '.'],
		'G':['-', '-', '.'],
		'H':['.', '.', '.', '.'],
		'I':['.', '.'],
		'J':['.', '-', '-', '-'],
		'K':['-', '.', '-'],
		'L':['.', '-', '.', '.'],
		'M':['-', '-'],
		'N':['-', '.'],
		'O':['-', '-', '-'],
		'P':['.', '-', '-', '.'],
		'Q':['-', '-', '.', '-'],
		'R':['.', '-', '.'],
		'S':['.', '.', '.'],
		'T':['-'],
		'U':['.', '.', '-'],
		'V':['.', '.', '.', '-'],
		'W':['.', '-', '-'],
		'X':['-', '.', '.', '-'],
		'Y':['-', '.', '-', '-'],
		'Z':['-', '-', '.', '.']
	}
	
	def playMorse(self, char_string):
		for c in char_string:
			self.playChar(c)
		
	def playChar(self, char):
		if char.upper() in self._MORSE_KEY:
			sys.stdout.write(char + '    ')
			seq = self._MORSE_KEY[char.upper()]
			dit_t = self._dot_duration_s()
			for symbol in seq:
				sys.stdout.write(symbol)
				if symbol == '.':
					self._dit(dit_t)
				elif symbol == '-':
					self._dah(dit_t)
			sys.stdout.write("\n")
			time.sleep(dit_t * 3)
			
	def _dot_duration_s(self): # seconds per dot
		return (60.0 / self.wpm) / self._DITS_PER_WORD 
	def _dit(self, dit_t):
		winsound.Beep(self.f, int(dit_t * 1000.0))
		time.sleep(dit_t)
	def _dah(self, dit_t):
		dah_t = 3 * dit_t
		winsound.Beep(self.f, int(dah_t * 1000.0))
		time.sleep(dit_t)


if __name__ == '__main__':
	mp = MorsePlayer()
	# mp.playMorse('Hello')

	in_data = sys.stdin.read()
	while(in_data):
		mp.playMorse(in_data)
		in_data = sys.stdin.read()
