using Godot;
using System;
using System.Security.Cryptography;
using static DrawLibrary.MathHelper;

namespace DrawLibrary
{
    public struct ParametricCubicCurve 
    {
        public static bool IsApproximateStraightLine(
            HermiteSpline hermite,
            out Line line
            )
        {
            (Vector2I start, Vector2I startTangent, Vector2I end, Vector2I endTangent) = hermite;

            startTangent = startTangent.Abs();
            endTangent = endTangent.Abs();

            Vector2I difference = (end - start).Abs();
            int minChangeRequiredToGetFromPointAToPointB = Math.Max(difference.X, difference.Y);
            
            int changedCausedByTangents = Math.Max(Math.Max(startTangent.X, endTangent.X), Math.Max(startTangent.Y, endTangent.Y));

            double relativeAmountChangedByTangents = changedCausedByTangents / (double)minChangeRequiredToGetFromPointAToPointB;

            //i.e. the change they cause if negligible (like one one-hundreth) then it's pretty much a straight line
            GD.Print("relativeChangeCauseByTangents: " + relativeAmountChangedByTangents);
            if (relativeAmountChangedByTangents <= 0.02)
            {
                line = new Line(start, end);
                return true;
            }
            else
            {
                line = new();
                return false;
            }
        }

        public static ParametricCubicCurve ConstructCubicHermiteCurve(in HermiteSpline hermite)
        {
            //GD.Print("p0: " + start + ", p1: " + end + ", m0: " + startTangent + ", m1: " + endTangent);

            (Vector2I start, Vector2I startTangent, Vector2I end, Vector2I endTangent) = hermite;

            Vector2I cubic = 2 * start + startTangent - 2 * end + endTangent;
            Vector2I quadratic = -3 * start - 2 * startTangent + 3 * end - endTangent;
            Vector2I linear = startTangent;
            Vector2I constant = new(); //move curve to origin to simplify computations

            //GD.Print("x0: " + start.X + ", x1 - x0: " + (end.X - start.X) + " x poly: " + cubic.X + ", " + quadratic.X + ", " + linear.X + ", " + constant.X);
            //GD.Print(" y poly: " + cubic.Y + ", " + quadratic.Y + ", " + linear.Y + ", " + constant.Y);

            return new ParametricCubicCurve()
            {
                a = cubic.X,
                b = quadratic.X,
                c = linear.X,
                d = constant.X,

                e = cubic.Y,
                f = quadratic.Y,
                g = linear.Y,
                h = constant.Y,

                start = start,
                end = end,
            };
        }

        public int a;
        public int b;
        public int c;
        public int d;

        public int e;
        public int f;
        public int g;
        public int h;

        public Vector2I start;
        public Vector2I end;

        public static Span<Vector2I> ComputePixels(in ParametricCubicCurve curve, Span<Vector2I> pixels)
        {
            int i = 0;
            for (double t = 0; t <= 1; t += 0.01)
            {
                pixels[i++] = Round(new Vector2(
                    (float)(curve.a * t * t * t + curve.b * t * t + curve.c *  t + curve.d),
                    (float)(curve.e * t * t * t + curve.f * t * t + curve.g * t + curve.h)
                    ) + curve.start);
            }
            return pixels.Slice(0, i);
        }
    }

    public struct HermiteSpline
    {
        Vector2I start;
        Vector2I startTangent;
        Vector2I end;
        Vector2I endTangent;

        public HermiteSpline(Vector2I start, Vector2I startTangent, Vector2I end, Vector2I endTangent)
        {
            this.start = start;
            this.startTangent = startTangent;
            this.end = end;
            this.endTangent = endTangent;   
        }

        public readonly void Deconstruct(out Vector2I start, out Vector2I startTangent, out Vector2I end, out Vector2I endTangent)
            => (start, startTangent, end, endTangent) = (this.start, this.startTangent, this.end, this.endTangent);

    }
}
