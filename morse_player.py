import argparse
import wave
import array
import struct
import math
import winsound
import io
import sys

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
    _tone_f = 880
    _tone_vol = 30000
    _wpm = 5 # The number of "PARIS" transmissions per minute
    _DITS_PER_WORD = 50 # The representative word ("PARIS") takes 50 dots
    
    _MORSE_KEY = {
        ' ':[' '],
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
    
    def setFreq(self, freq):
        if(freq > 0):
            self._tone_f = freq
            self._clearTones()
        
    def setVol(self, vol):
        if(vol > 0 and vol <= 100):
            self._tone_vol = 32768.0 * (vol / 100.0)
            self._clearTones()
    
    def setWpm(self, wpm):
        if(wpm > 0):
            self._wpm = wpm
            self._clearTones()

    def playMorse(self, char_string):
        for c in char_string:
            self.playChar(c)
        
    def playChar(self, char):
        self._genTonesIfNeeded();
        if char.upper() in self._MORSE_KEY:
            sys.stdout.write(char + '    ')
            seq = self._MORSE_KEY[char.upper()]
            for symbol in seq:
                sys.stdout.write(symbol)
                if symbol == '.':
                    self._dit.play()
                elif symbol == '-':
                    self._dah.play()
                elif symbol == ' ':
                    self._word_gap.play()
            sys.stdout.write("\n")
            self._interchar_gap.play()
            
    def _dotDurationS(self): # seconds per dot
        return (60.0 / self._wpm) / self._DITS_PER_WORD 
    
    def _clearTones(self): # delete locally stored tones
        self._dit = None
        self._dah = None
        self._interchar_gap = None
        self._word_gap = None
    
    def _genTonesIfNeeded(self):
        if self._dit == None or self._dah == None or self._interchar_gap == None or self._word_gap == None:
            self._genTones()    
    
    def _genTones(self): # regenerate locally stored tones
        dit_t = self._dotDurationS()
        dah_t = 3 * dit_t
        self._dit = Beep(self._tone_f, dit_t, self._tone_vol, dit_t)
        self._dah = Beep(self._tone_f, dit_t * 3, self._tone_vol,dit_t)
        # Silence between characters
        self._interchar_gap = Beep(self._tone_f, 0, self._tone_vol,dah_t)
        # Played as a silent dit, but remember there are two interchar gaps,
        # so this winds up being 7 dits of silence between words.
        self._word_gap = Beep(self._tone_f, 0, self._tone_vol,dit_t)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument("-w", "--wpm", default=5, type=float, help="words per minute at which to play morse code (default 5)")
    parser.add_argument("-f", "--freq", default=880, type=float, help="frequency of the tone to play, in Hz (default 880)")
    parser.add_argument("-v", "--vol", default=50, type=float, help="volume of the tone to play, in percent (default 50)")
    args = parser.parse_args()

    mp = MorsePlayer()
    mp.setFreq(args.freq)
    mp.setVol(args.vol)
    mp.setWpm(args.wpm)
    
    in_data = sys.stdin.read()
    while(in_data):
        mp.playMorse(in_data)
        in_data = sys.stdin.read()
