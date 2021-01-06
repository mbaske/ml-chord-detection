﻿using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System;
using UnityEngine;
using AudioSensor.Util;

namespace AudioSensor
{
    /// <summary>
    /// Sensor class for observing audio signals.
    /// Observations are generated by reading samples from the <see cref="AudioBuffer"/>,
    /// which is filled by the <see cref="AudioSensorComponent"/>.
    /// </summary>
    public class AudioSensor : ISensor
    {
        /// <summary>
        /// ResetEvent is invoked when ISensor.Reset() is called.
        /// </summary>
        public event Action ResetEvent;

        /// <summary>
        /// Buffer containing the audio samples.
        /// </summary>
        public AudioBuffer Buffer { get; private set; }

        /// <summary>
        /// PNG compressed observations for access by <see cref="AudioSensorProxy"/>.
        /// </summary>
        public byte[] CachedCompressedObservation { get; private set; }

        /// <summary>
        /// Observation shape of the sensor.
        /// </summary>
        public SensorObservationShape Shape
        {
            get { return m_Shape; }
            set { m_Shape = value; Allocate(); }
        }
        private SensorObservationShape m_Shape;

        /// <summary>
        /// The compression type used by the sensor.
        /// </summary>
        public SensorCompressionType CompressionType
        {
            get { return m_CompressionType; }
            set { m_CompressionType = value; Allocate(); }
        }
        private SensorCompressionType m_CompressionType;


        private List<byte> m_Bytes;
        private Texture2D m_Texture;
        private readonly string m_Name;

        private const int c_ChannelsPerTexture = 3;

        /// <summary>
        /// Initializes the sensor.
        /// </summary>
        /// <param name="shape">Observation shape.</param>
        /// <param name="compression">The compression to apply to the generated image.</param>
        /// <param name="name">Name of the sensor.</param>
        public AudioSensor(SensorObservationShape shape, SensorCompressionType compression, string name)
        {
            m_Name = name;
            m_Shape = shape;
            m_CompressionType = compression;

            Allocate();
        }

        private void Allocate()
        {
            Buffer = new AudioBuffer(m_Shape);

            if (m_CompressionType == SensorCompressionType.PNG)
            {
                m_Texture = TextureUtil.CreateTexture(m_Shape);
                m_Bytes = new List<byte>();
            }
        }

        /// <inheritdoc/>
        public string GetName()
        {
            return m_Name;
        }

        /// <inheritdoc/>
        public int[] GetObservationShape()
        {
            return m_Shape.ToArray();
        }

        /// <inheritdoc/>
        public SensorCompressionType GetCompressionType()
        {
            return m_CompressionType;
        }

        /// <inheritdoc/>
        public byte[] GetCompressedObservation()
        {
            m_Bytes.Clear();

            int n = Mathf.CeilToInt(m_Shape.Channels / (float)c_ChannelsPerTexture);
            for (int textureIndex = 0; textureIndex < n; textureIndex++)
            {
                TextureUtil.UpdateTexture(this, m_Texture, textureIndex, c_ChannelsPerTexture);
                m_Bytes.AddRange(m_Texture.EncodeToPNG());
            }

            CachedCompressedObservation = m_Bytes.ToArray();
            return CachedCompressedObservation;
        }

        /// <inheritdoc/>
        public int Write(ObservationWriter writer)
        {
            int w = m_Shape.Width;
            int h = m_Shape.Height;

            for (int channel = 0; channel < m_Shape.Channels; channel++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        writer[y, x, channel] = Buffer.GetSample(channel, w * y + x);
                    }
                }
            }

            return m_Shape.Channels * m_Shape.Width * m_Shape.Height;
        }

        /// <inheritdoc/>
        public void Update() { }

        /// <inheritdoc/>
        public void Reset()
        {
            Buffer.Clear();
            ResetEvent?.Invoke();
        }
    }
}