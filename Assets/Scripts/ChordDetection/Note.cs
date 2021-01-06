using UnityEngine;

public class Note
{
    public static string GetKeyName(int pianoKey)
    {
        return c_KeyNames[pianoKey % 12];
    }

    public static string GetKeyName(Note note)
    {
        return GetKeyName(note.PianoKey);
    }

    private static readonly string[] c_KeyNames = new string[]
        { "G#/Ab", "A", "A#/Bb", "B" , "C", "C#/Db", "D", "D#/Eb", "E", "F", "F#/Gb", "G" };

    public int PianoKey { get; private set; }
    public int ModKey => PianoKey % 12;
    public int Octave => Mathf.FloorToInt((PianoKey + 8) / 12f);
    public string Name => $"{PianoKey} [{c_KeyNames[ModKey]}{Octave}]";

    public Note(int pianoKey)
    {
        PianoKey = pianoKey;
    }

    public override string ToString()
    {
        return Name;
    }
}