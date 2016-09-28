import argparse
import wave
import array
import struct
import math
import winsound
import io
import sys
import time

# Memberwise multiply of two equal-size arrays
def mix_signals(a,b):
	return [a_s * b_s for a_s,b_s in zip(a, b)]

def gen_tone(sample_indices, freq, amplitude, samples_per_sec):
	return [int(amplitude * math.sin((2 * math.pi)*i*(float(freq)/samples_per_sec))) for i in sample_indices]

# Makes beep sample sets as wav files
class Beep:
	SAMPLES_PER_SEC = 44100.0
	NUM_CHANNELS = 1
	SAMPLE_BYTE_WIDTH = 2
	RAMP_FRACTION = 0.05 # percent each end used on ramping
	def __init__(self, f, t, v, t_pad=0):
		buffer = io.BytesIO('')
		note_output = wave.open(buffer, 'wb')
		note_output.setparams((self.NUM_CHANNELS, self.SAMPLE_BYTE_WIDTH, int(self.SAMPLES_PER_SEC), 0, 'NONE', 'not compressed'))
		
		i_range = range(0, int(self.SAMPLES_PER_SEC * (t + t_pad)))
		ramp_samples = int(self.RAMP_FRACTION * t * self.SAMPLES_PER_SEC)
		plateau_samples = int(t * self.SAMPLES_PER_SEC) - 2 * ramp_samples;
		pad_samples = int(t_pad * self.SAMPLES_PER_SEC)
		amplitude = [(x / float(ramp_samples)     ) for x in range(0, ramp_samples)] \
                  + [1 for x in range(0, plateau_samples)] \
                  + [(1.0 - (x / float(ramp_samples))) for x in range(0, ramp_samples)] \
				  + [0 for x in range(0, pad_samples)]
		tone = gen_tone(i_range, f, v, self.SAMPLES_PER_SEC)
		note = mix_signals(amplitude, tone)
		packed_values = array.array('h', [int(s) for s in note])
		note_output.writeframes(packed_values.tostring())
		self._buffer = buffer.getvalue()
	def play(self):
		winsound.PlaySound(self._buffer, winsound.SND_MEMORY)

		
class MorsePlayer:
	f = 880
	v = 30000
	wpm = 5 # The number of "PARIS" transmissions per minute
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
		
	def playChar(self, char, dit=None, dah=None):
		if char.upper() in self._MORSE_KEY:
			sys.stdout.write(char + '    ')
			seq = self._MORSE_KEY[char.upper()]
			dit_t = self._dot_duration_s()
			if dit == None:
				dit = Beep(self.f, dit_t, self.v, dit_t)
			if dah == None:
				dah = Beep(self.f, dit_t * 3, self.v,dit_t)
			for symbol in seq:
				sys.stdout.write(symbol)
				if symbol == '.':
					dit.play()
				elif symbol == '-':
					dah.play()
			sys.stdout.write("\n")
			time.sleep(dit_t * 3)
			
	def _dot_duration_s(self): # seconds per dot
		return (60.0 / self.wpm) / self._DITS_PER_WORD 

if __name__ == '__main__':
	parser = argparse.ArgumentParser()
	parser.add_argument("-w", "--wpm", default=5, type=float, help="words per minute at which to play morse code")
	parser.add_argument("-f", "--freq", default=880, type=float, help="frequency of the tone to play")
	parser.add_argument("-v", "--vol", default=30000, type=float, help="volume of the tone to play")
	args = parser.parse_args()

	mp = MorsePlayer()
	# print("F was %f, becomes %f"%(mp.f, args.freq))
	mp.f = args.freq
	# print("V was %f, becomes %f"%(mp.v, args.vol))
	mp.v = args.vol
	# print("WPM was %f, becomes %f"%(mp.wpm, args.wpm))
	mp.wpm = args.wpm
	# mp.playMorse('Hello')
	# b = Beep(880, 5)
	# print('playing')
	# b.play()
	# print('done')
	
	in_data = sys.stdin.read()
	while(in_data):
		mp.playMorse(in_data)
		in_data = sys.stdin.read()
