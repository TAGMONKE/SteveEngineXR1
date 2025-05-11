using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteveEngine
{
    public class CharacterController : Component
    {
        public CharacterController() { }
        public void Move(Vector3 direction, float speed)
        {
            // Assuming the GameObject has a Transform component  
            var transform = GameObject.Transform;
            Vector3 newPosition = transform.Position + direction * speed * Time.DeltaTime;
            transform.Position = newPosition;
        }
        public void Jump(float height)
        {
            // Assuming the GameObject has a Rigidbody component  
            Rigidbody rigidbody = (Rigidbody)GameObject.Components.FirstOrDefault(x => x.GetType().Name == "Rigidbody");
            if (rigidbody != null)
            {
                rigidbody.AddForce(new Vector3(0, height, 0), ForceMode.Impulse);
            }
        }
        public void Rotate(Vector3 rotation)
        {
            // Assuming the GameObject has a Transform component  
            var transform = GameObject.Transform;
            transform.Rotation += rotation;
        }
    }
}
