// BuildingToolPlugin.cs
using Godot;

[Tool]
public partial class BuildingToolPlugin : EditorPlugin
{
    // Carichiamo gli script come risorse in modo da poterli passare
    // all'editor.
    private readonly Script _pathToolScript = GD.Load<Script>("res://addons/building_tool/mesh_generator/PathGeneratorTool.cs");
    private readonly Script _buildingGenScript = GD.Load<Script>("res://addons/building_tool/building_generator/BuildingGenerator.cs");
    private readonly Script _buildingPieceScript = GD.Load<Script>("res://addons/building_tool/building_generator/BuildingPiece.cs");

    private readonly Texture2D _pathIcon = GD.Load<Texture2D>("res://addons/building_tool/path_icon.svg");

    private readonly Texture2D _brickIcon = GD.Load<Texture2D>("res://addons/building_tool/brick_wall_icon.svg");

    private readonly Texture2D _houseIcon = GD.Load<Texture2D>("res://addons/building_tool/house_icon.svg");

    public override void _EnterTree()
    {
        // Questo è il cuore: registra i tuoi script come
        // nuovi tipi di nodi nell'editor.

        // Parametri: 
        // 1. Nome del tipo (come appare nell'editor)
        // 2. Tipo base (da cosa eredita)
        // 3. Script da agganciare
        // 4. Icona (opzionale)

        AddCustomType("PathGeneratorTool", "Node3D", _pathToolScript, _pathIcon); // icon
        AddCustomType("BuildingGenerator", "Node3D", _buildingGenScript, _houseIcon); // icon
        AddCustomType("BuildingPiece", "Node", _buildingPieceScript, _brickIcon); // icon
    }

    public override void _ExitTree()
    {
        // Pulisci tutto quando il plugin viene disattivato
        RemoveCustomType("PathGeneratorTool");
        RemoveCustomType("BuildingGenerator");
        RemoveCustomType("BuildingPiece");
    }
}
