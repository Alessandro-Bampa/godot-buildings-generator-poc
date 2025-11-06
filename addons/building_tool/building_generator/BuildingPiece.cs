// BuildingPiece.cs
using Godot;

[Tool] // Aggiungiamo [Tool] per far girare l'helper nell'editor
[GlobalClass]
public partial class BuildingPiece : Node
{
    [Export]
    public Vector3 Dimensions { get; private set; } = new Vector3(1, 1, 1);

    // --- Helper Button ---
    private bool _autoSet = false;
    [Export]
    public bool AutoSetFromVisualBounds
    {
        get => _autoSet;
        set
        {
            _autoSet = value;
            if (Engine.IsEditorHint() && _autoSet)
            {
                AutoSetDimensions();
                _autoSet = false; // Resetta il pulsante
            }
        }
    }

    private void AutoSetDimensions()
    {
        Node parent = GetParent();
        if (parent == null)
        {
            GD.PrintErr("BuildingPiece deve essere figlio di un altro nodo.");
            return;
        }

        // Cerchiamo un "fratello" che sia un VisualInstance3D (la nostra mesh)
        VisualInstance3D visualNode = null;
        foreach (Node child in parent.GetChildren())
        {
            if (child is VisualInstance3D vInstance)
            {
                visualNode = vInstance;
                break; // Trovato!
            }
        }

        if (visualNode != null)
        {
            // Ottieni l'Aabb (la scatola gialla)
            Aabb bounds = visualNode.GetAabb();

            // Imposta le nostre dimensioni sulla *dimensione* di quella scatola
            this.Dimensions = bounds.Size;

            GD.Print($"Dimensioni di '{parent.Name}' auto-impostate a: {this.Dimensions}");
        }
        else
        {
            GD.PrintErr($"Nessun 'VisualInstance3D' fratello trovato sotto '{parent.Name}'.");
        }
    }
}