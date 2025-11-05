// PathGeneratorTool.cs
using Godot;

// [Tool] è fondamentale per far girare lo script nell'editor
[Tool]
public partial class PathGeneratorTool : Node3D
{
    // === INPUT CONFIGURABILI ===

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

    // === VARIABILI INTERNE ===

    // Tiene traccia degli edifici creati per poterli pulire
    private Godot.Collections.Array<Node> _generatedFollowers = new Godot.Collections.Array<Node>();

    // === TRIGGER (Il pulsante "Generate" nell'Inspector) ===
    private bool _generate = false;
    [Export]
    public bool Generate
    {
        get => _generate;
        set
        {
            _generate = value;
            // Esegui solo in editor e solo quando la checkbox è cliccata
            if (Engine.IsEditorHint() && _generate)
            {
                GenerateBuildings();
                _generate = false; // Resetta il "pulsante"
            }
        }
    }

    // === LOGICA PRINCIPALE ===
    private void GenerateBuildings()
    {
        // 1. Controlli di sicurezza
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

        // 2. Pulizia
        // Rimuovi tutti gli edifici generati in precedenza
        foreach (Node follower in _generatedFollowers)
        {
            follower.QueueFree();
        }
        _generatedFollowers.Clear();

        // 3. Preparazione
        Curve3D curve = pathNode.Curve;
        float totalPathLength = curve.GetBakedLength();
        float currentDistance = 0.0f;

        // 4. Loop di Generazione
        while (currentDistance < totalPathLength)
        {
            // --- Istanziazione Sicura (correzione C#) ---
            Node newBuildingNode = _buildingGeneratorScene.Instantiate();
            BuildingGenerator newBuilding = newBuildingNode as BuildingGenerator;

            if (newBuilding == null)
            {
                GD.PrintErr($"ERRORE FATALE: Il nodo radice di '{_buildingGeneratorScene.ResourcePath}' NON è un 'BuildingGenerator'.");
                GD.PrintErr("Assicurati che 'BuildingGenerator.cs' abbia [Tool], [GlobalClass], 'public partial' e RICOMPILA il progetto C#.");
                newBuildingNode.QueueFree();
                return; // Interrompi tutto
            }

            // --- Generazione e Calcolo Dimensioni ---
            int floors = GD.RandRange(0, _maxMidFloors);
            Aabb buildingBounds = newBuilding.GenerateBuilding(floors);

            // --- Correzione Logica Assi (X vs Z) ---
            // Assumiamo che la "facciata" (lunghezza) sia sull'asse Z
            // e la "profondità" (laterale) sia sull'asse X.
            float buildingLengthAlongPath = buildingBounds.Size.Z;
            float buildingDepth = buildingBounds.Size.X;

            // Calcola dove posizionare il *centro* dell'edificio
            float placementDistance = currentDistance + (buildingLengthAlongPath / 2.0f);

            // Controlla se l'edificio esce dal percorso
            if (placementDistance + (buildingLengthAlongPath / 2.0f) > totalPathLength)
            {
                newBuilding.QueueFree(); // Scarta questo edificio
                break; // Fine del percorso, esci dal loop
            }

            // --- Creazione e Posizionamento ---
            PathFollow3D follower = new PathFollow3D();
            pathNode.AddChild(follower);
            _generatedFollowers.Add(follower); // Salvalo per la pulizia futura

            // --- Correzione Posizionamento (Godot 4) ---
            // Usa 'Progress' (distanza in metri) per muoversi LUNGO il percorso
            follower.Progress = placementDistance;
            follower.RotationMode = PathFollow3D.RotationModeEnum.Y; // Mantieni l'edificio dritto (Y-Up)
            follower.Loop = false;

            // Aggiungi l'edificio al follower
            follower.AddChild(newBuilding);

            // --- Correzione Orientamento e Offset Laterale ---
            // Il follower punta +Z lungo il percorso. I modelli 3D standard
            // hanno la facciata lungo -Z. Ruota di 180° per farli "guardare avanti".
            newBuilding.RotationDegrees = new Vector3(0, 90, 0);

            // Sposta l'edificio a lato (asse X del follower)
            newBuilding.Position = new Vector3((buildingDepth / 2.0f) + _sideOffset, 0, 0);

            // --- Avanzamento ---
            // Avanza sul percorso per il prossimo edificio
            currentDistance += buildingLengthAlongPath + _spacing;
        }
    }
}