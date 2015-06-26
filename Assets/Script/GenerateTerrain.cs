using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GenerateTerrain : MonoBehaviour
{
    public GameObject CoverPrefab;
    public GameObject WaypointPrefab; 
    public float Length = 30f;
    public float RangeBetweenCovers = 5.35f;

    private float[] _lanes;

	void Start ()
	{
        Reset();
	}

    public void Reset()
    {
        _lanes = new float[GameLogic.Lanes.Length];
        GameLogic.Lanes.CopyTo(_lanes, 0);

        var children = new List<GameObject>();
        foreach (Transform cover in transform) children.Add(cover.gameObject);
        children.ForEach(DestroyImmediate);

        transform.localScale = new Vector3(1, 1, Length / 10f);
        transform.position = new Vector3(0, 0, Length / 2f);

        GenerateCovers();
    }

    private void GenerateCovers()
    {
        for (var i = 1; i * RangeBetweenCovers < Length; ++i)
        {
            ShuffleLanes();

            var z = i * RangeBetweenCovers;
            CreateCover(0, z);

            if (Random.Range(0f, 1f) < 0.1f)
                CreateCover(1, z);
            else
                CreateCheckpoint(1, z);

            CreateCheckpoint(2, z);
        }
    }

    public void ShuffleLanes()
    {
        var oldValues = new List<float>(_lanes);

        for (var i = 0; i < _lanes.Length; ++i)
        {
            var idx = Random.Range(0, oldValues.Count);
            _lanes[i] = oldValues[idx];
            oldValues.RemoveAt(idx);
        }
    }

    private void CreateCover(int lane, float z)
    {
        var position = new Vector3(_lanes[lane], 0.3f, z);
        var cover = Instantiate(CoverPrefab).transform;
        cover.position = position;
        cover.parent = transform;
    }

    private void CreateCheckpoint(int lane, float z)
    {
        var position = new Vector3(_lanes[lane], 0.3f, z);
        var checkpoint = Instantiate(WaypointPrefab).transform;
        checkpoint.position = position;
        checkpoint.parent = transform;
    }
}
