namespace SteveEngine
{
    public abstract class Component
    {
        public GameObject GameObject { get; set; }
        
        public virtual void Update(float deltaTime) { }
    }
}