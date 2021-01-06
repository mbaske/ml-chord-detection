using Unity.MLAgents.Actuators;

public class AgentInference : AudioAgent
{
    private PianoKeys m_PianoKeys;

    public override void Initialize()
    {
        base.Initialize();
        m_PianoKeys = FindObjectOfType<PianoKeys>();
    }

    /// <inheritdoc/>
    public override void OnEpisodeBegin()
    {
        m_Sampler.SamplingEnabled = true;
    }

    protected override void OnSamplingUpdate(int samplingStepCount, bool bufferLengthReached)
    {
        RequestDecision();
    }

    /// <inheritdoc/>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var chord = new Chord(actionBuffers.DiscreteActions[0], actionBuffers.DiscreteActions[1]);
        m_PianoKeys.OnAgentGuess(chord);
    }
}