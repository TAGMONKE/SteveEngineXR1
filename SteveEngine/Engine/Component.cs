// Update Component.cs
using SteveEngine;

public abstract class Component
{
    public GameObject GameObject { get; set; }

    // Component lifecycle methods
    public virtual void Awake() { } // Called when component is first created
    public virtual void Start() { } // Called before the first update
    public virtual void Update(float deltaTime) { }
    public virtual void OnDestroy() { } // Called when component is removed
}
