using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using static DrawLibrary.MathHelper;

namespace DrawLibrary
{
	public struct CubicCurve
	{
		public static CubicCurve ConstructCubicCurveFromParametricForm(in ParametricCubicCurve curve)
		{
			checked
			{
			(int f3, int f2, int f1, int f0, int g3, int g2, int g1, int g0) = 
				(curve.a, curve.b, curve.c, curve.d, curve.e, curve.f, curve.g, curve.h);

			long h01 = f0 * g1 - f1 * g0;
			long h02 = f0 * g2 - f2 * g0;
			long h12 = f1 * g2 - f2 * g1;
			long h03 = f0 * g3 - f3 * g0;
			long h13 = f1 * g3 - f3 * g1;
			long h23 = f2 * g3 - f3 * g2;

			long h12h23 = h12 * h23;
			long h03h23 = h03 * h23;
			long h01h23 = h01 * h23;
			long h13h13 = h13 * h13;
			long h02h23 = h02 * h23;
			long h03h13 = h03 * h13;
			long h02h13 = h02 * h13;
			long h12h03 = h12 * h03;
			long h03h03 = h03 * h03;

			long h01h12h23 = h01 * h12h23;
			long h01h03h23 = h01 * h03h23;
			long h01h13h13 = h01 * h13h13;
			long h02h02h23 = h02 * h02h23;
			long h02h03h13 = h02 * h03h13;
			long h03h03h12 = h12 * h03h03;
			long h03h03h03 = h03 * h03h03;

			long temp1 = h12h23 + h03h23 - h13h13;
			long temp2 = -2 * h02h23 + 2 * h03h13;
			long temp3 = h01h23 + 2 * h02h13 - 2 * h12h03 - 3 * h03h03;

			return new CubicCurve()
			{
				c00 = h01h12h23 + h01h03h23 - h01h13h13 - h02h02h23 + 2 * h02h03h13 - h03h03h12 - h03h03h03,

				c10 = -g1 * temp1 - g2 * temp2 - g3 * temp3,
				c01 = f1 * temp1 + f2 * temp2 + f3 * temp3,

				c20 = (g1 * g3 - g2 * g2) * h23 + 2 * g2 * g3 * h13 - g3 * g3 * (h12 + 3 * h03),
				c11 =
				-(f1 * g3 + f3 * g1 - 2 * f2 * g2) * h23
				- 2 * (f2 * g3 + f3 * g2) * h13 
				+ 2 * f3 * g3 * (h12 + 3 * h03),
				c02 = (f1 * f3 - f2 * f2) * h23 + 2 * f2 * f3 * h13 - f3 * f3 * (h12 + 3 * h03),

				c30 = g3 * g3 * g3,
				c21 = -3 * g3 * g3 * f3,
				c12 = 3 * g3 * f3 * f3,
				c03 = -f3 * f3 * f3,

				start = curve.start,
				end = curve.end
			};


			}
		}

		public long c00;

		public long c10;
		public long c01;

		public long c20;
		public long c11;
		public long c02;

		public long c30;
		public long c21;
		public long c12;
		public long c03;

		public Vector2I start;
		public Vector2I end;

		public static Span<Vector2I> ComputePixels(in CubicCurve curve, Span<Vector2I> pixels)
		{
			GD.Print(
				"c00: " + curve.c00 +

				", c01: " + curve.c01 +
				", c10: " + curve.c10 +

				", c20: " + curve.c20 +
				", c11: " + curve.c11 +
				", c02: " + curve.c02 +

				", c30: " + curve.c30 +
				", c21: " + curve.c21 +
				", c12: " + curve.c12 +
				", c03: " + curve.c03
				);
			return ComputePixels_XY(in curve, pixels);
		}

		private static Span<Vector2I> ComputePixels_XY(in CubicCurve curve, Span<Vector2I> pixels)
		{
			checked
			{
				(int x0, int y0) = curve.start;
				(int x1, int y1) = curve.end;



				(_, long c10, long c01, long c20, long c11, long c02, long c30, long c21, long c12, long c03) = curve;

				long c122 = c12 * 2;
				long c212 = c21 * 2;

				long dxxx = 6 * c30; //how much dxx changes with x++
				long dxx = 2 * c20 + 6 * c30; //how much dx changes with x++
				long dx = c10 + c20 + c30; //how much error changes with x++

				long dxy = c11 + c12 + c21;

				long dyyy = 6 * c03;
				long dyy = 2 * c02 + 6 * c03;
				long dy = c01 + c02 + c03; 

				long e = dx + dy + dxy; //3* to make up for subract terms

				long e2;

				int x = 0;
				int y = 0;

				int i = 0;
				while (x != x1 - x0 || y != y1 - y0)
				{
					if (i >= pixels.Length)
					{
						GD.PrintErr("too many pixels");
						return pixels;
					}
					pixels[i].X = x + x0;
					pixels[i].Y = y + y0;
					i++;

					e2 = 2 * e;

					if (e2 < dy)
					{
						y++;

						dx += dxy;
						dxx += c212;

						dy += dyy;
						dxy += c122;
						e += dy + dxy;
						dyy += dyyy;

					}

					if (e2 > dx)
					{
						x++;

						dy += dxy;
						dyy += c122;

						dx += dxx;
						dxy += c212;
						e += dx + dxy;
						dxx += dxxx;
					}
				}

				return pixels.Slice(0, i);
			}

			
		}

		public static long ComputeErrorAt(in CubicCurve curve, int x, int y)
		{
			var (c00, c10, c01, c20, c11, c02, c30, c21, c12, c03) = curve;
			return
				c00 +
				c10 * x + c01 * y +
				c20 * x * x + c11 * x * y + c02 * y * y +
				c30 * x * x * x + c21 * x * x * y + c12 * x * y * y + c03 * y * y * y;
		}

		
		public readonly void Deconstruct(out long c00, out long c10, out long c01, out long c20, out long c11, out long c02, out long c30, out long c21, out long c12, out long c03)
		{
			(c00, c10, c01, c20, c11, c02, c30, c21, c12, c03) = (this.c00, this.c10, this.c01, this.c20, this.c11, this.c02, this.c30, this.c21, this.c12, this.c03);
		}
		
	}
}
