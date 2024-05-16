using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PlaneMeshGenerator : MonoBehaviour
{
    [Header("Plane Settings")]
    public Vector2 planeSize = new Vector2(50, 50);
    public int resolution = 250;

    [Header("Wave Settings")]
    public float waveSpeed = 1f;
    public float waveLength = 5f;

    [Space(1)]
    [Tooltip("Use only in Edit Mode")]
    [Range(2, 10)]
    public int waveCount = 4;

    public float[] amplitudes;
    public float[] frequenciesX;
    public float[] frequenciesZ;
    public float[] waveOffsets;

    [Header("Random Recalculate Waves")]

    
    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;

    private bool turnOnWaves;

    public void SetTurnOnWaves(bool value)
    {
        turnOnWaves = value;
    }

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        InitializeWaveSettings();
        GenerateMesh(planeSize, resolution);
        AssignMesh();
    }

    private void Update()
    {
        resolution = Mathf.Clamp(resolution, 0, 250);
        if (turnOnWaves)
        {
            PlaneWaving(Time.time);
            AssignMesh();
        }
    }

    private void InitializeWaveSettings()
    {
        amplitudes = new float[waveCount];
        frequenciesX = new float[waveCount];
        frequenciesZ = new float[waveCount];
        waveOffsets = new float[waveCount];

        for (int i = 0; i < waveCount; i++)
        {
            amplitudes[i] = Random.Range(0.1f, 0.5f);
            frequenciesX[i] = Random.Range(0.2f, 0.8f);
            frequenciesZ[i] = Random.Range(0.2f, 0.8f);
            waveOffsets[i] = Random.Range(0f, Mathf.PI * 2);
        }
    }

    public void GenerateMesh(Vector2 size, int resolution)
    {
        vertices = new List<Vector3>();
        float xPerStep = size.x / resolution;
        float zPerStep = size.y / resolution;

        for (int z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                vertices.Add(new Vector3(x * xPerStep, 0, z * zPerStep));
            }
        }

        triangles = new List<int>();
        for (int row = 0; row < resolution; row++)
        {
            for (int column = 0; column < resolution; column++)
            {
                int i = row * (resolution + 1) + column;

                // первый треугольник
                triangles.Add(i);
                triangles.Add(i + resolution + 1);
                triangles.Add(i + resolution + 2);

                // второй треугольник
                triangles.Add(i);
                triangles.Add(i + resolution + 2);
                triangles.Add(i + 1);
            }
        }
    }

    public void AssignMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Пересчет нормалей для правильного освещения
        mesh.RecalculateNormals();
    }

    public void ToggleWaves()
    {
        turnOnWaves = !turnOnWaves;
    }

    public void RandomizeWaveParameters()
    {
        for (int i = 0; i < waveCount; i++)
        {
            amplitudes[i] = Random.Range(-1f, 1f);
            frequenciesX[i] = Random.Range(-1f, 1f);
            frequenciesZ[i] = Random.Range(-1f, 1f);
            waveOffsets[i] = Random.Range(0f, Mathf.PI * 2);
        }
    }

    private void PlaneWaving(float time)
    {
        float k = 2 * Mathf.PI / waveLength; // Волновое число

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            float x = vertex.x;
            float z = vertex.z;

            vertex.y = 0f;

            // Суммируем все волны
            for (int j = 0; j < waveCount; j++)
            {
                vertex.y += amplitudes[j] * Mathf.Sin(k * (frequenciesX[j] * x + frequenciesZ[j] * z) + waveSpeed * time + waveOffsets[j]);
            }

            vertices[i] = vertex;
        }

        mesh.vertices = vertices.ToArray();
        mesh.RecalculateNormals(); // Пересчет нормалей для правильного освещения
    }
}

[CustomEditor(typeof(PlaneMeshGenerator))]
public class PlaneMeshGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlaneMeshGenerator meshGenerator = (PlaneMeshGenerator)target;

        if (GUILayout.Button("Reset Mesh"))
        {
            if (Application.isPlaying)
            {
                meshGenerator.SetTurnOnWaves(false);
                meshGenerator.GenerateMesh(meshGenerator.planeSize, meshGenerator.resolution);
                meshGenerator.AssignMesh();
            }
        }

        if (GUILayout.Button("Turn On/Off Waves"))
        {
            meshGenerator.ToggleWaves();
        }

        if (GUILayout.Button("Randomize Wave Parameters"))
        {
            meshGenerator.RandomizeWaveParameters();
            meshGenerator.GenerateMesh(meshGenerator.planeSize, meshGenerator.resolution);
            meshGenerator.AssignMesh();
        }
    }
}
