using System.Collections.Generic;
using UnityEngine;

public class ChordPlayer : MonoBehaviour
{
    private int m_AgentCount;
    private List<AgentTraining> m_Agents;

    [SerializeField]
    private float m_Duration = 0.48f;

    private AudioSource[] m_Sources;
    // Clips by instrument & piano key.
    private Dictionary<string, Dictionary<int, AudioClip>> m_InstrumentClips;
    private List<AudioClip> m_DrumLoopClips;
    // Excluding Bass.
    private readonly string[] m_Instruments = new string[]
    {
        "StringEnsemble", "GuitarAcoustic", "GuitarDistorted", "GuitarDrive", // 20 - 44
        "MatrixBrass", "Piano", "PianoElectric", // 25 - 49
        "Trombone", "Sax" // 25 - 37
    };

    private void Awake()
    {
        LoadAudioClips();
        m_Sources = GetComponents<AudioSource>();
    }


    /// <summary>
    /// Registers an <see cref="AgentTraining"/>.
    /// <param name="agent"><see cref="AgentTraining"/>.</param>
    /// </summary>
    public void RegisterAgent(AgentTraining agent)
    {
        m_Agents ??= new List<AgentTraining>();
        m_Agents.Add(agent);
    }

    /// <summary>
    /// Called by each <see cref="AgentTraining"/> when it is ready for the 
    /// next observation. Plays next chord when all registered agents are ready.
    /// </summary>
    public void OnAgentReady()
    {
        if (++m_AgentCount == m_Agents.Count)
        {
            // Randomize.
            var instrument = m_Instruments[Random.Range(0, m_Instruments.Length)];
            int rootKey = GetRandomRootKey(instrument);
            int type = Random.Range(0, 11);
            Chord chord = new Chord(type, rootKey).GetRandomInversion();

            // Notify agents.
            foreach (var agent in m_Agents)
            {
                agent.OnNextChord(chord);
            }
            m_AgentCount = 0;

            int iSrc = 0;
            // Play chord.
            foreach (int pianoKey in chord.PianoKeys)
            {
                PlayInstrument(m_Sources[iSrc], 0.4f, instrument, pianoKey);
                iSrc++;
            }
            // Play bass (optional).
            if (Random.value < 0.5f)
            {
                int bassKey = rootKey % 12;
                bassKey += bassKey < 8 ? 12 : 0;
                PlayInstrument(m_Sources[iSrc], 0.7f, "Bass", bassKey);
                iSrc++;
            }
            // Play drums (optional).
            if (Random.value < 0.5f)
            {
                PlayRandomDrumLoop(m_Sources[iSrc], 0.5f);
            }
        }
    }

    private int GetRandomRootKey(string instrument)
    {
        int min = 25, max = 49;

        switch (instrument)
        {
            case "Sax":
            case "Trombone":
                min = 25;
                max = 37;
                break;

            case "StringEnsemble":
            case "GuitarAcoustic":
            case "GuitarDistorted":
            case "GuitarDrive":
                min = 20;
                max = 44;
                break;
        }

        return Random.Range(min, max);
    }

    private void PlayInstrument(AudioSource src, float volume, string instrument, int pianoKey)
    {
        if (HasClip(instrument, pianoKey, out AudioClip clip))
        {
            src.clip = clip;
            src.volume = volume;
            PlayClip(src);
        }
    }

    private void PlayRandomDrumLoop(AudioSource src, float volume)
    {
        src.clip = m_DrumLoopClips[Random.Range(0, m_DrumLoopClips.Count)];
        src.volume = volume;
        PlayClip(src);
    }

    private void PlayClip(AudioSource src)
    {
        src.time = RandomStartTime(src);
        src.Play();
        src.SetScheduledEndTime(AudioSettings.dspTime + m_Duration);
    }

    private float RandomStartTime(AudioSource src)
    {
        return Random.value * Mathf.Max(0, src.clip.length - m_Duration - 0.2f);
    }



    private void LoadAudioClips()
    {
        m_InstrumentClips = new Dictionary<string, Dictionary<int, AudioClip>>();
        var clips = Resources.LoadAll("Audio/Instruments", typeof(AudioClip));

        foreach (var clip in clips)
        {
            var name = clip.name.Split('_');

            string instrument = name[1];
            if (!m_InstrumentClips.TryGetValue(instrument, out Dictionary<int, AudioClip> byKey))
            {
                byKey = new Dictionary<int, AudioClip>();
                m_InstrumentClips.Add(instrument, byKey);
            }

            int key = short.Parse(name[0]);
            byKey.Add(key, (AudioClip)clip);
        }


        m_DrumLoopClips = new List<AudioClip>();
        clips = Resources.LoadAll("Audio/DrumLoops", typeof(AudioClip));

        foreach (var clip in clips)
        {
            m_DrumLoopClips.Add((AudioClip)clip);
        }
    }

    private bool HasClip(string instrument, int key, out AudioClip clip)
    {
        if (m_InstrumentClips.TryGetValue(instrument, out Dictionary<int, AudioClip> byKey))
        {
            return byKey.TryGetValue(key, out clip);
        }
        throw new KeyNotFoundException("Instrument not found " + name);
    }
}
