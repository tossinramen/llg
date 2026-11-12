"""Composes the game's peaceful lofi background track from scratch.

Run:  python Tools/audio/generate_music.py
Output: Assets/Resources/Audio/lofi_village.wav  (a seamless loop, about 80 seconds)

Everything is synthesised here (electric piano, bass, soft drums, vinyl crackle), so the music
is original to this project and free to use however you like.
Change SEED for a different melody, or the CHORDS list for a different mood.
"""
import os
import wave

import numpy as np
from scipy.signal import butter, lfilter

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "Resources", "Audio", "lofi_village.wav")

SR = 22050
BPM = 72
BEAT = 60.0 / BPM
BAR = BEAT * 4
SEED = 4
BARS = 24

# ii - V - I - vi in C with jazzy extensions (MIDI note numbers)
CHORDS = [
    [50, 53, 57, 60, 64],   # Dm9
    [43, 53, 59, 64, 69],   # G13
    [48, 52, 55, 59, 62],   # Cmaj9
    [45, 48, 52, 55, 59],   # Am9
]
BASS = [38, 31, 36, 33]
PENTATONIC = [72, 74, 76, 79, 81, 84]   # C major pentatonic, high octave

rng = np.random.default_rng(SEED)
length = int(BAR * BARS * SR)
tail = int(3 * SR)
mix = np.zeros(length + tail)


def hz(midi):
    return 440.0 * 2 ** ((midi - 69) / 12.0)


def add(signal, start_sec, gain=1.0):
    start = int(start_sec * SR)
    end = min(len(mix), start + len(signal))
    mix[start:end] += signal[: end - start] * gain


def lowpass(x, cutoff, order=2):
    b, a = butter(order, cutoff / (SR / 2), btype="low")
    return lfilter(b, a, x)


def highpass(x, cutoff, order=2):
    b, a = butter(order, cutoff / (SR / 2), btype="high")
    return lfilter(b, a, x)


def epiano(midi, dur, vel):
    """Soft Rhodes-like tone: a few harmonics, a gentle tremolo, a long decay."""
    t = np.arange(int((dur + 1.5) * SR)) / SR
    f = hz(midi) * (1 + rng.normal(0, 0.0008))
    tone = (np.sin(2 * np.pi * f * t) + 0.35 * np.sin(2 * np.pi * 2 * f * t) * np.exp(-t * 3)
            + 0.12 * np.sin(2 * np.pi * 3 * f * t) * np.exp(-t * 6))
    env = np.minimum(t / 0.008, 1) * np.exp(-t * 1.1)
    release = np.clip(1 - (t - dur) / 1.2, 0, 1)
    tremolo = 1 + 0.08 * np.sin(2 * np.pi * 4.5 * t)
    return tone * env * release * tremolo * vel


def bell(midi, vel):
    t = np.arange(int(2.0 * SR)) / SR
    f = hz(midi)
    tone = np.sin(2 * np.pi * f * t) + 0.25 * np.sin(2 * np.pi * 2.01 * f * t) * np.exp(-t * 4)
    return tone * np.minimum(t / 0.01, 1) * np.exp(-t * 2.2) * vel


def bass(midi, dur):
    t = np.arange(int((dur + 0.2) * SR)) / SR
    f = hz(midi)
    tone = np.sin(2 * np.pi * f * t) + 0.25 * np.sin(2 * np.pi * 2 * f * t)
    env = np.minimum(t / 0.01, 1) * np.exp(-t * 1.6) * np.clip(1 - (t - dur) / 0.2, 0, 1)
    return tone * env


def kick():
    t = np.arange(int(0.45 * SR)) / SR
    freq = 48 + 70 * np.exp(-t * 25)
    return np.sin(2 * np.pi * np.cumsum(freq) / SR) * np.exp(-t * 7)


def snare():
    t = np.arange(int(0.3 * SR)) / SR
    noise = lowpass(highpass(rng.normal(0, 1, len(t)), 900), 4500)
    body = np.sin(2 * np.pi * 185 * t) * np.exp(-t * 25)
    return (noise * np.exp(-t * 14) * 0.6 + body * 0.5)


def hat():
    t = np.arange(int(0.06 * SR)) / SR
    return highpass(rng.normal(0, 1, len(t)), 7000) * np.exp(-t * 70)


KICK, SNARE = kick(), snare()
duck = np.ones(len(mix))     # chords dip slightly when the kick hits (the "pumping" lofi feel)

for bar in range(BARS):
    t0 = bar * BAR
    chord = CHORDS[bar % 4]
    # piano: strummed on beat 1, a softer re-hit on the "and" of beat 3
    for i, note in enumerate(chord):
        add(epiano(note, BAR * 0.55, 0.16), t0 + i * 0.018)
        add(epiano(note + (12 if i == len(chord) - 1 else 0), BAR * 0.3, 0.07), t0 + BEAT * 2.5 + i * 0.012)
    add(bass(BASS[bar % 4], BEAT * 1.4), t0, 0.55)
    add(bass(BASS[bar % 4] + 7, BEAT * 0.8), t0 + BEAT * 2.5, 0.35)

    if bar >= 2:   # drums come in after two bars
        for beat in (0, 2.5):
            add(KICK, t0 + beat * BEAT, 0.75)
            s = int((t0 + beat * BEAT) * SR)
            duck[s:s + int(0.25 * SR)] *= np.linspace(0.72, 1, int(0.25 * SR))[: len(duck[s:s + int(0.25 * SR)])]
        for beat in (1, 3):
            add(SNARE, t0 + beat * BEAT + 0.012, 0.32)
        for eighth in range(8):
            swing = 0.06 if eighth % 2 else 0
            add(hat(), t0 + (eighth * 0.5 + swing) * BEAT, 0.07 if eighth % 2 else 0.11)

    # melody in bars 8-23: short pentatonic phrases with rests, repeated with variation
    if bar >= 8:
        phrase_rng = np.random.default_rng(SEED * 100 + (bar % 8))
        for step in range(8):
            if phrase_rng.random() < 0.38:
                note = PENTATONIC[phrase_rng.integers(len(PENTATONIC))]
                if bar >= 16 and phrase_rng.random() < 0.3:
                    note += 2 if note + 2 in PENTATONIC else 0
                add(bell(note, 0.09), t0 + step * BEAT * 0.5 + (0.05 if step % 2 else 0))

# warmth: roll off the highs, then add vinyl hiss and crackle
mix *= duck
mix = lowpass(mix, 3800)
hiss = lowpass(rng.normal(0, 0.004, len(mix)), 5000)
crackle = np.zeros(len(mix))
pops = rng.integers(0, len(mix), size=int(len(mix) / SR * 9))
crackle[pops] = rng.uniform(-0.25, 0.25, size=len(pops))
crackle = lowpass(crackle, 2500)
mix += hiss + crackle

# make it loop seamlessly: fold the ringing tail back onto the start
loop = mix[:length].copy()
loop[:tail] += mix[length:length + tail]
loop = loop / np.max(np.abs(loop)) * 0.7

os.makedirs(os.path.dirname(OUT), exist_ok=True)
with wave.open(OUT, "wb") as f:
    f.setnchannels(1)
    f.setsampwidth(2)
    f.setframerate(SR)
    f.writeframes((loop * 32767).astype(np.int16).tobytes())
print("Wrote", OUT, "-", round(length / SR, 1), "seconds")
