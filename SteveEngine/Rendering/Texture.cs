using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;

namespace SteveEngine
{
    public class Texture
    {
        public int Id { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        public Texture(string path)
        {
            LoadTexture(path);
        }
        
        private void LoadTexture(string path)
        {
            Id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Id);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            using (var image = new Bitmap(path))
            {
                Width = image.Width;
                Height = image.Height;
                
                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
                    image.Width, image.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, 
                    PixelType.UnsignedByte, data.Scan0);
                
                image.UnlockBits(data);
            }
            
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
        
        public void Use(int unit = 0)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            GL.BindTexture(TextureTarget.Texture2D, Id);
        }
    }
}