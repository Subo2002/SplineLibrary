using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static DrawLibrary.MathHelper;

namespace DrawLibrary
{
	//idea is do it all again, but now make the gradients floats
	//points are still ints though obviously
	public class Spline
	{
		private SplinePoint[] points = new SplinePoint[10];
		private int noPoints;

		public CubicCurveSegment[] curves = new CubicCurveSegment[30];
		public int noCurves;


		public Spline AddPoint(Vector2I point, float gradient)
		{
			SplinePoint lastPoint = points[noPoints - 1];
			Vector2I lastPosition = lastPoint.point;
			float lastGradient = lastPoint.gradient;
			bool vertical = /*lastPosition.X <= point.X*/false;
			Vector2I pointBeforeAltered = point;
			float gradientBeforeAltered = gradient;
			
			if (vertical)
			{
				(lastPosition.Y, lastPosition.X) = (lastPosition.X, lastPosition.Y);
				(point.Y, point.X) = (point.X, point.Y);
				lastGradient = 1 / lastGradient;
				gradient = 1 / gradient;
			}
			int alpha = ComputeAlpha(lastPosition, point, lastGradient);
			int beta = -ComputeAlpha(lastPosition, point, gradient);

			CubicCurveSegment initialCurve = new CubicCurveSegment(lastPosition, point, alpha, beta, vertical);
			List<CubicCurveSegment> parts = CubicCurveSegment.SubdivideByTurningPoints(initialCurve);
			foreach (CubicCurveSegment curve in parts)
			{
				curves[noCurves++] = curve;
			}

			points[noPoints++] = new SplinePoint(pointBeforeAltered, gradientBeforeAltered);

			return this;
		}

		public Spline(Vector2I point1, Vector2I point2, float gradient1, float gradient2)
		{
			bool vertical = /*point1.X >= point2.X*/false;
			if (vertical)
			{
				(point1.Y, point1.X) = (point1.X, point1.Y);
				(point2.Y, point2.X) = (point2.X, point2.Y);
				gradient1 = 1 / gradient1;
				gradient2 = 1 / gradient2;
			}
				
			int alpha = ComputeAlpha(point1, point2, gradient1);
			int beta = -ComputeAlpha(point1, point2, gradient2);

			CubicCurveSegment initialCurve = new CubicCurveSegment(point1, point2, alpha, beta, vertical);
			List<CubicCurveSegment> parts = CubicCurveSegment.SubdivideByTurningPoints(initialCurve);
			foreach (CubicCurveSegment curve in parts)
			{
				curves[noCurves++] = curve;
			}

			points[noPoints++] = new SplinePoint(point1, gradient1);
			points[noPoints++] = new SplinePoint(point2, gradient2);
		}

		private int ComputeAlpha(Vector2I point0, Vector2I point1, float gradient)
			=> (int)(gradient * (point1.X - point0.X) - (point1.Y - point0.Y));

		private static int maxNoPixels = 200;
		public Vector2I[] pixelPoints = new Vector2I[maxNoPixels];
		public int noPixelPoints;

		public void ComputePixelPoints()
		{
			noPixelPoints = 0;
			ref CubicCurveSegment curve = ref curves[0];
			int x0, x1, y0, y1;
			for (int i = 0; i < noCurves; i++)
			{
				curve = ref curves[i];
				x0 = curve.startPoint.X;
				x1 = curve.endPoint.X;
				y0 = curve.startPoint.Y;
				y1 = curve.endPoint.Y;
				//GD.Print("curve goes between: " + curve.startPoint + ", and: " + curve.endPoint);
				switch (x1 >= x0, y1 >= y0)
				{
					case (true, true):
						ComputePixelPoints_XY(curve);
						break;
					case (true, false):
						ComputePixelPoints_X(curve);
						break;
					case (false, true):
						ComputePixelPoints_Y(curve);
						break;
					case (false, false):
						ComputePixelPoints_(curve);
						break;
				}
			}
				
		}

		//bresenhams algorithm
		public void ComputePixelPoints_Monotonic(CubicCurveSegment curve)
		{
			int a = curve.a;
			int b = curve.b;
			int c = curve.c;
			int d = curve.d;
			int e_ = curve.e;

			int x0 = curve.startPoint.X;
			int y0 = curve.startPoint.Y;
			int x1 = curve.endPoint.X;
			int y1 = curve.endPoint.Y;

			//GD.Print("Curve points are, start: (" + x0 + ", " + y0 + "), end : (" + x1 + ", " + y1 + ")");

			int sx = 1;
			int sy = y1 >= y0 ? 1 : -1;

			int dxxx = 6 * a;
			int dxx = 6 * a + 2 * b;
			int dx = a + b + c;

			int dy = sy * d;

			int e = dx + dy + e_;

			int x = x0;
			int y = y0;
			ref Vector2I newPoint = ref pixelPoints[0];
			int e2;

			while (x != x1 || y != y1)
			{
				GD.Print("e: " + e + ", dx: " + dx + ", dy: " + dy + ", sx: " + sx + ", sy: " + sy);
				if (noPixelPoints == maxNoPixels)
				{
					GD.Print("hit max no pixels");
					break;
				}

				newPoint = ref pixelPoints[noPixelPoints++]; 
				newPoint.X = x;
				newPoint.Y = y;

				e2 = 2 * e;

				if (e2 >= dy)
				{
					y += sy;
					e += dy;
				}
				if (e2 <= dx)
				{
					x += sx;

					dx += dxx;
					e += dx;
					dxx += dxxx;
				}
			}
			//GD.Print("noPixels: " + noPixelPoints);
		}

		public void ComputePixelPoints_XY(CubicCurveSegment curve)
		{
			int a = curve.a;
			int b = curve.b;
			int c = curve.c;
			int d = curve.d;
			int e_ = curve.e;

			int x0 = curve.startPoint.X;
			int y0 = curve.startPoint.Y;
			int x1 = curve.endPoint.X;
			int y1 = curve.endPoint.Y;

			//GD.Print("Curve points are, start: (" + x0 + ", " + y0 + "), end : (" + x1 + ", " + y1 + ")");


			int dxxx = 6 * a;
			int dxx = 6 * a + 2 * b;
			int dx = a + b + c;

			int dy = d;

			int e = dx + dy + e_;

			int x = x0;
			int y = y0;
			ref Vector2I newPoint = ref pixelPoints[0];
			int e2;

			while (x != x1 || y != y1)
			{
				//GD.Print("e: " + e + ", dx: " + dx + ", dy: " + dy + ", sx: " + 1 + ", sy: " + 1);
				if (noPixelPoints == maxNoPixels)
				{
					GD.Print("hit max no pixels");
					break;
				}

				newPoint = ref pixelPoints[noPixelPoints++];
				newPoint.X = x;
				newPoint.Y = y;

				e2 = 2 * e;

				if (e2 >= dy)
				{
					y++;

					e += dy;
				}
				if (e2 <= dx)
				{
					x++;

					dx += dxx;
					e += dx;
					dxx += dxxx;
				}
			}
			//GD.Print("noPixels: " + noPixelPoints);
		}

		public void ComputePixelPoints_X(CubicCurveSegment curve)
		{
			int a = curve.a;
			int b = curve.b;
			int c = curve.c;
			int d = curve.d;
			int e_ = curve.e;

			int x0 = curve.startPoint.X;
			int y0 = curve.startPoint.Y;
			int x1 = curve.endPoint.X;
			int y1 = curve.endPoint.Y;

			//GD.Print("Curve points are, start: (" + x0 + ", " + y0 + "), end : (" + x1 + ", " + y1 + ")");

			int dxxx = 6 * a;
			int dxx = 6 * a + 2 * b;
			int dx = a + b + c;

			int dy = -d;

			int e = dx + dy + e_;

			int x = x0;
			int y = y0;
			ref Vector2I newPoint = ref pixelPoints[0];
			int e2;

			while (x != x1 || y != y1)
			{
				//GD.Print("e: " + e + ", dx: " + dx + ", dy: " + dy + ", sx: " + 1 + ", sy: " + -1);
				if (noPixelPoints == maxNoPixels)
				{
					GD.Print("hit max no pixels");
					break;
				}

				newPoint = ref pixelPoints[noPixelPoints++];
				newPoint.X = x;
				newPoint.Y = y;

				e2 = 2 * e;

				if (e2 <= dy)
				{
					y--;
					e += dy;
				}
				if (e2 >= dx)
				{
					x++;

					dx += dxx;
					e += dx;
					dxx += dxxx;
				}
			}
			//GD.Print("noPixels: " + noPixelPoints);
		}

		public void ComputePixelPoints_Y(CubicCurveSegment curve)
		{
			int a = curve.a;
			int b = curve.b;
			int c = curve.c;
			int d = curve.d;
			int e_ = curve.e;

			int x0 = curve.startPoint.X;
			int y0 = curve.startPoint.Y;
			int x1 = curve.endPoint.X;
			int y1 = curve.endPoint.Y;

			//GD.Print("Curve points are, start: (" + x0 + ", " + y0 + "), end : (" + x1 + ", " + y1 + ")");

			int dxxx = -6 * a;
			int dxx = -6 * a + 2 * b;
			int dx = -a + b - c;

			int dy = d;

			int e = dx + dy + e_;

			int x = x0;
			int y = y0;
			ref Vector2I newPoint = ref pixelPoints[0];
			int e2;

			while (x != x1 || y != y1)
			{
				//GD.Print("e: " + e + ", dx: " + dx + ", dy: " + dy + ", sx: " + -1 + ", sy: " + 1);
				if (noPixelPoints == maxNoPixels)
				{
					GD.Print("hit max no pixels");
					break;
				}

				newPoint = ref pixelPoints[noPixelPoints++];
				newPoint.X = x;
				newPoint.Y = y;

				e2 = 2 * e;

				if (e2 <= dy)
				{
					y++;
					e += dy;
				}
				if (e2 >= dx)
				{
					x--;

					dx += dxx;
					e += dx;
					dxx += dxxx;
				}
			}
			//GD.Print("noPixels: " + noPixelPoints);
		}

		public void ComputePixelPoints_(CubicCurveSegment curve)
		{
			int a = curve.a;
			int b = curve.b;
			int c = curve.c;
			int d = curve.d;
			int e_ = curve.e;

			int x0 = curve.startPoint.X;
			int y0 = curve.startPoint.Y;
			int x1 = curve.endPoint.X;
			int y1 = curve.endPoint.Y;

			//GD.Print("Curve points are, start: (" + x0 + ", " + y0 + "), end : (" + x1 + ", " + y1 + ")");

			int dxxx = -6 * a;
			int dxx = -6 * a + 2 * b;
			int dx = -a + b - c;

			int dy = -d;

			int e = dx + dy + e_;

			int x = x0;
			int y = y0;
			ref Vector2I newPoint = ref pixelPoints[0];
			int e2;

			while (x != x1 || y != y1)
			{
				//GD.Print("e: " + e + ", dx: " + dx + ", dy: " + dy + ", sx: " + -1 + ", sy: " + -1);
				if (noPixelPoints == maxNoPixels)
				{
					GD.Print("hit max no pixels");
					break;
				}

				newPoint = ref pixelPoints[noPixelPoints++];
				newPoint.X = x;
				newPoint.Y = y;

				e2 = 2 * e;

				if (e2 >= dy)
				{
					y--;
					e += dy;
				}
				if (e2 <= dx)
				{
					x--;

					dx += dxx;
					e += dx;
					dxx += dxxx;
				}
			}
			//GD.Print("noPixels: " + noPixelPoints);
		}

		private const int maxNoLazyPixels = 10000;
		public Vector2I[] lazyDrawPoints = new Vector2I[maxNoLazyPixels];
		public int nolazyDrawPoints;

		public void LazyDraw()
		{
			CubicCurveSegment curve;
			float y;
			float xx;
			float xxx;
			for (int i = 0; i < noCurves; i++)
			{
				curve = curves[i];
				float dx = 0.5f * (curve.endPoint.X - curve.startPoint.X >= 0 ? 1 : -1);
				for (float x = 0; x < curve.endPoint.X - curve.startPoint.X; x += dx)
				{
					if (nolazyDrawPoints == maxNoLazyPixels)
						break;
					xx = x * x;
					xxx = xx * x;
					y = -(curve.a * xxx + curve.b * xx + curve.c * x + curve.e - curve.d * curve.startPoint.Y) / curve.d;
					lazyDrawPoints[nolazyDrawPoints++] = new Vector2I(Round(x + curve.startPoint.X), Round(y));
				}
			}
		}

		/*
		public List<Vector2I> ComputePixelPoints()
		{
			List<Vector2I> _points = new List<Vector2I>();
			noPixelPoints = 0;
			CubicCurveSegment curveInitial = new CubicCurveSegment(points[0], points[1], (int)alphas[0], (int)betas[0], denominator);
			List<CubicCurveSegment> parts = CubicCurveSegment.SubdivideByTurningPoints(curveInitial);
			foreach (CubicCurveSegment curve in parts)
			{
				_points.Add(curve.endPoint);
				ComputePixelPoints_Monotonic(curve);
			}
			return _points;
		}
		*/
	}

	public readonly struct SplinePoint
	{
		public readonly Vector2I point;
		public readonly float gradient;

		public SplinePoint(Vector2I point, float gradient)
		{
			this.point = point;
			this.gradient = gradient;
		}
	}

	public readonly struct CubicCurveSegment
	{
		public readonly int a;
		public readonly int b;
		public readonly int c;
		public readonly int d;
		public readonly int e;

		public readonly Vector2I startPoint;
		public readonly Vector2I endPoint;

		public readonly bool vertical;

		public CubicCurveSegment(Vector2I point1, Vector2I point2, int alpha, int beta, bool vertical = false)
		{
			int l = point2.X - point1.X;
			a = alpha - beta;
			b = beta - 2 * alpha;
			c = point2.Y - point1.Y + alpha;
			int ll = l * l;
			int lll = ll * l;
			b *= l;
			c *= ll;
			d = -lll;
			e = 0;
			startPoint = point1;
			endPoint = point2;
			this.vertical = vertical;
		   
		}

		//reparamaterize curve
		private CubicCurveSegment(CubicCurveSegment curve, Vector2I startPoint, Vector2I endPoint)
		{
			int a = curve.a;
			int b = curve.b;
			int c = curve.c;
			int d = curve.d;
			int e = curve.e;
			int u = startPoint.X - curve.startPoint.X;
			this.vertical = curve.vertical;
			if (u == 0) //then basically just copy
			{
				this.a = a;
				this.b = b;
				this.c = c;
				this.d = d;
				this.e = e;
				this.startPoint = startPoint;
				this.endPoint = endPoint;
			}
			else
			{
				int uu = u * u;
				int uuu = uu * u;
				this.a = a;
				this.b = b + 3 * a * u;
				this.c = c + 2 * b * u + 3 * a * uu;
				this.d = d;
				this.e = a * uuu + b * uu + c * u + d * (startPoint.Y - curve.startPoint.Y) + e;
				this.startPoint = startPoint;
				this.endPoint = endPoint;
			}
		}

		public static List<CubicCurveSegment> SubdivideByTurningPoints(CubicCurveSegment curve)
		{
			long a = curve.a;
			long b = curve.b;
			long c = curve.c;
			int x0 = curve.startPoint.X;
			int x1 = curve.endPoint.X;


			List<CubicCurveSegment> parts = new List<CubicCurveSegment>(3);

			long disc = (b * b) - (3 * a * c);
			//GD.Print("(a, b, c, disc): (" + a + ", " + b + ", " + c + ", " + disc);
			switch (disc)
			{
				case < 0:
					//GD.Print("No turing points, points are, start: " + curve.startPoint + ", end: " + curve.endPoint);
					parts.Add(curve);
					break;
				case 0:
					double x = -b / (double)(3 * a) + x0;
					if ((x0 <= x1) && (x < x0 || x > x1))
						break;
					if ((x1 <= x0) && (x < x1 || x > x0))
						break;
					double y = curve.ComputeY(x);
					Vector2I turningPoint = new Vector2I((int)x, (int)y);

					//GD.Print("1 turning point case 1, " + curve.startPoint + ", " + turningPoint + ", " + curve.endPoint);
					parts.Add(new CubicCurveSegment(curve, curve.startPoint, turningPoint));
					parts.Add(new CubicCurveSegment(curve, turningPoint, curve.endPoint));

					break;
				case > 0:
					double sqrtDisc = Math.Sqrt(disc);

					double _x0 = (-b - sqrtDisc) / (3 * a) + x0;
					double _x1 = (-b + sqrtDisc) / (3 * a) + x0;

					if (x0 <= x1 && _x0 > _x1)
						(_x1, _x0) = (_x0, _x1);
					else if (x0 >= x1 && _x0 < _x1)
						(_x1, _x0) = (_x0, _x1);

					bool _x0_valid = (x0 <= x1 && _x0 >= x0 && _x0 <= x1) || (x1 <= x0 && _x0 >= x1 && _x0 <= x0);
					bool _x1_valid = (x0 <= x1 && _x1 >= x0 && _x1 <= x1) || (x1 <= x0 && _x1 >= x1 && _x1 <= x0);

					double y0;
					double y1;
					Vector2I turningPoint0;
					Vector2I turningPoint1;

					//GD.Print("turningPoint1.X: " + _x0 + ", turningPoint2.X: " + _x1);

					switch (_x0_valid, _x1_valid)
					{
						case (true, true):
							y0 = curve.ComputeY(_x0);
							y1 = curve.ComputeY(_x1);
							turningPoint0 = new Vector2I(Round(_x0), Round(y0));
							turningPoint1 = new Vector2I(Round(_x1), Round(y1));
							parts.Add(new CubicCurveSegment(curve, curve.startPoint, turningPoint0));
							parts.Add(new CubicCurveSegment(curve, turningPoint0, turningPoint1));
							parts.Add(new CubicCurveSegment(curve, turningPoint1, curve.endPoint));
							//GD.Print("2 turning point, " + curve.startPoint + ", " + turningPoint0 + ", " + turningPoint1 + ", " + curve.endPoint);
							//GD.Print("turningPoint1: " + turningPoint0 + ", turningPoint2: " + turningPoint1);
							break;
						case (true, false):
							y0 = curve.ComputeY((float)_x0);
							turningPoint0 = new Vector2I(Round(_x0), Round(y0));
							parts.Add(new CubicCurveSegment(curve, curve.startPoint, turningPoint0));
							parts.Add(new CubicCurveSegment(curve, turningPoint0, curve.endPoint));
							//GD.Print("1 turning point case 2, " + curve.startPoint + ", " + turningPoint0 + ", " + curve.endPoint);
							//GD.Print("turningPoint: " + turningPoint0);
							break;
						case (false, true):
							y1 = curve.ComputeY((float)_x1);
							turningPoint1 = new Vector2I(Round(_x1), Round(y1));
							parts.Add(new CubicCurveSegment(curve, curve.startPoint, turningPoint1));
							parts.Add(new CubicCurveSegment(curve, turningPoint1, curve.endPoint));
							//GD.Print("1 turning point case 3, " + curve.startPoint + ", " + turningPoint1 + ", " + curve.endPoint);
							//GD.Print("turningPoint: " + turningPoint1);
							break;
						case (false, false):
							parts.Add(curve);
							break;
					}
					break;
			}

			return parts;
		}

		private static int Round(double X)
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

		public readonly double ComputeY(double x)
		{
			x -= startPoint.X; //reparametrize
			double xx = x * x;
			double xxx = xx * x;
			return -(a * xxx + b * xx + c * x - d * startPoint.Y + e) / d;
		}


	}

	


}
