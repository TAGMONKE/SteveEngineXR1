using OpenTK;
using SteveEngine;

public class EditorTool
{
    public static GameObject CreatePrimitiveObject(OpenTK.Mathematics.Vector3 position)
    {
        GameObject obj = new GameObject("pr");
        obj.Transform.Position = position;
        
        var renderer = obj.AddComponent("MeshRenderer");
        
        var collider = obj.AddComponent("Collider");

        return obj;
    }
}
