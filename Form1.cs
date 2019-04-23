using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace Compress
{
    public partial class Form1 : Form
    {
        List<PointF[]> PointsXY = new List<PointF[]>();//存放兰伯特投影转换坐标数据
        List<PointF[]> PointsBL = new List<PointF[]>();//存放原始的经纬度数据
        List<PointF[]> Points = new List<PointF[]>();//存放四至转换后的数据
        List<PointF[]> compressedPoints = new List<PointF[]>();//存放压缩后的数据
        List<int> ID = new List<int>();

        //四至
        Boundary boundaryXY = new Boundary();
        Boundary boundaryBL = new Boundary();
        Boundary boundaryCompression = new Boundary();

        string flag = "BL";
        double threshold;//阈值
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    PointsBL = readMapPoints(openFileDialog1.FileName, out ID);
                    Map.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //读GEN格式文件
        public static List<PointF[]> readMapPoints(string fileName, out List<int> id)        //获取地图数据里的点
        {
            List<PointF[]> mapPoints = new List<PointF[]>();
            id = new List<int>();

            string[] lines = File.ReadAllLines(fileName);//File.ReadAllLines()函数将全部文本文档内容放入字符串数组lines中，每一行作为lines这个字符串数组的一个元素
            List<PointF> points = new List<PointF>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    id.Add(int.Parse(lines[i].Trim()));
                    continue;
                }
                if (String.Compare(lines[i].ToUpper(), "END") == 0)
                {
                    mapPoints.Add(points.ToArray());//将 List<PointF> points转为数组,并添加到mapPoints中
                    points.Clear();
                    if (lines[i + 1].ToUpper() == "END")//读到两个END，结束
                        break;
                    else
                    {
                        id.Add(int.Parse(lines[i + 1].Trim()));//int.Parse将字符串转化为int型；Trim()是String类的内置方法，用于去除字符串前后的空格
                        i++;
                    }
                }
                else
                {
                    string[] data = lines[i].Split(',');//分割
                    PointF point = new PointF();
                    point.X = Convert.ToSingle(data[0]);
                    point.Y = Convert.ToSingle(data[1]);//字符串转化为浮点型
                    points.Add(point);
                }
            }
            return mapPoints;
        }

        //计算地图显示内容的四至
        public static Boundary getBoundary(List<PointF[]> mapPoints)
        {
            Boundary boundary = new Boundary();
            foreach (PointF[] pointArr in mapPoints)
            {
                foreach (PointF point in pointArr)
                {
                    if (boundary.xMin > point.X)
                        boundary.xMin = point.X;
                    if (boundary.xMax < point.X)
                        boundary.xMax = point.X;
                    if (boundary.yMin > point.Y)
                        boundary.yMin = point.Y;
                    if (boundary.yMax < point.Y)
                        boundary.yMax = point.Y;
                }
            }
            return boundary;
        }

        //建立地图四至与绘制画布的变换关系
        public static List<PointF[]> changePointsForDraw(List<PointF[]> mapPoints, PictureBox pictureBox, Boundary boundary)
        {
            List<PointF[]> outList = new List<PointF[]>();
            float mapWidth = boundary.xMax - boundary.xMin;
            float mapHeight = boundary.yMax - boundary.yMin;

            float perWidth = pictureBox.Width / mapWidth;
            float perHeight = pictureBox.Height / mapHeight;

            float temp = (perWidth < perHeight) ? perWidth : perHeight;//确定变换比例
            for (int i = 0; i < mapPoints.Count; i++)
            {
                PointF[] points = new PointF[mapPoints[i].Length];
                for (int j = 0; j < mapPoints[i].Length; j++)
                {
                    points[j] = new PointF();
                    points[j].X = (mapPoints[i][j].X - boundary.xMin) * temp;
                    points[j].Y = (mapPoints[i][j].Y - boundary.yMin) * temp;
                    if (perWidth > perHeight)
                    {
                        points[j].Y = pictureBox.Height - points[j].Y;
                    }
                    else
                    {
                        points[j].Y = mapHeight * temp - points[j].Y;
                    }
                }
                outList.Add(points);
            }
            return outList;
        }

        //兰伯特投影
        public static List<PointF[]> inverseCompute(List<PointF[]> mapPoints)
        {
            List<PointF[]> outList = new List<PointF[]>();
            for (int i = 0; i < mapPoints.Count; i++)
            {
                PointF[] points = new PointF[mapPoints[i].Length];
                for (int j = 0; j < mapPoints[i].Length; j++)
                {
                    points[j] =Lambert.blToXY(mapPoints[i][j]);
                }
                outList.Add(points);
                Lambert .blToXY 
            }
            return outList;
        }

        // 转弧度
        static double toRadian(double degree)
        {
            return degree / 180 * Math.PI;
        }

        //求MN对应直线的参数A、B、C
        static void computeABC(out double A,out double B,out double C,PointF M,PointF N)
        {
            double xM = M.X, yM = M.Y, xN = N.X, yN = N.Y;
            double temp = Math.Sqrt(Math.Pow(yM - yN, 2) + Math.Pow(xM - xN, 2));//两点间距离
            A = (yM - yN) / temp;
            B = (xN - xM) / temp;
            C = (xM * yN - xN * yM) / temp;
        }
       
        //求点point到直线距离
        static double distanceOfPointToLine(PointF point,double A,double B,double C)
        {
            return  Math.Abs(A * point.X + B * point.Y + C);
        }

        //判断曲线是否为闭合曲线
        public static bool isCircle(PointF[] points)
        {
            if(points[0].X==points[points.Length-1].X && points[0].Y==points[points.Length-1].Y)
            {
                return true;
            }
            return false;
        }

        //求两点距离
        static double distanceOfTwoPoints(PointF point1,PointF point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        //切断闭合曲线，返回切点的索引
        public static int breakpoint(PointF[] points)
        {
            List<double> d = new List<double>();//存放所有点到第一个点的距离
            for(int i=0;i<points.Length;i++)
            {
                d.Add(distanceOfTwoPoints(points[0], points[i]));
            }
            return d.IndexOf(d.Max());
        }

        //道格拉斯-普克迭代函数
        static void f(PointF[] points,List<PointF> compressedPoints ,int M,int N,double threshold)
        {
            double dh;
            int flag;
            double A,B,C;
            int pointsCount = N - M + 1;
            if (pointsCount>=3)
            {
                computeABC(out A, out B, out C, points[M], points[N]);//out参数用于在方法中返回多余的返回值，使得方法可以返回不同类型的返回值。
                List<double> dArr = new List<double>();//存放点到MN的距离
                for (int i = M+1; i <= N-1; i++)
                {
                    dArr.Add(distanceOfPointToLine(points[i], A, B, C));
                }
                dh = dArr.Max();
                flag = (dh > threshold) ? 1 : 0;
                int maxIndex=dArr.IndexOf(dh) + M+1;

                if (flag == 0)//dh<=threshold
                {
                    if (N != points.Length - 1)     //如果没有遍历到末点
                    {
                        compressedPoints.Add(points[N]);
                        f(points, compressedPoints, N, points.Length - 1, threshold);
                    }
                }
                else//dh>threshold
                {
                    f(points, compressedPoints, M, maxIndex, threshold);
                }
            }
            else 
                if(pointsCount==2 && N!=points.Length-1)//如果只有两个点,且没有遍历到末点
            {
                compressedPoints.Add(points[N]);//添加末点
                f(points, compressedPoints, N, points.Length - 1, threshold);
            }
        }

        //主压缩函数
        public static PointF[] DouglasPuckerCompress(PointF[] points,double threshold)
        {
            List<PointF> compressedPoints = new List<PointF>();
            compressedPoints.Add(points[0]);//添加起点
            f(points, compressedPoints, 0, points.Length - 1,threshold);
            compressedPoints.Add(points[points.Length - 1]);//添加末点
            return compressedPoints.ToArray();
        }

        //计算压缩率
        public static string computeCompressRate(List<PointF[]> mapPointsXY,List<PointF[]> compressedPoints)
        {
            int Count1 = 0, Count2 = 0;
            for (int i=0;i<mapPointsXY.Count;i++)
            {
                Count1 += mapPointsXY[i].Length;//压缩前点的数量
                Count2 += compressedPoints[i].Length;//压缩后点的数量
               
            }
            return Math.Round(Count2 * 100.0 / Count1).ToString() + "%";
        }
    
        private void pictureBox1_Paint(object sender, PaintEventArgs g)
        {
            try
            {
                if (PointsBL.Count > 0)
                {
                    switch (flag)
                    {
                        case "BL":
                            boundaryBL= getBoundary(PointsBL);
                            Points =changePointsForDraw(PointsBL, Map, boundaryBL);
                            break;
                        case "XY":
                            PointsXY = inverseCompute(PointsBL);
                            boundaryXY= getBoundary(PointsXY);
                            Points = changePointsForDraw(PointsXY, Map, boundaryXY);
                            break;
                        case "Compression":
                            boundaryCompression =getBoundary(compressedPoints);
                            Points =changePointsForDraw(compressedPoints, Map, boundaryCompression);
                            break;
                    }
                    showMap(Points, g);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        //显示地图
        void showMap(List<PointF[]> Points, PaintEventArgs g) 
        {
            for (int i = 0; i < Points.Count; i++)
                g.Graphics.DrawLines(Pens.Black, Points[i]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (PointsBL.Count > 0)
            {
                flag = "XY";
                Map.Refresh();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                try
                {
                    threshold = double.Parse(textBox1.Text);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n请输入正确的阈值!");
                    return;
                }
                flag = "Compression";
                compressedPoints.Clear();
                for (int i = 0; i < PointsXY.Count; i++)
                {
                    if (isCircle(PointsXY[i]))
                    {
                        Console.WriteLine(i.ToString());
                        //如果曲线闭合，则把曲线分为两段，分别压缩，然后合并
                        int breakPointIndex = breakpoint(PointsXY[i]);
                        PointF[] points1 = new PointF[breakPointIndex - 0 + 1];
                        PointF[] points2 = new PointF[PointsXY[i].Length - 1 - breakPointIndex + 1];
                        Array.Copy(PointsXY[i], points1, points1.Length);
                        Array.Copy(PointsXY[i], breakPointIndex, points2, 0, points2.Length);
                        PointF[] compressedPoints1 = DouglasPuckerCompress(points1, threshold);
                        PointF[] compressedPoints2 = DouglasPuckerCompress(points2, threshold);
                        PointF[] finalCompressedPoints = new PointF[compressedPoints1.Length + compressedPoints2.Length - 1];
                        Array.Copy(compressedPoints1, finalCompressedPoints, compressedPoints1.Length);
                        Array.Copy(compressedPoints2, 1, finalCompressedPoints, compressedPoints1.Length, compressedPoints2.Length - 1);
                        compressedPoints.Add(finalCompressedPoints);
                    }
                    else
                    {
                        compressedPoints.Add(DouglasPuckerCompress(PointsXY[i], threshold));
                    }
                }
                string compressedResultStr = textBox1.Text + "       " + computeCompressRate(PointsXY, compressedPoints);
                listBox1.Items.Insert(1, compressedResultStr);
                Map.Refresh();
               
            }
            else
                MessageBox.Show("请输入阈值!");
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            listBox1.Items.Add("阈值     压缩率");
        }
    }
}
