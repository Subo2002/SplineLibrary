using Godot;
using System;
using DrawLibrary;
using static DrawLibrary.MathHelper;

public partial class Draw_Spline : Node2D
{
	
	[Export]
	Sprite2D sprite;
	[Export]
	VBoxContainer box;
	[Export]
	LineEdit gradient;
	[Export]
	CheckBox drawTangetLine;
	[Export]
	private float cursorAcceptedError;

	private bool justAddedNewPoint;
	private float timerCount;
	public override void _Input(InputEvent @event)
	{
		if (@event.IsAction("Select") & !mouseInBox)
		{

		}
		if (@event.IsAction("AddNewPoint"))
		{
			if (!justAddedNewPoint)
			{
				Vector2 _position = GetGlobalMousePosition();
				Vector2I position = new Vector2I(Round(_position.X), Round(_position.Y));
				//GD.Print("Position: " + position);
				points[noPoints] = position;
				gradients[noPoints] = 1;
				noPoints++;
				justAddedNewPoint = true;
				updateCurve = true;
				updateGraphic = true;
			}
			else if (@event.IsReleased())
				justAddedNewPoint = false;
		}
		//just see if there is a point nearby?
		else if (@event.IsAction("Select") & !mouseInBox) 
		{
			Vector2 _position = GetGlobalMousePosition();
			Vector2I position = new Vector2I(Round(_position.X), Round(_position.Y));
			if (selectedPointIndex == -1)
			{
				Vector2I point;
				for (int i = 0; i < noPoints; i++)
				{
					point = points[i];
					if (Math.Abs(position.X - point.X) < cursorAcceptedError && Math.Abs(position.Y - point.Y) < cursorAcceptedError)
					{
						updateGraphic = true;
						selectedPointIndex = i;
					}
				}
				//display gradient guy
				//so like a line
				//and also a point
			}
			else if (selectedPointIndex != -1)
			{
				bool selectedNothing = true;
				Vector2I point;
				for (int i = 0; i < noPoints; i++)
				{
					point = points[i];
					if (Math.Abs(position.X - point.X) < cursorAcceptedError && Math.Abs(position.Y - point.Y) < cursorAcceptedError)
					{
						selectedNothing = false;
						updateGraphic = true;
						selectedPointIndex = i;
					}
				}
				if (selectedNothing)
				{
					updateGraphic = true;
					box.Visible = false;

					selectedPointIndex = -1;
				}

			}
		}
	}
	private int selectedPointIndex = -1;
	private bool updateCurve;

	public override void _Process(double delta)
	{
		if (timerCount != -1)
			timerCount += (float)delta;

		if (!updateGraphic)
			return;

		updateGraphic = false;
		image.Fill(Color.Color8(255, 255, 255));
			
		if (updateCurve)
		{
			if (noPoints >= 2)
			{
				DrawCurve();
			}
				
			DrawPoints();
		}
		if (selectedPointIndex != -1)
		{
			box.Visible = true;
			gradient.Text = "" + gradients[selectedPointIndex];
			image.SetPixelv(points[selectedPointIndex], Color.Color8(0, 0, 0));

			if (drawTangetLine.ButtonPressed)
			{
				Vector2I point = points[selectedPointIndex];
				Vector2 gradient = new Vector2(1, gradients[selectedPointIndex]);
				int size = Math.Max((int)(1 / gradients[selectedPointIndex]), 1);
				Line tangentLine = new Line(point - (Vector2I)(10 * size * gradient), point + (Vector2I)(10 * size * gradient));
				Span<Vector2I> tangentLinePixels = Line.ComputePixels(tangentLine, PixelPoints);
				//GD.Print("noPoints for tangentLine: " + tangentLine.noPoints);
				
				for (int i = 0; i < tangentLinePixels.Length; i++)
					image.SetPixelv(tangentLinePixels[i], Color.Color8(255, 0, 255));
			}
		}
		
			
		sprite.Texture = ImageTexture.CreateFromImage(image);

	}

	private const int maxNoPoints = 10;
	private Vector2I[] points = new Vector2I[maxNoPoints];
	private int noPoints;
	private float[] gradients = new float[maxNoPoints];
	private bool updateGraphic;
	private Image image;
	private Vector2I center;
	

	public override void _Ready()
	{
		Vector2 size = GetViewportRect().Size;
		GD.Print("size: " + size);
		int width = Round(size.X);
		int length = Round(size.Y);
		image = Image.Create(width, length, false, Image.Format.Rgba8);
		image.Fill(Color.Color8(255, 255, 255));
		sprite.Position = size / 2;
		center = new Vector2I(width / 2, length / 2);
		sprite.Texture = ImageTexture.CreateFromImage(image);

		box.Visible = false;
	}
	[Export]
	private bool lazyDraw;

	Vector2I[] PixelPoints = new Vector2I[1000];
	Vector2I[] PixelPointsLazy = new Vector2I[1000];

	private void DrawCurve()
	{
		HermiteSpline circleApproxHermite = new HermiteSpline(
			center + new Vector2I(0, -60), new Vector2I(60, 0), 
			center + new Vector2I(100, 0), new Vector2I(0, 100));
		HermiteSpline betweenTwoPointsHermite = new HermiteSpline(points[0], new Vector2I(1, 1), points[1], new Vector2I(1, 1));
		HermiteSpline hermite = circleApproxHermite;
		if (ParametricCubicCurve.IsApproximateStraightLine(circleApproxHermite, out Line line))
		{
			Span<Vector2I> linePixels = Line.ComputePixels(line, PixelPoints);

			AddPixelsToImage(image, linePixels, Color.Color8(255, 0, 0));
		}
		else
		{
			ParametricCubicCurve spline = ParametricCubicCurve.ConstructCubicHermiteCurve(hermite);
			CubicCurve curve = CubicCurve.ConstructCubicCurveFromParametricForm(spline);
			Span<Vector2I> pixelPointsLazy = ParametricCubicCurve.ComputePixels(spline, new Span<Vector2I>(PixelPointsLazy));
			Span<Vector2I> pixelPoints = CubicCurve.ComputePixels(curve, new Span<Vector2I>(PixelPoints));

			ThirdImpact(image, curve);

			AddPixelsToImage(image, pixelPointsLazy, Color.Color8(0, 255, 0));

			AddPixelsToImage(image, pixelPoints, Color.Color8(0, 0, 255));

		}
		

		//Spline spline = new(points[0], points[1], gradients[0], gradients[1]);
		//spline.ComputePixelPoints();
		/*
		for (int i = 2; i < noPoints; i++)
			spline.AddPoint(points[i], gradients[i]);
		*/

		if (lazyDraw)
		{
			/*
			spline.LazyDraw();

			Vector2I[] lazyPoints = spline.lazyDrawPoints;
			for (int i = 0; i < spline.nolazyDrawPoints; i++)
				image.SetPixelv(lazyPoints[i], Color.Color8(0, 0, 255));
			*/
		}

		//spline.ComputePixelPoints();

		//Vector2I[] pixelPoints2 = spline.pixelPoints;
		/*
		for (int i = 0; i < pixelPointsLazy.Length; i++)
			image.SetPixelv(pixelPointsLazy[i], Color.Color8(0, 0, 255));
		*/

		
		
		

		
		

		/*
		ref CubicCurveSegment curve = ref spline.curves[0];
		Vector2I point = new Vector2I(0, 0);
		for (int i = 0; i < spline.noCurves; i++)
		{
			curve = ref spline.curves[i];
			point.X = curve.vertical ? curve.startPoint.Y : curve.startPoint.X;
			point.Y = curve.vertical ? curve.startPoint.X : curve.startPoint.Y;
			image.SetPixelv(point, Color.Color8(0, 255, 0));

		}
		*/
	}

	private static void ThirdImpact(Image image, CubicCurve curve)
	{
		Color red = Color.Color8(1, 0, 0);
		Color white = Color.Color8(1, 1, 1);

		long error;
		byte t;
		Color color;
		int high = int.MaxValue;
		for (int i = 0; i < image.GetHeight(); i++)
		{
			for (int j = 0; j < image.GetWidth(); j++)
			{
				error = CubicCurve.ComputeErrorAt(curve, j - curve.start.X, i - curve.start.Y);
				error = Math.Clamp(Math.Abs(error) / 1000, 0, high);
				t = (byte)((error / (double)high) * 255);
				GD.Print("error, :" + error + ", high: " + high + ", t: " + t);
				color = (255 - t) * white + t * red;
				image.SetPixel(j, i, color);
			}
		}
	}
	

	private void DrawPoints()
	{
		for (int i = 0; i < noPoints; i++)
			image.SetPixelv(points[i], Color.Color8(0, 255, 0));
	}

	private void UpdateGradient(string text)
	{
		if (float.TryParse(text, out float value))
		{
			if (float.IsNaN(value))
				value = 0;
			else if (float.IsNegativeInfinity(value))
				value = -1;
			else if (float.IsPositiveInfinity(value))
				value = 1;
			updateGraphic = true;
			updateCurve = true;
			gradients[selectedPointIndex] = value; 
		}
		else
		{
			gradient.Text = "" + gradients[selectedPointIndex];
		}
	}

	private bool mouseInBox;
	private void MouseInBox()
	{
		mouseInBox = !mouseInBox;
		//GD.Print("MouseInBox: " + mouseInBox);
	}

}
