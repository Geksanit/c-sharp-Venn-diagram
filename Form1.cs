using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Graphics holst;
        private Pen pen_green;
        private Point A_center;
        private Point B_center;
        private int radius;
        private ISet<Char> A_set;
        private ISet<Char> B_set;
        private ISet<Char> U_set;
        private GraphicsPath A_path;
        private GraphicsPath B_path;

        public Form1()
        {
            InitializeComponent();
            A_center = new Point(200, 200);
            B_center = new Point(400, 200);
            radius = 130;
            pen_green = new Pen(Color.Green, 1);
            A_set = new HashSet<char>();
            A_set.Add('x');
            A_set.Add('x');
            A_set.Add('y');
            A_set.Add('z');

            B_set = new HashSet<char>();
            B_set.Add('y');
            B_set.Add('m');

            U_set = new HashSet<char>();
            U_set.UnionWith(A_set);
            U_set.UnionWith(B_set);
            U_set.Add('u');

            textBox1.Text = SetToString(A_set);
            textBox2.Text = SetToString(B_set);
            textBox3.Text = SetToString(U_set);

            A_path = GetPathForCircle(A_center);
            B_path = GetPathForCircle(B_center);
        }

        public void Draw(object sender, EventArgs e)
        {
            Console.WriteLine("Draw call {0}", textBox4.Text);
            var res = ParseExpression(textBox4.Text);
            Console.WriteLine("res {0}", SetToString(res));
            label10.Text = SetToString(res);
            renderResult(res);
        }

        private ISet<char> ParseExpression(string str)
        {
            string patternBinExpr = @"(!?\w)\s*(\+|\*|\\)\s*(!?\w)"; // example !A + B
            Match match = Regex.Match(str, patternBinExpr);
            if (match.Success)
            {
                var x = ParseSymbol(match.Groups[1].Value);
                var oper = match.Groups[2].Value;
                var y = ParseSymbol(match.Groups[3].Value);
                if (oper == "+")
                {
                    x.UnionWith(y);
                    return x;
                }
                else if (oper == "*")
                {
                    x.IntersectWith(y);
                    return x;
                }
                x.ExceptWith(y);
                return x;
            }
            string patternUnarExpr = @"(!?\w)";
            Match matchUnar = Regex.Match(str, patternUnarExpr);
            if (matchUnar.Success)
            {
                var x = ParseSymbol(matchUnar.Groups[1].Value);
                return x;
            }
            return new HashSet<char>();
        }

        private ISet<char> ParseSymbol(string str) 
        {
            string pattern = @"(!?)(A|B)"; // example !A
            Match match = Regex.Match(str, pattern);
            if (match.Success)
            {
                var symbol = match.Groups[2].Value;
                var set = symbol == "A" ? A_set : B_set;
                if (match.Groups[1].Value == String.Empty)
                {
                    return new HashSet<char>(set);
                }
                else
                {
                    
                    var res = new HashSet<char>();
                    res.UnionWith(U_set);
                    res.ExceptWith(set);
                    return res;
                }
            }
            else
            {
                return new HashSet<char>();
            }
        }

        private string SetToString(ISet<char> set)
        {
            var str = new StringBuilder();
            foreach (char ch in set)
            {
                str.Append(ch);
                str.Append(' ');
            }
            return str.ToString();
        }

        private bool isPointInCircle(Point point, Point center)
        {
            var a = point.X - center.X;
            var b = point.Y - center.Y;
            var c = Math.Sqrt(a * a + b * b);
            return c < radius;
        }

        private void renderResult(ISet<char> set)
        {
            holst = Graphics.FromHwnd(pictureBox1.Handle);
            ClearCanvas();
            var w = pictureBox1.Width;
            var h = pictureBox1.Height;

            Rectangle originalRectangle = new Rectangle(0, 0, w, h);
            Region resultRegion = new Region(originalRectangle);
            resultRegion.MakeEmpty();

            foreach (char ch in set)
            {
                Region region = new Region(originalRectangle);
                if (A_set.Contains(ch))
                {
                    region.Intersect(A_path);
                } else
                {
                    region.Exclude(A_path);
                }
                if (B_set.Contains(ch))
                {
                    region.Intersect(B_path);
                }
                else
                {
                    region.Exclude(B_path);
                }
                resultRegion.Union(region);
            }
            SolidBrush myBrush = new SolidBrush(Color.Blue);
            holst.FillRegion(myBrush, resultRegion);
        }

        public void RenderCircle(Point center)
        {
            holst.DrawEllipse(pen_green, new Rectangle(center.X - radius, center.Y - radius, radius * 2, radius * 2));
        }

        private GraphicsPath GetPathForCircle(Point center)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(center.X - radius, center.Y - radius, radius * 2, radius * 2);
            return path;
        }

        private void ClearCanvas()
        {
            holst.Clear(Color.White);
            RenderCircle(A_center);
            RenderCircle(B_center);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            holst = e.Graphics;
            ClearCanvas();
        }

        private void UpdateUSet()
        {
            U_set = new HashSet<char>();
            U_set.UnionWith(A_set);
            U_set.UnionWith(B_set);
            U_set.Add('u');
            textBox3.Text = SetToString(U_set);
        }

        private void UpdateSetFromText(ISet<char> set, string text)
        {
            set.Clear();
            var textWithoutSpaces = text.Replace(" ", "");
            foreach (char ch in textWithoutSpaces)
            {
                set.Add(ch);
            }
        }

        private void AInputOnChange(object sender, EventArgs e)
        {
            UpdateSetFromText(A_set, textBox1.Text);
            UpdateUSet();
        }

        private void BInputOnChange(object sender, EventArgs e)
        {
            UpdateSetFromText(B_set, textBox2.Text);
            UpdateUSet();
        }

    }
    }
