using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using System.Collections;

public class AgentTraining : AudioAgent
{
    // Number of decisions & actions per episode,
    // independent of the audio buffer's length.
    [SerializeField]
    private int m_EpisodeLength = 100;
    private int m_DecisionCount;
    private StatsRecorder m_Stats;

    [SerializeField, Range(0f, 1f)]
    private float m_PauseDuration;
    private const float c_EpisodeDelay = 0.25f;

    private ChordPlayer m_ChordPlayer;
    private Chord m_ChordPlayed;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        m_Stats = Academy.Instance.StatsRecorder;
        m_ChordPlayer = FindObjectOfType<ChordPlayer>();
        m_ChordPlayer.RegisterAgent(this);
    }

    /// <inheritdoc/>
    public override void OnEpisodeBegin()
    {
        m_DecisionCount = 0;
        StartCoroutine(DelayAgentReady(c_EpisodeDelay));
    }

    /// <summary>
    /// Called by <see cref="ChordPlayer"/> prior to playing the next chord.
    /// <param name="chord">The chord played.</param>
    /// </summary>
    public void OnNextChord(Chord chord)
    {
        // Continue audio sampling.
        m_Sampler.SamplingEnabled = true;
        m_ChordPlayed = chord;
    }

    protected override void OnSamplingUpdate(int samplingStepCount, bool bufferLengthReached)
    {
        if (bufferLengthReached)
        {
            // Pause audio sampling.
            m_Sampler.SamplingEnabled = false;
            m_DecisionCount++;
            RequestDecision();
        }
    }

    /// <inheritdoc/>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var chord = new Chord(actionBuffers.DiscreteActions[0], actionBuffers.DiscreteActions[1]);

        if (m_ChordPlayed.Equals(chord))
        {
            AddReward(1);
            WriteStats(true, true);
        }
        else if (m_ChordPlayed.Type == chord.Type)
        {
            AddReward(0.5f);
            WriteStats(true, false);
        }
        else if (m_ChordPlayed.Key == chord.Key)
        {
            AddReward(0.5f);
            WriteStats(false, true);
        }
        else
        {
            WriteStats(false, false);
        }

        if (m_DecisionCount == m_EpisodeLength)
        {
            EndEpisode();
        }
        else
        {
            StartCoroutine(DelayAgentReady(m_PauseDuration));
        }
    }

    private void WriteStats(bool hasTypeMatch, bool hasKeyMatch)
    {
        m_Stats.Add("ChordType/" + m_ChordPlayed.Type, hasTypeMatch ? 1 : 0);
        m_Stats.Add("Chord/Type", hasTypeMatch ? 1 : 0);
        m_Stats.Add("Chord/Key", hasKeyMatch ? 1 : 0);
    }

    private IEnumerator DelayAgentReady(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        m_ChordPlayer.OnAgentReady();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}