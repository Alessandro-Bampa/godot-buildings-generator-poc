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

    private List<Node> _generatedParts = new List<Node>();

    /// <summary>
    /// Genera l'edificio e restituisce l'AABB (footprint) del *solo* piano terra.
    /// </summary>
    public Aabb GenerateBuilding(int numMidFloors)
    {
        foreach (var child in _generatedParts)
        {
            child.QueueFree();
        }
        _generatedParts.Clear();

        // Tracciamo l'altezza Y della "cima" dell'ultimo pezzo aggiunto
        float currentTopY = 0.0f;

        // AABB del piano terra, usato per il footprint
        Aabb groundFloorAabb = new Aabb();

        // --- 1. Genera Piano Terra ---
        if (_groundFloorPrefabs != null && _groundFloorPrefabs.Length > 0)
        {
            // Istanzia e casta al tipo base del pezzo (CSGBox3D)

            CsgBox3D groundFloor = _groundFloorPrefabs[GD.RandRange(0, _groundFloorPrefabs.Length - 1)].Instantiate<CsgBox3D>();
            AddChild(groundFloor);
            _generatedParts.Add(groundFloor);

            // Ottieni la dimensione del "pavimento" (il nodo radice)
            Vector3 size = groundFloor.Size; // es. (10, 3, 10)

            // L'origine (0,0,0) di un CSGBox è al centro.
            // La sua "base" è a -size.Y / 2 (es. -1.5)
            // Vogliamo che la base dell'edificio sia a Y=0.
            // Quindi, spostiamo il pezzo in su di (size.Y / 2).
            groundFloor.Position = new Vector3(0, size.Y / 2.0f, 0);

            // Calcola la nuova "cima"
            currentTopY = size.Y; // es. 3.0

            // Costruisci l'AABB del footprint per il PathGeneratorTool
            // basandoti SOLO sulla dimensione del nodo radice.
            groundFloorAabb = new Aabb(new Vector3(-size.X / 2.0f, -size.Y / 2.0f, -size.Z / 2.0f), size);
        }

        // --- 2. Genera Piani Intermedi ---
        if (_midFloorPrefabs != null && _midFloorPrefabs.Length > 0)
        {
            for (int i = 0; i < numMidFloors; i++)
            {
                CsgBox3D midFloor = _midFloorPrefabs[GD.RandRange(0, _midFloorPrefabs.Length - 1)].Instantiate<CsgBox3D>();
                AddChild(midFloor);
                _generatedParts.Add(midFloor);

                Vector3 size = midFloor.Size; // es. (10, 3, 10)

                // La "base" di questo nuovo pezzo è a -size.Y / 2 (es. -1.5)
                // Vogliamo che la sua base si allinei con la "cima" precedente (currentTopY)
                // Quindi, la sua Posizione.Y = currentTopY + (size.Y / 2)
                // es. 3.0 + (3.0 / 2) = 4.5
                midFloor.Position = new Vector3(0, currentTopY + (size.Y / 2.0f), 0);

                // Aggiorna la nuova "cima"
                currentTopY += size.Y; // es. 3.0 + 3.0 = 6.0
            }
        }

        // --- 3. Genera Tetto ---
        if (_roofPrefabs != null && _roofPrefabs.Length > 0)
        {
            CsgBox3D roof = _roofPrefabs[GD.RandRange(0, _roofPrefabs.Length - 1)].Instantiate<CsgBox3D>();
            AddChild(roof);
            _generatedParts.Add(roof);

            Vector3 size = roof.Size; // es. (11, 0.5, 11)

            // Stessa logica di impilamento dei piani intermedi
            roof.Position = new Vector3(0, currentTopY + (size.Y / 2.0f), 0);

            // (Non è necessario aggiornare currentTopY, è l'ultimo pezzo)
        }

        return groundFloorAabb;
    }
}