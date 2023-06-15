using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using static System.Math;

namespace TestsSystems_HardnessTester
{
    internal static class FindShape
    {
        public static RotatedRect FindSqr(Mat image)
        {
            Mat src = image.Clone();
            Mat temp = src.Clone();
            CvInvoke.FastNlMeansDenoising(src, src, 30);
            CvInvoke.MedianBlur(src, src, 5);
            CvInvoke.BilateralFilter(src, temp, -1, 15, 15, Emgu.CV.CvEnum.BorderType.Constant);
            CvInvoke.Canny(src, temp, 3900, 6800, apertureSize: 7, l2Gradient: true);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(temp, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxNone);

            contours = DeleteContursEdge(contours, temp, 1);
            contours = DeleteSmallCntrs(contours);

            temp.SetTo(new MCvScalar(0));
            for (int i = 0; i < contours.Size; i++)
                CvInvoke.DrawContours(temp, contours, i, new MCvScalar(255));

            #region настройки
            int H = 100;
            int HY = temp.Height / H;
            int HX = temp.Width / H;
            double angleResolutio = PI / 90.0;
            double errorK = 2.5 * angleResolutio;
            double errorB = Min(temp.Height, temp.Width) * 0.05;
            #endregion

            #region находим линии
            VectorOfPointF linesVector = new VectorOfPointF();
            CvInvoke.HoughLines(temp, linesVector, 1, angleResolutio, 30, 2, 2);
            var linesList = new List<LineSegment2D>();
            for (var i = 0; i < linesVector.Size; i++)
            {
                var rho = linesVector[i].X;
                var theta = linesVector[i].Y;
                var pt1 = new System.Drawing.Point();
                var pt2 = new System.Drawing.Point();
                var a = Math.Cos(theta);
                var b = Math.Sin(theta);
                var x0 = a * rho;
                var y0 = b * rho;
                pt1.X = (int)Math.Round(x0 + temp.Width * 2 * (-b));
                pt1.Y = (int)Math.Round(y0 + temp.Width * 2 * (a));
                pt2.X = (int)Math.Round(x0 - temp.Width * 2 * (-b));
                pt2.Y = (int)Math.Round(y0 - temp.Width * 2 * (a));
                linesList.Add(new LineSegment2D(pt1, pt2));
            }
            var lines = linesList.ToArray();
            #endregion

            #region находим пары параллельных прямых
            var linesParallel = new List<(int, int/*,double,double*/)>();
            for (int i = 0; i < linesVector.Size; i++)
                for (int j = i + 1; j < linesVector.Size; j++)
                {
                    if ((Abs(Abs(linesVector[j].Y) - Abs(linesVector[i].Y)) <= errorK)
                        && (Abs(linesVector[j].X - linesVector[i].X) >= errorB))
                        linesParallel.Add((i, j/*, lineParam[i].Item1, lineParam[j].Item1*/));
                }
            #endregion

            #region находим перпендикулярные пары для параллельных пар
            var accumParallelandPerpendicular = new List<(int, int, int, int)>();
            for (int i = 0; i < linesParallel.Count; i++)
            {
                for (int j = i + 1; j < linesParallel.Count; j++)
                {
                    int i1 = linesParallel[i].Item1;
                    int j1 = linesParallel[i].Item2;
                    int i2 = linesParallel[j].Item1;
                    int j2 = linesParallel[j].Item2;
                    var k1 = (linesVector[i1].Y + linesVector[j1].Y) / 2.0;
                    var k2 = (linesVector[i2].Y + linesVector[j2].Y) / 2.0;
                    if (Abs(Abs(Abs(k1) - Abs(k2)) - PI / 2.0) <= errorK)
                        accumParallelandPerpendicular.Add((i1, j1, i2, j2));
                }
            }
            #endregion

            #region находим точки пересечения прямых
            var poinsList4 = new List<(System.Drawing.Point, System.Drawing.Point, System.Drawing.Point, System.Drawing.Point)>();
            var p4 = new System.Drawing.Point[4];
            for (int i = 0; i < accumParallelandPerpendicular.Count; i++)
            {
                int i1 = accumParallelandPerpendicular[i].Item1;
                int j1 = accumParallelandPerpendicular[i].Item2;
                int i2 = accumParallelandPerpendicular[i].Item3;
                int j2 = accumParallelandPerpendicular[i].Item4;

                float r1 = linesVector[i1].X;
                float f1 = linesVector[i1].Y;
                float r2 = linesVector[i2].X;
                float f2 = linesVector[i2].Y;
                p4[0] = GetPointPolar(r1, f1, r2, f2);
                if (p4[0].X < 0 || p4[0].Y < 0 || p4[0].X >= temp.Width || p4[0].Y >= temp.Height) continue;

                r1 = linesVector[i2].X;
                f1 = linesVector[i2].Y;
                r2 = linesVector[j1].X;
                f2 = linesVector[j1].Y;
                p4[1] = GetPointPolar(r1, f1, r2, f2);
                if (p4[1].X < 0 || p4[1].Y < 0 || p4[1].X >= temp.Width || p4[1].Y >= temp.Height) continue;

                r1 = linesVector[j2].X;
                f1 = linesVector[j2].Y;
                r2 = linesVector[j1].X;
                f2 = linesVector[j1].Y;
                p4[2] = GetPointPolar(r1, f1, r2, f2);
                if (p4[2].X < 0 || p4[2].Y < 0 || p4[2].X >= temp.Width || p4[2].Y >= temp.Height) continue;

                r1 = linesVector[j2].X;
                f1 = linesVector[j2].Y;
                r2 = linesVector[i1].X;
                f2 = linesVector[i1].Y;
                p4[3] = GetPointPolar(r1, f1, r2, f2);
                if (p4[3].X < 0 || p4[3].Y < 0 || p4[3].X >= temp.Width || p4[3].Y >= temp.Height) continue;

                poinsList4.Add((p4[0], p4[1], p4[2], p4[3]));
            }
            #endregion

            #region Удаляем все прямоугольники  
            var SqrList = new List<((System.Drawing.Point, System.Drawing.Point, System.Drawing.Point, System.Drawing.Point), double, double)>();
            foreach (var p in poinsList4)
            {
                var p1 = p.Item1;
                var p2 = p.Item2;
                double a1 = Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
                p1 = p.Item3;
                double a2 = Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
                p2 = p.Item4;
                double a3 = Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
                p1 = p.Item1;
                double a4 = Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
                double aSum = (a1 + a2 + a3 + a4);
                double aAverage = aSum / 4.0;
                double errorSqr = Sqrt((aAverage - a1) * (aAverage - a1) + (aAverage - a2) * (aAverage - a2)
                    + (aAverage - a3) * (aAverage - a3) + (aAverage - a4) * (aAverage - a4));
                errorSqr /= aAverage;
                if ((aSum > errorB * 4) && (errorSqr < 0.0952380952381))// -  отношения ско и среднего значения ребра при котором пара рёбер отличается на 15%(предел равен двум)
                    SqrList.Add((p, errorSqr, aAverage * aAverage));
            }

            var sAverage = (from l in SqrList select l.Item3).Average();
            sAverage /= 2;

            SqrList = (from l in SqrList
                       where l.Item3 > sAverage
                       select l).ToList();

            SqrList = new HashSet<((System.Drawing.Point, System.Drawing.Point, System.Drawing.Point, System.Drawing.Point), double, double)>(SqrList).ToList();

            #endregion

            #region дистанционное преобразование
            var canny_img = temp.Clone();
            var poinsAndErrorDist = new List<((System.Drawing.Point, System.Drawing.Point, System.Drawing.Point, System.Drawing.Point), float)>();
            Matrix<float> dist_img = new Matrix<float>(temp.Rows, temp.Cols);
            CvInvoke.BitwiseNot(canny_img, canny_img);
            CvInvoke.DistanceTransform(canny_img, dist_img, null, DistType.L2, 5);

            for (int i = 0; i < SqrList.Count; i++)
            {
                float errorDist = 0;
                var p = SqrList[i].Item1;
                errorDist += ErrorLine(p.Item1, p.Item2, in dist_img);
                errorDist += ErrorLine(p.Item2, p.Item3, in dist_img);
                errorDist += ErrorLine(p.Item3, p.Item4, in dist_img);
                errorDist += ErrorLine(p.Item4, p.Item1, in dist_img);
                float a1 = (float)Sqrt((p.Item1.X - p.Item2.X) * (p.Item1.X - p.Item2.X) + (p.Item1.Y - p.Item2.Y) * (p.Item1.Y - p.Item2.Y));
                errorDist /= a1;
                poinsAndErrorDist.Add((p, errorDist));
            }

            poinsAndErrorDist.Sort((((System.Drawing.Point, System.Drawing.Point, System.Drawing.Point, System.Drawing.Point), float) x, ((System.Drawing.Point, System.Drawing.Point, System.Drawing.Point, System.Drawing.Point), float) y) =>
            {
                return x.Item2.CompareTo(y.Item2);
            });

            #endregion

            var savePointsS = poinsAndErrorDist[0].Item1;

            #region превращаем почти квадрат в квадрат
            var poinsRect = poinsAndErrorDist[0].Item1;
            float centerX = (float)((poinsRect.Item1.X + poinsRect.Item2.X + poinsRect.Item3.X + poinsRect.Item4.X) / 4.0);
            float centerY = (float)((poinsRect.Item1.Y + poinsRect.Item2.Y + poinsRect.Item3.Y + poinsRect.Item4.Y) / 4.0);
            PointF centerSqr = new PointF(centerX, centerY);
            var side = (float)((Sqrt((poinsRect.Item1.X - poinsRect.Item2.X) * (poinsRect.Item1.X - poinsRect.Item2.X) + (poinsRect.Item1.Y - poinsRect.Item2.Y) * (poinsRect.Item1.Y - poinsRect.Item2.Y))
                + Sqrt((poinsRect.Item3.X - poinsRect.Item2.X) * (poinsRect.Item3.X - poinsRect.Item2.X) + (poinsRect.Item3.Y - poinsRect.Item2.Y) * (poinsRect.Item3.Y - poinsRect.Item2.Y))
                + Sqrt((poinsRect.Item3.X - poinsRect.Item4.X) * (poinsRect.Item3.X - poinsRect.Item4.X) + (poinsRect.Item3.Y - poinsRect.Item4.Y) * (poinsRect.Item3.Y - poinsRect.Item4.Y))
                + Sqrt((poinsRect.Item1.X - poinsRect.Item4.X) * (poinsRect.Item1.X - poinsRect.Item4.X) + (poinsRect.Item1.Y - poinsRect.Item4.Y) * (poinsRect.Item1.Y - poinsRect.Item4.Y))) / 4.0);
            SizeF sideSqr = new SizeF(side, side);

            float angleSqr;
            //angleSqr = (float)(Atan(Abs((poinsRect.Item1.Y - centerY) / (poinsRect.Item1.X - centerX)))
            //    + (PI / 2 - Atan(Abs((poinsRect.Item2.Y - centerY) / (poinsRect.Item2.X - centerX))))
            //    + Atan(Abs((poinsRect.Item3.Y - centerY) / (poinsRect.Item3.X - centerX)))
            //    + (PI / 2 - Atan(Abs((poinsRect.Item4.Y - centerY) / (poinsRect.Item4.X - centerX))))) / 4;

            if (poinsRect.Item1.X > centerX && poinsRect.Item1.Y > centerY)
                angleSqr = (float)Atan(((poinsRect.Item1.X - centerX) / (poinsRect.Item1.Y - centerY)));
            else if (poinsRect.Item2.X > centerX && poinsRect.Item2.Y > centerY)
                angleSqr = (float)Atan(((poinsRect.Item2.X - centerX) / (poinsRect.Item2.Y - centerY)));
            else if (poinsRect.Item3.X > centerX && poinsRect.Item3.Y > centerY)
                angleSqr = (float)Atan(((poinsRect.Item3.X - centerX) / (poinsRect.Item3.Y - centerY)));
            else if (poinsRect.Item4.X > centerX && poinsRect.Item4.Y > centerY)
                angleSqr = (float)Atan(((poinsRect.Item4.X - centerX) / (poinsRect.Item4.Y - centerY)));
            else
                angleSqr = 0;

            angleSqr = (float)(angleSqr * 180f / PI);


            angleSqr = (float)(45f - angleSqr);
            var Rect = new RotatedRect(centerSqr, sideSqr, angleSqr);
            #endregion

            var saveRectS = Rect;

            #region уточняем значение квадрата 
            var FinalRectList = new List<(RotatedRect, float)>();

            int clarificationXY = 5;//15
            int clarificationA = 5;//10
            int clarificationF = 40;//5
            float xn = Rect.Center.X - clarificationXY; xn = xn > 0 ? xn : 0;
            float xk = Rect.Center.X + clarificationXY; xk = xk > temp.Width ? temp.Width : xk;
            float xh = (xk - xn) / 20f;
            float yn = Rect.Center.Y - clarificationXY; yn = yn > 0 ? yn : 0;
            float yk = Rect.Center.Y + clarificationXY; yk = yk > temp.Height ? temp.Height : yk;
            float yh = (yk - yn) / 20f;
            int an = (int)Rect.Size.Width - clarificationA; an = an > 0 ? an : 20;
            int ak = (int)Rect.Size.Width + clarificationA; ak = ak > temp.Width / 2 ? temp.Width / 2 : ak;
            float fn = angleSqr - (float)1;
            float fk = angleSqr + (float)1;
            float fh = (float)((fk - fn) / (float)clarificationF);

            for (float x = xn; x < xk; x += xh)
            {
                for (float y = yn; y < yk; y += yh)
                {
                    for (int a = an; a < ak; a++)
                    {
                        for (float f = fn; f <= fk; f += fh)
                        {
                            float errorDist = 0;
                            RotatedRect rect = new RotatedRect()
                            {
                                Angle = f,
                                Center = new PointF(x, y),
                                Size = new System.Drawing.Size(a, a)
                            };
                            //rect.Angle = f;
                            //rect.Center = new PointF(x, y);
                            //rect.Size = new System.Drawing.Size(a, a);
                            var ptemp = rect.GetVertices();
                            //if()
                            var p = (System.Drawing.Point.Round(ptemp[0]), System.Drawing.Point.Round(ptemp[1]), System.Drawing.Point.Round(ptemp[2]), System.Drawing.Point.Round(ptemp[3]));
                            errorDist += ErrorLine(p.Item1, p.Item2, in dist_img);
                            errorDist += ErrorLine(p.Item2, p.Item3, in dist_img);
                            errorDist += ErrorLine(p.Item3, p.Item4, in dist_img);
                            errorDist += ErrorLine(p.Item4, p.Item1, in dist_img);
                            float a1 = (float)a;//Sqrt((p.Item1.X - p.Item2.X) * (p.Item1.X - p.Item2.X) + (p.Item1.Y - p.Item2.Y) * (p.Item1.Y - p.Item2.Y));
                            errorDist /= a1;
                            FinalRectList.Add((rect, errorDist));
                        }
                    }
                }
            }

            FinalRectList.Sort(((RotatedRect, float) x, (RotatedRect, float) y) =>
            {
                return x.Item2.CompareTo(y.Item2);
            });
            #endregion  

            #region рисуем
            return FinalRectList[0].Item1;
            #endregion

        }

        private static float ErrorLine(System.Drawing.Point p1,System.Drawing.Point p2, in Matrix<float> mat)
        {
            float errorLine;
            int x1 = p1.X;
            int y1 = p1.Y;
            int x2 = p2.X;
            int y2 = p2.Y;
            int deltaX = Math.Abs(x2 - x1);
            int deltaY = Math.Abs(y2 - y1);
            int signX = x1 < x2 ? 1 : -1;
            int signY = y1 < y2 ? 1 : -1;
            int error = deltaX - deltaY;

            errorLine = mat[y2, x2];

            while (x1 != x2 || y1 != y2)
            {
                errorLine += mat[y1, x1];

                int error2 = error * 2;
                if (error2 > -deltaY)
                {
                    error -= deltaY;
                    x1 += signX;
                }
                if (error2 < deltaX)
                {
                    error += deltaX;
                    y1 += signY;
                }
            }
            return errorLine;
        }
        private static System.Drawing.Point GetPointPolar(float r1, float f1, float r2, float f2)
        {
            float c = (float)(Cos(f1) / Cos(f2));
            float y = (float)((r1 - c * r2) / (Sin(f1) - c * Sin(f2)));
            float x = (float)((r1 - y * Sin(f1)) / Cos(f1));
            return System.Drawing.Point.Round(new PointF(x, y));
        }
        private static VectorOfVectorOfPoint DeleteSmallCntrs(VectorOfVectorOfPoint input)
        {    
            List<int> ints = new List<int>();

            var Scntr = new List<double>();
            var Lcntr = new List<double>();
            for (int i = 0; i < input.Size; i++)
            {
                Scntr.Add(CvInvoke.ContourArea(input[i], false));
                Lcntr.Add(CvInvoke.ArcLength(input[i], true));
            }
            var Saverage = Scntr.Average();
            var Laverage = Lcntr.Average();


            Laverage *= 0.9;
            Saverage *= 0.9;

            for (int i = 0; i < input.Size; i++)
            {
                if (!(Scntr[i] < Saverage && Lcntr[i] < Laverage))
                    ints.Add(i);
            }

            System.Drawing.Point[][] conts = new System.Drawing.Point[ints.Count][];
            for (int i = 0; i < ints.Count; i++)
                conts[i] = new System.Drawing.Point[input[ints[i]].Size];

            for (int i = 0; i < ints.Count; i++)
                for (int j = 0; j < conts[i].Length; j++)
                    conts[i][j] = input[ints[i]][j];

            var res = new VectorOfVectorOfPoint(conts);
            return res;
        }

        public static CircleF FindCirl1(Mat image)
        {
            Mat src = image.Clone();
           // CvInvoke.CvtColor(src, src, ColorConversion.Bgr2Gray);
            Mat src2 = src.Clone();
            Mat temp = new Mat();


            #region фильтры
            CvInvoke.MedianBlur(src, src, 15);
            CvInvoke.BilateralFilter(src, temp, -1, 15, 15, BorderType.Constant);
            CvInvoke.Canny(temp, temp, 3900, 6800, 7, true);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(temp, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxNone);
            contours = DeleteContursEdge(contours, temp, 1);
            temp.SetTo(new MCvScalar(0));
            CvInvoke.DrawContours(temp, contours, -1, new MCvScalar(255), 1);
            #endregion

            #region ищем окружности 
            double cannyThreshold = 180.0;
            double circleAccumulatorThreshold = 120.0;
            CircleF[] circles = CvInvoke.HoughCircles(temp, HoughModes.Gradient, 2.08/*2.0*/, 1 /* 2000*/, cannyThreshold,
                    circleAccumulatorThreshold, minRadius: Math.Min(temp.Width, temp.Height) / 20, maxRadius: Math.Min(temp.Width, temp.Height) / 2);


            #endregion

            #region сортируем окружности  
            var canny_img = temp.Clone();
            var circleListError = new List<(CircleF, float)>();
            Matrix<float> dist_img = new Matrix<float>(temp.Rows, temp.Cols);
            CvInvoke.BitwiseNot(canny_img, canny_img);
            Matrix<int> labels = new Matrix<int>(canny_img.Rows, canny_img.Cols);
            CvInvoke.DistanceTransform(canny_img, dist_img, labels, DistType.L2, 5);

            foreach (var c in circles)
            {
                float errorDist = 0;
                errorDist = ErrorCirclev1(c, in dist_img);//v2= ; _ = dist_img;
                circleListError.Add((c, errorDist));
            }

            circleListError.Sort(((CircleF, float) x, (CircleF, float) y) =>
            {
                return x.Item2.CompareTo(y.Item2);
            });
            #endregion

            #region раскладываем номера контуров по лейблам
            Dictionary<int, int> labelsContours = new Dictionary<int, int>();
            for (int i = 0; i < contours.Size; i++)
            {
                int key = labels[contours[i][0].Y, contours[i][0].X];
                if (!labelsContours.ContainsKey(key))
                    labelsContours.Add(key, i);
            }
            #endregion

            #region страшное 

            HashSet<int> labelsSave = new HashSet<int>();
            List<System.Drawing.Point> circlesPoints = new List<System.Drawing.Point>();
            ErrorCirclev2(circleListError[0].Item1, in dist_img, in labels, ref labelsSave);
            double rError = 0;
            CircleF circle = circleListError[0].Item1;
            int count = 0;
            foreach (var label in labelsSave)
            {
                for (int i = 0; i < contours[label].Size; i++)
                {
                    System.Drawing.Point p = contours[label][i];

                    circlesPoints.Add(p);
                    double r = Sqrt((p.X - circle.Center.X) * (p.X - circle.Center.X) + (p.Y - circle.Center.Y) * (p.Y - circle.Center.Y));
                    rError += Abs(r - circle.Radius);
                    count++;
                }
            }
            rError /= (double)count;
            #endregion


            #region отправляем
            //var radius = circleListError[0].Item1.Radius;
            //var center = circleListError[0].Item1.Center;
            //canvas.PaintCircle(radius, center.X, center.Y);
            return circleListError[0].Item1;
            #endregion
        }

        public static CircleF FindCirl2(Mat image)
        {
            var canny = new Mat();
            CvInvoke.BilateralFilter(image.Clone(), image, -1, 15, 15, Emgu.CV.CvEnum.BorderType.Constant);//50 50 
            CvInvoke.MedianBlur(image, image, 15);

            double threshold = 1000;
            double thresholdStep = 1000;
            bool correction = false;
            double maxPercentage = 0.55;
            double minPercentage = 0.45;
            int numberIterations = 0;
            while (true)
            {
                if (numberIterations++ > 100)
                {
                    CannyUp(image, canny, 10000);
                    break;
                }

                if (threshold >= 10000)
                {
                    CvInvoke.MedianBlur(image, image, 5);
                    threshold = 1000;
                }
                CannyUp(image, canny, threshold);
                double percentageWhitePixels = PercentageWhitePixels(canny);
                if (percentageWhitePixels < maxPercentage && percentageWhitePixels > minPercentage) //количество пикселей в данном диапозоне процентов
                    break;
                else if (percentageWhitePixels >= maxPercentage)
                {
                    if (correction)
                        thresholdStep /= 2.0;
                    threshold += thresholdStep;
                }
                else if (percentageWhitePixels <= minPercentage)
                {
                    correction = true;
                    if (correction)
                        thresholdStep /= 2.0;
                    threshold -= thresholdStep;
                }
            }

            VectorOfVectorOfPoint cntr = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canny, cntr, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            CircleF[] circles = CvInvoke.HoughCircles(canny, HoughModes.Gradient, 0.01, 5000, 500,
               1, minRadius: Math.Min(canny.Width, canny.Height) / 20,
               maxRadius: Math.Min(canny.Width, canny.Height) / 2);

            return circles[0];
        }
        private static double PercentageWhitePixels(Mat image)
        {
            int height = image.Height;
            int width = image.Width;

            Matrix<Byte> matrix = new Matrix<Byte>(image.Rows, image.Cols, image.NumberOfChannels);
            image.CopyTo(matrix);
            var data = matrix.Data;

            double sum = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    if (data[y, x] == 255)
                        sum++;
            }
            return (sum / (height * width)) * 100.0;
        }

        private static void CannyUp(IInputOutputArray input, IInputOutputArray output, double threshold)
        {
            CvInvoke.Canny(input, output, threshold / 2.0, threshold, 7, true);
        }

        private static VectorOfVectorOfPoint DeleteContursEdge(VectorOfVectorOfPoint input, Mat s, double interest = 1.5)
        {
            if (interest < 0) interest = 0;
            if (interest > 100) interest = 100;
            List<int> ints = new List<int>();

            System.Drawing.Point Min = new System.Drawing.Point((int)(s.Width * (interest / 100.0)), (int)(s.Height * (interest / 100.0)));
            System.Drawing.Point Max = new System.Drawing.Point((int)(s.Width * (1 - interest / 100.0)), (int)(s.Height * (1 - interest / 100.0)));
            for (int i = 0; i < input.Size; i++)
                if (ConturAtEdge(input[i], Min, Max))
                    ints.Add(i);

            System.Drawing.Point[][] conts = new System.Drawing.Point[ints.Count][];
            for (int i = 0; i < ints.Count; i++)
                conts[i] = new System.Drawing.Point[input[ints[i]].Size];

            for (int i = 0; i < ints.Count; i++)
                for (int j = 0; j < conts[i].Length; j++)
                    conts[i][j] = input[ints[i]][j];

            var res = new VectorOfVectorOfPoint(conts);
            return res;
        }
        private static bool ConturAtEdge(VectorOfPoint input, System.Drawing.Point Min, System.Drawing.Point Max)
        {
            bool PointAtRectangle;
            for (int i = 0; i < input.Size; i++)
            {
                PointAtRectangle = input[i].X > Min.X && input[i].Y > Min.Y && input[i].X < Max.X && input[i].Y < Max.Y;
                if (!PointAtRectangle)
                    return false;
            }
            return true;
        }
        private static float ErrorCirclev1(CircleF c, in Matrix<float> mat)
        {
            float errorCorect = 0;
            float errorCircle = 0;
            int x0 = (int)c.Center.X;
            int y0 = (int)c.Center.Y;
            int radius = (int)c.Radius;

            int x = 0;
            int y = radius;
            int delta = 1 - 2 * radius;
            int error;
            while (y >= 0)
            {
                if ((y0 + y) >= mat.Height || (x0 + x) >= mat.Width || (y0 - y) < 0 || (x0 - x) < 0)
                    return float.MaxValue;

                var mastemp = mat[y0 + y, x0 + x];
                if (mastemp <= 1)
                    errorCircle++;
                else if (mastemp <= 2)
                    errorCircle += 0.75f;
                else if (mastemp <= 3)
                    errorCircle += 0.5f;
                else if (mastemp < 5)
                    errorCircle += 0.25f;

                mastemp = mat[y0 - y, x0 + x];
                if (mastemp <= 1)
                    errorCircle++;
                else if (mastemp <= 2)
                    errorCircle += 0.75f;
                else if (mastemp <= 3)
                    errorCircle += 0.5f;
                else if (mastemp < 5)
                    errorCircle += 0.25f;

                mastemp = mat[y0 + y, x0 - x];
                if (mastemp <= 1)
                    errorCircle++;
                else if (mastemp <= 2)
                    errorCircle += 0.75f;
                else if (mastemp <= 3)
                    errorCircle += 0.5f;
                else if (mastemp < 5)
                    errorCircle += 0.25f;

                mastemp = mat[y0 - y, x0 - x];
                if (mastemp <= 1)
                    errorCircle++;
                else if (mastemp <= 2)
                    errorCircle += 0.75f;
                else if (mastemp <= 3)
                    errorCircle += 0.5f;
                else if (mastemp < 5)
                    errorCircle += 0.25f;

                errorCorect += 4;
                error = 2 * (delta + y) - 1;
                if (delta < 0 && error <= 0)
                {
                    ++x;
                    delta += 2 * x + 1;
                    continue;
                }
                error = 2 * (delta - x) - 1;
                if (delta > 0 && error > 0)
                {
                    --y;
                    delta += 1 - 2 * y;
                    continue;
                }
                ++x;
                delta += 2 * (x - y);
                --y;
            }
            return (errorCorect - errorCircle) / errorCorect;
        }

        private static float ErrorCirclev2(CircleF c, in Matrix<float> mat, in Matrix<int> labels, ref HashSet<int> save)
        {
            float errorCorect = 0;
            float errorCircle = 0;
            int x0 = (int)c.Center.X;
            int y0 = (int)c.Center.Y;
            int radius = (int)c.Radius;

            int x = 0;
            int y = radius;
            int delta = 1 - 2 * radius;
            int error;
            while (y >= 0)
            {
                if ((y0 + y) >= mat.Height || (x0 + x) >= mat.Width || (y0 - y) < 0 || (x0 - x) < 0)
                    return float.MaxValue;

                var mastemp = mat[y0 + y, x0 + x];
                if (mastemp < 5)
                {
                    save.Add(labels[y0 + y, x0 + x]);
                    if (mastemp <= 1)
                        errorCircle++;
                    else if (mastemp <= 2)
                        errorCircle += 0.75f;
                    else if (mastemp <= 3)
                        errorCircle += 0.5f;
                    else if (mastemp < 5)
                        errorCircle += 0.25f;
                }

                mastemp = mat[y0 - y, x0 + x];
                if (mastemp < 5)
                {
                    save.Add(labels[y0 - y, x0 + x]);
                    if (mastemp <= 1)
                        errorCircle++;
                    else if (mastemp <= 2)
                        errorCircle += 0.75f;
                    else if (mastemp <= 3)
                        errorCircle += 0.5f;
                    else if (mastemp < 5)
                        errorCircle += 0.25f;
                }

                mastemp = mat[y0 + y, x0 - x];
                if (mastemp < 5)
                {
                    save.Add(labels[y0 + y, x0 - x]);
                    if (mastemp <= 1)
                        errorCircle++;
                    else if (mastemp <= 2)
                        errorCircle += 0.75f;
                    else if (mastemp <= 3)
                        errorCircle += 0.5f;
                    else if (mastemp < 5)
                        errorCircle += 0.25f;
                }

                mastemp = mat[y0 - y, x0 - x];
                if (mastemp < 5)
                {
                    save.Add(labels[y0 - y, x0 - x]);
                    if (mastemp <= 1)
                        errorCircle++;
                    else if (mastemp <= 2)
                        errorCircle += 0.75f;
                    else if (mastemp <= 3)
                        errorCircle += 0.5f;
                    else if (mastemp < 5)
                        errorCircle += 0.25f;
                }

                errorCorect += 4;
                error = 2 * (delta + y) - 1;
                if (delta < 0 && error <= 0)
                {
                    ++x;
                    delta += 2 * x + 1;
                    continue;
                }
                error = 2 * (delta - x) - 1;
                if (delta > 0 && error > 0)
                {
                    --y;
                    delta += 1 - 2 * y;
                    continue;
                }
                ++x;
                delta += 2 * (x - y);
                --y;
            }
            return (errorCorect - errorCircle) / errorCorect;
        }
    }
}

