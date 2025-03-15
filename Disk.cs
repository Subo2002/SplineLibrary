using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawLibrary
{
	public class Disk
	{
		public Vector2I center;
		public int radius;

		public Vector2I[] pixelPoints = new Vector2I[2000];
		public int noPixelPoints;

		public void ComputePixelPoints()
		{
			noPixelPoints = 0;
			int r2 = radius * radius;

			int y0 = -radius;
			int y1 = +radius;

			int x0 = -radius;
			int x1 = +radius;

			int x02 = x0 * x0;

			int x2;
			int y2 = y0 * y0;
			for (int y = y0; y <= y1; y2 += y++ * 2 + 1)
			{
				x2 = x02;
				for (int x = x0; x <= x1; x2 += x++ * 2 + 1)
				{
					//GD.Print("x: " + x + ", y: " + y + ", x2: " + x2 + ", y2: " + y2 + ", r2: " + r2);
					if (x2 + y2 >= r2)
						continue;
					ref Vector2I pos = ref pixelPoints[noPixelPoints++];
					pos.X = x;
					pos.Y = y;
					pos += center;
				}
			}
		}
	}
}
