using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawLibrary
{
    public static class MathHelper
    {
        public static void AddPixelsToImage(Image image, Span<Vector2I> pixels, Color color)
        {
            for (int i = 0; i < pixels.Length; i++)
                image.SetPixelv(pixels[i], color);
        }

        public static int Round(double X)
        {
            int x = (int)X;
            switch (X - x)
            {
                case < -0.5:
                    x--;
                    break;
                case < 0.5:
                    break;
                case >= 0.5:
                    x++;
                    break;
            }
            return x;
        }

        static readonly Vector2I NW = new(-1, -1);
        static readonly Vector2I N = new(0, -1);
        static readonly Vector2I NE = new(1, -1);
        static readonly Vector2I W = new(-1, 0);
        static readonly Vector2I Zero = new(0, 0);
        static readonly Vector2I E = new(1, 0);
        static readonly Vector2I SW = new(-1, 1);
        static readonly Vector2I S = new(0, 1);
        static readonly Vector2I SE = new(1, 1);

        public static Vector2I Round(Vector2 pos)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;

            Vector2I output = new();

            output.X = x;
            output.Y = y;

            output +=
            (pos.X - x, pos.Y - y) switch
            {
                ( < -0.5f, < -0.5f) => NW,
                ( > 0.5f, < -0.5f) => NE,
                (_, < -0.5f) => N,
                ( < -0.5f, > 0.5f) => SW,
                ( > 0.5f, > 0.5f) => SE,
                (_, > -0.5f) => S,
                ( < -0.5f, _) => W,
                ( > 0.5f, _) => E,
                (_, _) => Zero,
            };

            return output;
        }

        public static void Round(in Vector2 pos, ref Vector2I output)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;

            output.X = x;
            output.Y = y;

            output +=
            (pos.X - x, pos.Y - y) switch
            {
                (< -0.5f, < -0.5f) => NW,
                (> 0.5f, < -0.5f) => NE,
                (_, <-0.5f) => N,
                ( < -0.5f, > 0.5f) => SW,
                ( > 0.5f, > 0.5f) => SE,
                (_, > -0.5f) => S,
                ( < -0.5f, _) => W,
                ( > 0.5f, _) => E,
                (_, _) => Zero,
            };
        }

        public static void Round_Cumulative(in Vector2 pos, ref Vector2I output, ref Vector2 error)
        {
            error += pos;
            int x = (int)(error.X);
            int y = (int)(error.Y);

            output.X = x;
            output.Y = y;

            output +=
            (error.X - x, error.Y - y) switch
            {
                ( < -0.5f, < -0.5f) => NW,
                ( > 0.5f, < -0.5f) => NE,
                (_, < -0.5f) => N,
                ( < -0.5f, > 0.5f) => SW,
                ( > 0.5f, > 0.5f) => SE,
                (_, > -0.5f) => S,
                ( < -0.5f, _) => W,
                ( > 0.5f, _) => E,
                (_, _) => Zero,
            };

            error -= output;
        }
    }
}
