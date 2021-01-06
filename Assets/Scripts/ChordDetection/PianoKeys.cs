using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PianoKeys : MonoBehaviour
{
    [SerializeField]
    private Color m_RootKeyHighlight;
    [SerializeField]
    private Color m_OtherKeysHighlight;

    [SerializeField]
    private int m_MaxCount = 24;
    [SerializeField]
    private int m_Threshold = 4;
    [SerializeField]
    private float m_Exponent = 1;

    private MaterialPropertyBlock m_MPB;
    private Dictionary<int, int> m_RootKeyCount;
    private Dictionary<int, int> m_OtherKeysCount;
    private Dictionary<int, Color> m_DefColors;
    private Dictionary<int, Renderer> m_Renderers;
    private List<int> m_Keys;

    public void OnAgentGuess(Chord chord)
    {
        bool shift = chord.Key < 4;
        var pianoKeys = chord.PianoKeys.Select(k => shift ? k + 12 : k);
        var root = pianoKeys.First();
        var rest = pianoKeys.Skip(1);

        foreach (int k in m_Keys)
        {
            int c = m_RootKeyCount[k];
            c += k == root ? 1 : -1;
            m_RootKeyCount[k] = Mathf.Clamp(c, 0, m_MaxCount);

            c = m_OtherKeysCount[k];
            c += rest.Contains(k) ? 1 : -1;
            m_OtherKeysCount[k] = Mathf.Clamp(c, 0, m_MaxCount);
        }

        SetColors();
    }

    private void Awake()
    {
        m_MPB = new MaterialPropertyBlock();
        m_RootKeyCount = new Dictionary<int, int>();
        m_OtherKeysCount = new Dictionary<int, int>();
        m_DefColors = new Dictionary<int, Color>();
        m_Renderers = new Dictionary<int, Renderer>();
        m_Keys = new List<int>();

        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            var r = c.GetComponent<Renderer>();
            int k = short.Parse(c.name);
            m_RootKeyCount.Add(k, 0);
            m_OtherKeysCount.Add(k, 0);
            m_DefColors.Add(k, r.material.color);
            m_Renderers.Add(k, r);
            m_Keys.Add(k);
        }
    }

    private void SetColors()
    {
        foreach (int k in m_Keys)
        {
            var col = m_DefColors[k];
            col = Color.Lerp(col, m_OtherKeysHighlight, Interpolation(m_OtherKeysCount[k]));
            col = Color.Lerp(col, m_RootKeyHighlight, Interpolation(m_RootKeyCount[k]));
            m_MPB.SetColor("_Color", col);
            m_Renderers[k].SetPropertyBlock(m_MPB);
        }
    }

    private float Interpolation(int count)
    {
        if (count <= m_Threshold)
        {
            return 0;
        }

        float v = (count - m_Threshold) / (float)(m_MaxCount - m_Threshold);
        return Mathf.Pow(v, m_Exponent);
    }
}
