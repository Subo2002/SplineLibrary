using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DrawLibrary
{


	public struct Line
	{
		public Vector2I point0;
		public Vector2I point1;

		public Line(Vector2I point0, Vector2I point1)
		{
			this.point0 = point0;
			this.point1 = point1;
		}

		public static Span<Vector2I> ComputePixels(in Line line, Span<Vector2I> pixelsBuffer)
		{
			int i = 0;

			var (point0, point1) = line;

			int dx = Math.Abs(point1.X - point0.X);
			int dy = -Math.Abs(point1.Y - point0.Y);
			int e = dx + dy;
			int e2;
			int x = point0.X;
			int y = point0.Y;
			int sx = point1.X > point0.X ? 1 : -1;
			int sy = point1.Y > point0.Y ? 1 : -1;
			while (true)
			{
				if (i == pixelsBuffer.Length)
					break;
				ref Vector2I point = ref pixelsBuffer[i++];
				point.X = x;
				point.Y = y;

				e2 = 2 * e;
				if (e2 >= dy)
				{
					e += dy;
					x += sx;
					if (x == point1.X)
						break;
					
				}
				if (e2 <= dx)
				{
					e += dx;
					y += sy;
					if (y == point1.Y)
						break;
					
				}
			}

			return pixelsBuffer.Slice(0, i);
		}

		public readonly void Deconstruct(out Vector2I point0, out Vector2I point1)
		{
			(point0, point1) = (this.point0, this.point1);
		}
	}
}
