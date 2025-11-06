// BuildingGenerator.cs (Corretto per la tua struttura)
using Godot;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class BuildingGenerator : Node3D
{
    [Export]
    private PackedScene[] _groundFloorPrefabs;
    [Export]
    private PackedScene[] _midFloorPrefabs;
    [Export]
    private PackedScene[] _roofPrefabs;

    private List<Node3D> _generatedParts = new List<Node3D>();

    // Accetta 'owner' per risolvere l'errore "Invalid owner"
    public Aabb GenerateBuilding(int numMidFloors, Node owner)
    {
        foreach (var child in _generatedParts)
        {
            child.QueueFree();
        }
        _generatedParts.Clear();

        float currentTopY = 0.0f;
        Aabb groundFloorAabbForFootprint = new Aabb();
        bool isEditor = Engine.IsEditorHint();

        // --- 1. Genera Piano Terra ---
        if (_groundFloorPrefabs != null && _groundFloorPrefabs.Length > 0)
        {
            Node3D groundFloor = _groundFloorPrefabs[GD.RandRange(0, _groundFloorPrefabs.Length - 1)].Instantiate<Node3D>();
            AddChild(groundFloor);
            if (isEditor) groundFloor.Owner = owner;
            _generatedParts.Add(groundFloor);

            BuildingPiece pieceInfo = groundFloor.GetNode<BuildingPiece>("BuildingPiece");
            if (pieceInfo == null)
            {
                GD.PrintErr($"Pezzo '{groundFloor.Name}' non ha un nodo 'BuildingPiece'!");
                return new Aabb();
            }

            // Usa 'Dimensions' da BuildingPiece come UNICA fonte di verità
            Vector3 size = pieceInfo.Dimensions;

            // Logica di impilamento per origini centrate
            groundFloor.Position = new Vector3(0, size.Y / 2.0f, 0);
            currentTopY = size.Y;

            // L'AABB del footprint si basa su 'Dimensions'
            groundFloorAabbForFootprint = new Aabb(new Vector3(-size.X / 2.0f, -size.Y / 2.0f, -size.Z / 2.0f), size);
        }

        // --- 2. Genera Piani Intermedi ---
        if (_midFloorPrefabs != null && _midFloorPrefabs.Length > 0)
        {
            for (int i = 0; i < numMidFloors; i++)
            {
                Node3D midFloor = _midFloorPrefabs[GD.RandRange(0, _midFloorPrefabs.Length - 1)].Instantiate<Node3D>();
                AddChild(midFloor);
                if (isEditor) midFloor.Owner = owner;
                _generatedParts.Add(midFloor);

                BuildingPiece pieceInfo = midFloor.GetNode<BuildingPiece>("BuildingPiece");
                if (pieceInfo == null) continue;

                Vector3 size = pieceInfo.Dimensions;

                midFloor.Position = new Vector3(0, currentTopY + (size.Y / 2.0f), 0);
                currentTopY += size.Y;
            }
        }

        // --- 3. Genera Tetto ---
        if (_roofPrefabs != null && _roofPrefabs.Length > 0)
        {
            Node3D roof = _roofPrefabs[GD.RandRange(0, _roofPrefabs.Length - 1)].Instantiate<Node3D>();
            AddChild(roof);
            if (isEditor) roof.Owner = owner;
            _generatedParts.Add(roof);

            BuildingPiece pieceInfo = roof.GetNode<BuildingPiece>("BuildingPiece");
            if (pieceInfo == null) return groundFloorAabbForFootprint;

            Vector3 size = pieceInfo.Dimensions;

            roof.Position = new Vector3(0, currentTopY + (size.Y / 2.0f), 0);
        }

        return groundFloorAabbForFootprint;
    }
}