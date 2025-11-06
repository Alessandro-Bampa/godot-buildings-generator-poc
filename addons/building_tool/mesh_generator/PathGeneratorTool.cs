// PathGeneratorTool.cs
using Godot;

[Tool]
public partial class PathGeneratorTool : Node3D
{
    [Export]
    private NodePath _pathNodePath;
    [Export]
    private PackedScene _buildingGeneratorScene;
    [Export]
    private float _spacing = 1.0f;
    [Export]
    private float _sideOffset = 2.0f;
    [Export(PropertyHint.Range, "0,10,1")]
    private int _maxMidFloors = 3;

    private Godot.Collections.Array<Node> _generatedFollowers = new Godot.Collections.Array<Node>();

    // ... (la proprietà 'Generate' get/set rimane invariata) ...
    private bool _generate = false;
    [Export]
    public bool Generate
    {
        get => _generate;
        set
        {
            _generate = value;
            if (Engine.IsEditorHint() && _generate)
            {
                GenerateBuildings();
                _generate = false;
            }
        }
    }


    private void GenerateBuildings()
    {
        // ... (controlli di sicurezza e pulizia rimangono invariati) ...
        if (_pathNodePath == null || _buildingGeneratorScene == null)
        {
            GD.PrintErr("Path o Building Scene non impostati!");
            return;
        }
        Path3D pathNode = GetNode<Path3D>(_pathNodePath);
        if (pathNode == null || pathNode.Curve == null)
        {
            GD.PrintErr("Nodo Path non trovato o curva non disegnata!");
            return;
        }

        foreach (Node follower in _generatedFollowers)
        {
            follower.QueueFree();
        }
        _generatedFollowers.Clear();

        Curve3D curve = pathNode.Curve;
        float totalPathLength = curve.GetBakedLength();
        float currentDistance = 0.0f;

        // **MODIFICA:** Ottieni il proprietario della scena UNA SOLA VOLTA
        Node sceneOwner = GetTree().EditedSceneRoot;

        while (currentDistance < totalPathLength)
        {
            Node newBuildingNode = _buildingGeneratorScene.Instantiate();
            BuildingGenerator newBuilding = newBuildingNode as BuildingGenerator;
            if (newBuilding == null)
            {
                GD.PrintErr($"ERRORE FATALE: Il nodo radice di '{_buildingGeneratorScene.ResourcePath}' NON è un 'BuildingGenerator'.");
                newBuildingNode.QueueFree();
                return;
            }

            int floors = GD.RandRange(0, _maxMidFloors);

            // **MODIFICA:** Passa 'sceneOwner' alla funzione GenerateBuilding
            Aabb buildingBounds = newBuilding.GenerateBuilding(floors, sceneOwner);

            // ... (logica di calcolo dimensioni e distanza invariata) ...
            float buildingLengthAlongPath = buildingBounds.Size.Z;
            float buildingDepth = buildingBounds.Size.X;
            float placementDistance = currentDistance + (buildingLengthAlongPath / 2.0f);

            if (placementDistance + (buildingLengthAlongPath / 2.0f) > totalPathLength)
            {
                newBuilding.QueueFree();
                break;
            }

            PathFollow3D follower = new PathFollow3D();
            pathNode.AddChild(follower); // 1. Aggiungi alla scena
            _generatedFollowers.Add(follower);

            // ... (impostazioni follower: Progress, RotationMode, Loop) ...
            follower.Progress = placementDistance;
            follower.RotationMode = PathFollow3D.RotationModeEnum.Y;
            follower.Loop = false;

            follower.AddChild(newBuilding); // 2. Aggiungi alla scena

            // ... (impostazioni newBuilding: RotationDegrees, Position) ...
            newBuilding.RotationDegrees = new Vector3(0, 0, 0);
            newBuilding.Position = new Vector3((buildingDepth / 2.0f) + _sideOffset, 0, 0);

            if (Engine.IsEditorHint())
            {
                // 3. Imposta il proprietario per i nodi creati da QUESTO tool
                follower.Owner = sceneOwner;
                newBuilding.Owner = sceneOwner;
            }

            currentDistance += buildingLengthAlongPath + _spacing;
        }
    }
}