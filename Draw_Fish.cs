using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using DrawLibrary;
using static DrawLibrary.MathHelper;

namespace SplineDemo
{


	public partial class Draw_Fish : Node2D
	{
		[Export]
		Sprite2D sprite;

		const int noSpinePoints = 10;
		Disk[] disks = new Disk[noSpinePoints];
		Vector2[] spinePoints = new Vector2[noSpinePoints];

		int[] spineSizes = new int[noSpinePoints]
		{
			 11,
			 5,
			 9,
			 8,
			 7,
			 6,
			 5,
			 4,
			 3,
			 2,
		};

		float[] spineLengths = new float[noSpinePoints - 1]
		{
			15, 10, 9, 8, 7, 6, 5, 4, 3
		};

		ImageTexture texture;
		Image image;
		Vector2I center;
		Vector2I diskCenter;

		Color white = Color.Color8(255, 255, 255);
		Color orange = Color.Color8(255, 100, 100);

		public override void _Ready()
		{
			Vector2 size = GetViewportRect().Size;
			GD.Print("size: " + size);
			int width = Round(size.X);
			int length = Round(size.Y);
			image = Image.Create(width, length, false, Image.Format.Rgba8);
			image.Fill(Color.Color8(255, 255, 255));
			sprite.Position = size / 2;
			centerExact = new Vector2(width / (float)2, length / (float)2);
			center = (Vector2I)centerExact;
			texture = ImageTexture.CreateFromImage(image);
			sprite.Texture = texture;

			//initalize disks
			Disk disk;
			for (int i = 0; i < noSpinePoints; i++)
			{
				disk = new();
				disks[i] = disk;
				disk.radius = spineSizes[i];
			}
			Vector2I offset = new(0, 10);
			Vector2I diskCenter = center;
			for (int i = 0; i < noSpinePoints; i++)
			{
				spinePoints[i] = diskCenter;
				disks[i].center = diskCenter;
				diskCenter += offset;
			}

		}

		Vector2 centerExact;
		double t;
		const double tau = Math.PI * 2;
		Vector2 pos;
		const float movementRadius = 100;
		const float speed = 2;
		Vector2 error;

		public override void _Process(double delta)
		{
			//GD.Print("fps: " + Engine.GetFramesPerSecond());

			//head move code
			//move in circle
			/*
			t += delta * speed;
			if (t >= tau)
				t -= tau;

			pos.X = (float)(movementRadius * Math.Cos(t));
			pos.Y = (float)(movementRadius * Math.Sin(t));

			pos += centerExact;
			*/

			//follow mouse
			Vector2 mousePos = GetGlobalMousePosition();
			Vector2 direction = (mousePos - disks[0].center).Normalized();
			pos = disks[0].center + speed * direction;

			Round(in pos, ref disks[0].center);

			spinePoints[0] = pos;

			//clear
			image.Fill(Color.Color8(255, 255, 255));

			//spine code
			ref Vector2 lastSpinePoint = ref spinePoints[0];
			ref Vector2 spinePoint = ref spinePoints[1];
			/*
			for (int i = 1; i < noSpinePoints; i++)
			{
				spinePoint = ref spinePoints[i];
				spinePoint = (spinePoint - lastSpinePoint).Normalized() * spineLengths[i-1] + lastSpinePoint;
				lastSpinePoint = ref spinePoint;
				Round(in spinePoint, ref disks[i].center);
			}
			*/

			/*
			for (int i = 0; i < noSpinePoints; i++)
			{
				disks[i].ComputePixelPoints();
			}

			Vector2I[] pixelPoints;
			for (int i = 0; i < noSpinePoints; i++)
			{
				pixelPoints = disks[i].pixelPoints;
				for (int j = 0; j < disks[i].noPixelPoints; j++)
				{
					image.SetPixelv(pixelPoints[j], orange);
				}
			}
			*/

			Vector2 normal1 = new Vector2(-direction.Y, direction.X);
			Vector2 direction2 = (lastSpinePoint - spinePoint).Normalized();
			Vector2 normal2 = new Vector2(-direction.Y, direction.X);
			spinePoint = -direction2 * spineLengths[0] + lastSpinePoint;
			Round(in spinePoint, ref disks[1].center);

			Spline spline1 = new Spline(
				Round(lastSpinePoint + normal1 * spineSizes[0]),
				Round(spinePoint + normal2 * spineSizes[1]),
				normal1.Y/normal1.X,
				normal2.Y/normal2.X
				);

			Spline spline2 = new Spline(
				Round(lastSpinePoint - normal1 * spineSizes[0]),
				Round(spinePoint - normal2 * spineSizes[1]),
				normal1.Y / normal1.X,
				normal2.Y / normal2.X
				);

			spline1.ComputePixelPoints();
			spline2.ComputePixelPoints();
			disks[0].ComputePixelPoints();
			disks[1].ComputePixelPoints();

			Vector2I[] pixelPoints;
			int noPixelPoints;
			pixelPoints = spline1.pixelPoints;
			noPixelPoints = spline1.noPixelPoints;
			for (int i = 0; i < noPixelPoints; i++)
			{
				image.SetPixelv(pixelPoints[i], orange);
			}
			pixelPoints = spline2.pixelPoints;
			noPixelPoints = spline2.noPixelPoints;
			for (int i = 0; i < noPixelPoints; i++)
			{
				image.SetPixelv(pixelPoints[i], orange);
			}
			pixelPoints = disks[0].pixelPoints;
			noPixelPoints = disks[0].noPixelPoints;
			for (int i = 0; i < noPixelPoints; i++)
			{
				image.SetPixelv(pixelPoints[i], orange);
			}
			pixelPoints = disks[1].pixelPoints;
			noPixelPoints = disks[1].noPixelPoints;
			for (int i = 0; i < noPixelPoints; i++)
			{
				image.SetPixelv(pixelPoints[i], orange);
			}

			texture.Update(image);
		}

		//just colors in as organge atm
		private void DrawBetweenTwoLines(Image image, Span<Vector2I> line1, Span<Vector2I> line2)
		{
			bool line1Decreasing = line1[0].X > line1[^1].X;
			bool line2Decreasing = line2[0].X > line2[^1].X;

			int s1 = line1Decreasing ? 1 : -1;
			int s2 = line2Decreasing ? 1 : -1;

			(int currentTopIndex, int lastTopIndex) = line1Decreasing ? (0, line1.Length - 1) : (line1.Length - 1, 0);
			(int currentBottomIndex, int lastBottomIndex) = line2Decreasing ? (0, line2.Length - 1) : (line2.Length - 1, 0);

			int currentX = Math.Min(line1[currentTopIndex].X, line2[currentBottomIndex].X);
			int finalX = Math.Max(line1[lastTopIndex].X, line2[lastBottomIndex].X);
			int x;

			bool bad;

			int startY;
			int endY;

			int maxCount = 5000;
			int count = 0;
			while (currentX >= finalX)
			{
				if (count == maxCount)
				{
					GD.Print("Hit max pixel count in DrawBetweenTwoLines");
					return;
				}

				count++;
				//GD.Print("Current top: " + currentTopIndex + ", Current bottom: " + currentBottomIndex);
				bad = false;
				x = line1[currentTopIndex].X;
				if (x != currentX)
				{
					currentTopIndex += s1;
					bad = true;
				}
				x = line2[currentBottomIndex].X;
				if (x != currentX)
				{
					currentBottomIndex += s2;
					bad = true;
				}

				//GD.Print("CurrentX: " + currentX + ", topPixelX: " + topPixels[currentTopIndex].X + ", bottomPixelX: " + x);

				if (bad)
					continue;

				//GD.Print("Drawing");

				startY = line1[currentTopIndex].Y;
				endY = line2[currentBottomIndex].Y;
				if (startY > endY)
					(startY, endY) = (endY, startY);


				for (int j = startY; j <= endY; j++)
				{
					image.SetPixel(x, j, orange);
				}

				//image.SetPixelv(topPixels[i], Color.Color8(0, 0, 0));
				//image.SetPixelv(bottomPixels[i], Color.Color8(0, 0, 0));


				currentX--;
				currentTopIndex += s1;
				currentBottomIndex += s2;
			}
		}
	}
}
