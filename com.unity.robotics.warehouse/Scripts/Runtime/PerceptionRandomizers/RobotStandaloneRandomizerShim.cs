using System;
using Unity.Robotics.PerceptionRandomizers.Shims;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using Object = UnityEngine.Object;

[AddRandomizerMenu("Robotics/Robot Standalone Randomizer")]
public class RobotStandaloneRandomizerShim : RandomizerShim
{
    public float distFromEdge;
    GameObject[] floorObjects;
    FloatParameter random = new FloatParameter { value = new UniformSampler(0, 1) };

    protected override void OnScenarioStart()
    {
        floorObjects = GameObject.FindGameObjectsWithTag("Floor");
        base.OnScenarioStart();
    }

    protected override void OnIterationStart()
    {
        var randIdx = Mathf.FloorToInt(random.Sample() * floorObjects.Length);
        var pt = SamplePoint(floorObjects[randIdx], distFromEdge, 10);

        foreach (var tag in Object.FindObjectsOfType<RobotPlacementTag>())
        {
            tag.PlaceRobot(pt + new Vector3(0, 1, 0));
        }
    }

    Vector3 SamplePoint(GameObject obj, float edge, int maxAttempts)
    {
        var bounds = obj.GetComponent<Renderer>().bounds;
        var attempts = 0;

        while (attempts < maxAttempts)
        {
            Vector3 pt;

            pt.x = edge > bounds.extents.x
                ? bounds.center.x
                : random.Sample() * (bounds.extents.x * 2 - edge * 2) + (bounds.center.x - bounds.extents.x);
            pt.y = bounds.center.y + bounds.extents.y;
            pt.z = edge > bounds.extents.z
                ? bounds.center.z
                : random.Sample() * (bounds.extents.z * 2 - edge * 2) + (bounds.center.z - bounds.extents.z);

            // TODO: bounding box check
            attempts++;
            return pt;
        }

        return Vector3.zero;
    }
}
