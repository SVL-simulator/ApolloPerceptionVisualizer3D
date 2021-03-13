/**
 * Copyright (c) 2019-2021 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using UnityEngine;
using Simulator.Bridge;
using Simulator.Bridge.Data;
using Simulator.Utilities;
using Simulator.Sensors.UI;
using Simulator.Map;
using System;

namespace Simulator.Sensors
{
    [SensorType("ApolloPerceptionVisualizer3D", new[] { typeof(Detected3DObjectArray) })]
    public class ApolloPerceptionVisualizer3D : SensorBase
    {
        Detected3DObject[] Detected = Array.Empty<Detected3DObject>();

        WireframeBoxes WireframeBoxes;
        MapOrigin MapOrigin;

        [AnalysisMeasurement(MeasurementType.Count)]
        public int MaxTracked = -1;

        public override SensorDistributionType DistributionType => SensorDistributionType.HighLoad;

        void Start()
        {
            WireframeBoxes = SimulatorManager.Instance.WireframeBoxes;
            MapOrigin = MapOrigin.Find();
        }

        public override void OnBridgeSetup(BridgeInstance bridge)
        {
            bridge.AddSubscriber<Detected3DObjectArray>(Topic, data => Detected = data.Data);
        }

        public override void OnVisualize(Visualizer visualizer)
        {
            MaxTracked = Math.Max(MaxTracked, Detected.Length);
            foreach (var detected in Detected)
            {
                Color color;
                switch (detected.Label)
                {
                    case "Car":
                        color = Color.green;
                        break;
                    case "Pedestrian":
                        color = Color.yellow;
                        break;
                    case "Bicycle":
                        color = Color.cyan;
                        break;
                    default:
                        color = Color.magenta;
                        break;
                }

                if (MapOrigin == null)
                {
                    Debug.LogError("Fail to visualize Apollo perceptions due to null MapOrigin.");
                    return;
                }

                var position = MapOrigin.FromNorthingEasting(detected.Gps.Northing, detected.Gps.Easting);
                position.y = (float)detected.Gps.Altitude - MapOrigin.AltitudeOffset;

                var mapRotation = MapOrigin.transform.localRotation;
                var rotationY = detected.Heading + mapRotation.eulerAngles.y;
                Quaternion rotation = Quaternion.Euler(0, (float)rotationY, 0);

                var transform = Matrix4x4.TRS(position, rotation, Vector3.one);
                WireframeBoxes.Draw(transform, Vector3.zero, detected.Scale, color);
            }
        }

        public override void OnVisualizeToggle(bool state)
        {
            //
        }
    }
}
