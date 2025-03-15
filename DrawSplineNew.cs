using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrawLibrary;
using Godot;
using static DrawLibrary.MathHelper;

namespace SplineDemo
{
    public partial class DrawSplineNew : Node2D
    {
        [Export]
        public Sprite2D sprite;
        ImageTexture texture;
        Image image;

        Vector2 centerExact;
        Vector2I center;

        public override void _Ready()
        {
            Vector2 size = GetViewportRect().Size;
            int width = Round(size.X);
            int length = Round(size.Y);
            image = Image.Create(width, length, false, Image.Format.Rgba8);
            image.Fill(Color.Color8(255, 255, 255));
            sprite.Position = size / 2;
            centerExact = new Vector2(width / (float)2, length / (float)2);
            center = (Vector2I)centerExact;
            texture = ImageTexture.CreateFromImage(image);
            sprite.Texture = texture;
        }

        Vector2I[] Pixels = new Vector2I[1000]; 
        public override void _Process(double delta)
        {
            Span<Vector2I> pixels = new Span<Vector2I>(Pixels);

            CubicCurve curve = CubicCurve.ConstructCubicCurveFromParametricForm(
                new ParametricCubicCurve(

                    )
                );
        }
    }
}
