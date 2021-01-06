using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum ChordType : int
{
    Major = 0,
    Minor = 1,
    Diminished = 2,
    Augmented = 3,
    Suspended2 = 4,
    Suspended4 = 5,
    MajorSixth = 6,
    MinorSixth = 7,
    MajorSeventh = 8,
    MinorSeventh = 9,
    DominantSeventh = 10
}

public class Chord
{
    private static readonly Dictionary<ChordType, int[]> 
           c_Patterns = new Dictionary<ChordType, int[]>
    {
        { ChordType.Major,           new[] { 0, 4, 7 } },
        { ChordType.Minor,           new[] { 0, 3, 7 } },
        { ChordType.Diminished,      new[] { 0, 3, 6 } },
        { ChordType.Augmented,       new[] { 0, 4, 8 } },
        { ChordType.Suspended2,      new[] { 0, 2, 7 } },
        { ChordType.Suspended4,      new[] { 0, 5, 7 } },
        { ChordType.MajorSixth,      new[] { 0, 4, 7, 9 } },
        { ChordType.MinorSixth  ,    new[] { 0, 3, 7, 9 } },
        { ChordType.MajorSeventh,    new[] { 0, 4, 7, 11 } },
        { ChordType.MinorSeventh,    new[] { 0, 3, 7, 10 } },
        { ChordType.DominantSeventh, new[] { 0, 4, 7, 10 } }
    };

    public ChordType Type { get; private set; }
    public int Key => RootNote.ModKey;
    public Note RootNote => m_Notes[(m_Notes.Count - m_Inversion) % m_Notes.Count];
    public IEnumerable<int> PianoKeys => m_Notes.Select(note => note.PianoKey);

    private readonly int m_Inversion;
    private readonly List<Note> m_Notes;

    public Chord(int type, int root) : this((ChordType)type, root) { }

    public Chord(ChordType type, int root)
    {
        Type = type;
        int n = c_Patterns[type].Length;
        m_Notes = new List<Note>(n);

        for (int i = 0; i < n; i++)
        {
            m_Notes.Add(new Note(root + c_Patterns[type][i]));
        }
    }

    public Chord(ChordType type, int inversion, IEnumerable<Note> notes)
    {
        Type = type;
        m_Inversion = inversion;
        m_Notes = new List<Note>(notes);
    }

    public IEnumerable<Chord> Inversions()
    {
        var notes = new List<Note>(m_Notes);
        for (int i = 0, n = notes.Count - 1; i < n; i++)
        {
            notes.Add(new Note(notes[0].PianoKey + 12));
            notes.RemoveAt(0);
            yield return new Chord(Type, i + 1, notes);
        }
    }

    public Chord GetInversion(int i)
    {
        return i == 0 ? this : Inversions().ToArray()[i - 1];
    }

    public Chord GetRandomInversion()
    {
        return GetInversion(Random.Range(0, m_Notes.Count));
    }

    // Chords are considered equal if their notes match,
    // regardless of octave and inversion.
    public bool Equals(Chord other)
    {
        var a = PianoKeys.Select(k => k % 12).OrderBy(k => k);
        var b = other.PianoKeys.Select(k => k % 12).OrderBy(k => k);
        return a.SequenceEqual(b);
    }

    public override string ToString()
    {
        var noteNames = m_Notes.Select(note => note.Name).ToArray();
        string inversion = m_Inversion > 0 ? $"Inversion {m_Inversion}" : "Root";
        return $"{Note.GetKeyName(Key)} [{Key}] {Type} {inversion} ({string.Join(", ", noteNames)})";
    }
}