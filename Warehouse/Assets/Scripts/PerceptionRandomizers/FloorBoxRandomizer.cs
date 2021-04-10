using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Randomizers.SampleRandomizers;
using UnityEngine.Perception.Randomization.Randomizers.SampleRandomizers.Tags;
using UnityEngine.Perception.Randomization.Samplers;
using Unity.Simulation.Warehouse;
using RosSharp.Control;

using Object = UnityEngine.Object;

[Serializable]
[AddRandomizerMenu("Perception/Floor Box Randomizer")]
public class FloorBoxRandomizer : Randomizer
{
    public GameObject objectToSpawn;
    public int numObjectToSpawn;
    public int maxPlacementTries = 100;
    private FloatParameter random = new FloatParameter {value = new UniformSampler(0f, 1f)};

    private SurfaceObjectPlacer placer;
    private GameObject parentFloorBoxes;
    private List<CollisionConstraint> constraints;
    private CollisionConstraint turtleConstraint;

    protected override void OnAwake()
    {
        // Add collision constraints to spawned shelves
        var tags = tagManager.Query<ShelfBoxRandomizerTag>();

        constraints = new List<CollisionConstraint>();

        foreach (var tag in tags)
        {
            var shelf = new CollisionConstraint(tag.transform.position.x, tag.transform.position.z, tag.GetComponentInChildren<Renderer>().bounds.extents.x);
            constraints.Add(shelf);
        }

        base.OnAwake();
    }

    protected override void OnIterationStart()
    {
        // Create floor boundaries for spawning
        var bounds = new Bounds(Vector3.zero, new Vector3(WarehouseManager.instance.m_width, 0, WarehouseManager.instance.m_length));
        placer = new SurfaceObjectPlacer(bounds, random, maxPlacementTries);

        // Instantiate boxes at arbitrary location
        parentFloorBoxes = new GameObject("SpawnedBoxes");
        for (int i = 0; i < numObjectToSpawn; i++) 
        {
            var o = Object.Instantiate(objectToSpawn, parentFloorBoxes.transform);
            o.AddComponent<FloorBoxRandomizerTag>();
            o.AddComponent<RotationRandomizerTag>();
        }

        // Begin placement interation
        placer.IterationStart();
        
        var tags = tagManager.Query<FloorBoxRandomizerTag>();

        foreach (var tag in tags)
        {
            bool success = placer.PlaceObject(tag.gameObject);
            if (!success)
            {
                return;
            }
        }
    }

    protected override void OnIterationEnd()
    {
        Object.Destroy(parentFloorBoxes);
    }
}