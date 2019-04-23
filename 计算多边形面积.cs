using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Drawing.Drawing2D;


namespace 地图投影
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 定义基本变量
        private string Filename;
        private FileStream Fs;
        private ArrayList List = new ArrayList();
        private ArrayList List_2 = new ArrayList();//ArrayLIst表示动态数组

        Pen p = new Pen(Color.Black);
        Pen np = new Pen(Color.Red);

        PointF[] points;
        PointF[] npoints;
        #endregion

        #region 北京54坐标系的基本参数
        double a = 6378245;               //长半轴
        double b = 6356863.01877;         //短半轴
        double e0 = 0.006693421622966;    //第一偏心率平方
        double e1 = 0.006738525414684;    //第二偏心率平方
        #endregion

        #region 兰伯特投影
        double L0 = 105 * Math.PI / 180;  //原点经度
        double B0 = 0;                    //原点纬度
        double B1 = 20 * Math.PI / 180;   //第一标准纬线
        double B2 = 40 * Math.PI / 180;   //第二标准纬线
        float x;
        #endregion

        #region 数组复制函数
        public void Copy(PointF[] p1, out PointF[] p2)//PointF表示浮点的XY坐标定义在二维空间的有序的对
        {
            PointF temp;
            ArrayList a = new ArrayList();

            for (int i = 0; i < p1.Length; i++)
            {
                temp = new PointF(p1[i].X, p1[i].Y);
                a.Add(temp);//把temp添加到a数组中
            }

            p2 = (PointF[])a.ToArray(typeof(PointF));//把a 数组中的元素放在 PointF[] p2
        }
        #endregion

        #region 打开北京54坐标系
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd1 = new OpenFileDialog();//OpenFileDialog 显示一个标准对话框，提示打开文件
            ofd1.FileName = "*.gen";
            if (ofd1.ShowDialog() == DialogResult.OK)//指定当对话框返回值为OK时
            {
                Filename = ofd1.FileName;//文件名字
                
                //显示数据
                Fs = new FileStream(Filename, FileMode.Open);//用指定的路径和创建模式初始化FileStream为fs
                StreamReader sr = new StreamReader(Fs);//为指定的流初始化StreamReader
                richTextBox1.Text = sr.ReadToEnd();//读取流的当前位置到结束
                Fs.Close();//关闭fs

                //存储数据
                Fs = new FileStream(Filename, FileMode.Open);
                StreamReader Sr = new StreamReader(Fs);

                string[] data;
                string line;

                PointF[] point;
                PointF temp;

                ArrayList list1 = new ArrayList();

                while (!Sr.EndOfStream)
                {
                    //读取id
                    Sr.ReadLine();
                    line = Sr.ReadLine();

                     while (line != "END" && line != null)
                     {
                            
                         data = line.Split(',');
                         temp = new PointF(float.Parse(data[0]), float.Parse(data[1]));
                         list1.Add(temp);
                          line = Sr.ReadLine();

                      }   
                    point = (PointF[])list1.ToArray(typeof(PointF));
                    List.Add(point);
                    list1 = new ArrayList();
                        
                }

                //经纬网
                for (int i = 70; i <= 140; i += 5)
                {
                    for (int j = 0; j <= 60; j += 5)
                    {
                        temp = new PointF(i, j);
                        list1.Add(temp);
                    }
                    point = (PointF[])list1.ToArray(typeof(PointF));
                    List.Add(point);
                    list1 = new ArrayList();
                }

                for (int j = 0; j <= 60; j += 5)
                {
                    for (int i = 70; i <= 140; i += 5)
                    {
                        temp = new PointF(i, j);
                        list1.Add(temp);
                    }
                    point = (PointF[])list1.ToArray(typeof(PointF));
                    List.Add(point);
                    list1 = new ArrayList();
                }

                list1 = new ArrayList();
                Fs.Dispose();
            }
        }
        #endregion

        #region 显示北京54坐标系
        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();//清除文本框控件中的所有文本
            using (Graphics g = this.richTextBox1.CreateGraphics())//为控件创建GDI和绘图图面
            {
                richTextBox1.Refresh();//强制控件使其工作区无效，并立即重汇自己和任何子控件

                for (int i = 0; i < List.Count;i++ )
                {
                    Copy((PointF[])List[i], out points);
                    if (points != null)
                    {
                        for (int j = 0; j < points.Count(); j++)
                        {
                            points[j].X = 8 * points[j].X - 400;
                            points[j].Y = -8 * points[j].Y + 480;
                            npoints = points;

                        }
                        g.DrawCurve(p, npoints);
                    }
                    
                    points = null;
                }
            }
            npoints = null;
           
        }
        #endregion

        #region 中国版图墨卡托投影转换
        private void button8_Click(object sender, EventArgs e)
        {
            float tempx = 0;float tempy = 0;
            richTextBox1.Clear();
            using (Graphics g = this.richTextBox1.CreateGraphics())
            {
                richTextBox1.Refresh();
                for (int i = 0; i < List.Count; i++)
                {
                    Copy((PointF[])List[i], out points);
                    if (points != null)
                    {
                        for (int j = 0; j < points.Length; j++)
                        {
                            x = points[j].X;
                            points[j].X = (float)(((Math.Pow(a, 2) / b) / Math.Sqrt(1 + e1)) * Math.Log((Math.Tan(Math.PI / 4 + (points[j].Y * Math.PI / 180) / 2)) * Math.Pow(((1 - Math.Sqrt(e0) * Math.Sin(points[j].Y * Math.PI / 180)) / (1 + Math.Sqrt(e0) * Math.Sin(points[j].Y * Math.PI / 180))), (Math.Sqrt(e0) / 2)), Math.E));
                            points[j].Y = (float)(((Math.Pow(a, 2) / b) / Math.Sqrt(1 + e1)) * ((x * Math.PI / 180) - L0));
                            //转换坐标系
                            tempx = points[j].X; 
                            tempy = points[j].Y;
                            points[j].X = 0.0001f * tempy + 450;
                            points[j].Y = 720-0.0001f * tempx ;
                            npoints = points;
                        }
                        g.DrawLines(p, npoints);
                    }
                    points = null;
                }

            }
            npoints = null;
        }
        #endregion

        #region 中国版图兰勃特投影变换
        private void button9_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            float tempx = 0; float tempy = 0;
            using (Graphics g = this.richTextBox1.CreateGraphics())
            {
                richTextBox1.Refresh();
                double mb1 = Math.Cos(B1) / Math.Sqrt(1 - e0 * Math.Pow(Math.Sin(B1), 2));
                double mb2 = Math.Cos(B2) / Math.Sqrt(1 - e0 * Math.Pow(Math.Sin(B2), 2));
                double tb1 = Math.Tan(Math.PI / 4 - B1 / 2) / Math.Pow(((1 - Math.Sqrt(e0) * Math.Sin(B1)) / (1 + Math.Sqrt(e0) * Math.Sin(B1))), (Math.Sqrt(e0) / 2));
                double tb2 = Math.Tan(Math.PI / 4 - B2 / 2) / Math.Pow(((1 - Math.Sqrt(e0) * Math.Sin(B2)) / (1 + Math.Sqrt(e0) * Math.Sin(B1))), (Math.Sqrt(e0) / 2));
                double n = Math.Log(mb1 / mb2, Math.E) / Math.Log(tb1 / tb2, Math.E);
                double f = mb1 / (n * Math.Pow(tb1, n));
                double r, r0 = a * f;
                double ct;

                for (int i = 0; i < List.Count; i++)
                {
                    Copy((PointF[])List[i], out points);
                    if (points != null)
                    {
                        for (int j = 0; j < points.Length; j++)
                        {
                            r = a * f * Math.Pow(Math.Tan(Math.PI / 4 - (points[j].Y * Math.PI / 180) / 2) / Math.Pow(((1 - Math.Sqrt(e0) * Math.Sin(points[j].Y * Math.PI / 180)) / (1 + Math.Sqrt(e0) * Math.Sin(points[j].Y * Math.PI / 180))), (Math.Sqrt(e0) / 2)), n);
                            ct = n * (points[j].X * Math.PI / 180 - L0);
                            points[j].X = (float)(r0 - r * Math.Cos(ct));
                            points[j].Y = (float)(r * Math.Sin(ct));

                            tempx = points[j].X;
                            tempy = points[j].Y;
                            points[j].X = 0.0001f * tempy+400;
                            points[j].Y = -0.0001f * tempx+700;
                            npoints = points;
                        }
                        g.DrawLines(p, npoints);
                    } 
                    points = null;
                }
            }
            npoints = null;
        }
        #endregion

        #region WGS84坐标系的基本参数
        double a1 = 6378137;               //长半轴
        double b1 = 6356752.3142;         //短半轴
        double e01 = 0.00669437999013;    //第一偏心率平方
        double e11 = 0.006739496742227;    //第二偏心率平方
        #endregion

        #region 打开WGS84坐标系
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd2 = new OpenFileDialog();
            ofd2.FileName = "*.gen";

            if (ofd2.ShowDialog() == DialogResult.OK)
            {
                Filename = ofd2.FileName;
               
                //显示数据
                Fs = new FileStream(Filename, FileMode.OpenOrCreate);
                StreamReader sr = new StreamReader(Fs);
                richTextBox1.Text = sr.ReadToEnd();
                Fs.Close();

                Fs = new FileStream(Filename, FileMode.OpenOrCreate);
                StreamReader Sr = new StreamReader(Fs);

                string[] data;
                string line;

                PointF[] point;
                PointF temp;

                ArrayList list1 = new ArrayList();

                while (!Sr.EndOfStream)
                {
                    //读取id
                    Sr.ReadLine();

                    line = Sr.ReadLine();

                    while (line != "END" && line != null)
                    {
                        
                        data = line.Split(',');
                        temp = new PointF(float.Parse(data[0]), float.Parse(data[1]));
                        list1.Add(temp);
                        line = Sr.ReadLine();
                    }
                    point = (PointF[])list1.ToArray(typeof(PointF));
                    List_2.Add(point);
                    list1 = new ArrayList();

                }

                //经纬网
                for (int i = -180; i <= 180; i += 5)
                {
                    for (int j = -85; j <= 85; j += 5)
                    {
                        temp = new PointF(i, j);
                        list1.Add(temp);
                    }
                    point = (PointF[])list1.ToArray(typeof(PointF));
                    List_2.Add(point);
                    list1 = new ArrayList();
                }

                for (int j = -85; j <= 85; j += 5)
                {
                    for (int i = -180; i <= 180; i += 5)
                    {
                        temp = new PointF(i, j);
                        list1.Add(temp);
                    }
                    point = (PointF[])list1.ToArray(typeof(PointF));
                    List_2.Add(point);
                    list1 = new ArrayList();
                }

                list1 = new ArrayList();
                Fs.Dispose();
            }

        }
        #endregion

        #region 显示WGS84坐标系
        private void button6_Click_1(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            using (Graphics g = this.richTextBox1.CreateGraphics())
            {
                richTextBox1.Refresh();
                for (int i = 0; i < List_2.Count; i++)
                {
                    
                    Copy((PointF[])List_2[i], out points);
                    if (points != null)
                    {
                        for (int j = 0; j < points.Count(); j++ )
                        {
                            points[j].X = 3 * points[j].X +500;
                            points[j].Y = -3 * points[j].Y +250;
                            npoints = points;
                        }
                        g.DrawCurve(p, npoints);
                    }
                    points = null;
                    
                }
               
            }
            npoints = null;
        }
        #endregion

        #region 世界版图墨卡托投影转换
        private void button3_Click_1(object sender, EventArgs e)
        {
            float tempx = 0; float tempy = 0;
            richTextBox1.Clear();
            using (Graphics g = this.richTextBox1.CreateGraphics())
            {
                richTextBox1.Refresh();
                for (int i = 0; i < List_2.Count; i++)
                {
                    Copy((PointF[])List_2[i], out points);
                    if (points != null)
                    {
                        for (int j = 0; j < points.Length; j++)
                        {
                            x = points[j].X;
                            points[j].X = (float)(((Math.Pow(a1, 2) / b1) / Math.Sqrt(1 + e11)) * Math.Log((Math.Tan(Math.PI / 4 + (points[j].Y * Math.PI / 180) / 2)) * Math.Pow(((1 - Math.Sqrt(e01) * Math.Sin(points[j].Y * Math.PI / 180)) / (1 + Math.Sqrt(e01) * Math.Sin(points[j].Y * Math.PI / 180))), (Math.Sqrt(e01) / 2)), Math.E));
                            points[j].Y = (float)(((Math.Pow(a1, 2) / b1) / Math.Sqrt(1 + e11)) * ((x * Math.PI / 180) - 0));

                            tempx = points[j].X;
                            tempy = points[j].Y;
                            points[j].X = 0.00002f * tempy + 480;
                            points[j].Y = 330-0.00002f * tempx ;
                            npoints = points;

                        }
                        g.DrawLines(p, npoints);
                    }
                    points = null;
                }
            }
            npoints = null;
        }
        #endregion

        #region 为计算大圆轨迹定义变量
        //////////////////////////////////////////////////////////////////////////
        //下面为WGS84中巴黎、北京的坐标（经度，纬度）
        //巴黎（2.35222222°，48.85661389°）
        //北京（116.40966670°，39.90690556°）


        //巴黎坐标
        static double LO1 = 2.35222222 ;//经度
        static double LA1 = 48.85661389 ;//纬度
        //北京坐标
        static double LO2 = 116.40966670;//经度
        static double LA2 = 39.90690556;//纬度

        static double tanLA1 = Math.Tan(LA1 / 180 * Math.PI);
        static double tanLA2 = Math.Tan(LA2 / 180 * Math.PI);
        static double sinLO1 = Math.Sin(LO1 / 180 * Math.PI);
        static double sinLO2 = Math.Sin(LO2 / 180 * Math.PI);
        static double cosLO1 = Math.Cos(LO1 / 180 * Math.PI);
        static double cosLO2 = Math.Cos(LO2 / 180 * Math.PI);
        

        //设大圆航线上的点数为1000个
        static int number = 1000;
        
        //航迹点泛型
        public List<PointF[]> routes = new List<PointF[]>();

        //计算新极点Q,角度制
        public double LOQ;//新极点经度
        public double LAQ;//新极点纬度
        #endregion

        #region 求新极点
        public void getQ()
        {
            //经度
            LOQ = Math.Atan(-(tanLA2 * cosLO1 - tanLA1 * cosLO2) / (tanLA2 * sinLO1 - tanLA1 * sinLO2)) * 180 / Math.PI;
            //纬度
            LAQ = -Math.Atan(Math.Cos((LO1 - LOQ) / 180 * Math.PI) / tanLA1) * 180 / Math.PI;
        }
        #endregion

        #region 求航迹点
        public void route()
        {
            double difference = LO2 - LO1;//北京与巴黎的经差
            double dx = difference / number;//航迹点间的经差
            PointF[] pts = new PointF[number]; //存放航迹点

            //北京到新极点的距离平方
            double rr = Math.Pow((LOQ - LO2 ), 2) + Math.Pow((LAQ - LA2 ), 2);

            for (int i = 0; i < number;i++ )
            {
                //实例化变量
                pts[i] = new PointF();
                
                //航迹点的经度
                pts[i].X = Convert.ToSingle(LO2  - i * dx);
                double tx = pts[i].X;
                
                //航迹点的纬度
                double ty = Math.Sqrt(rr - Math.Pow((LOQ - tx), 2)) + LAQ;
                pts[i].Y = Convert.ToSingle(ty);
            }
            routes.Add(pts);
        }
        #endregion

        #region 绘制北京到巴黎的大圆轨迹，将航迹点（WGS84坐标系）转换为墨卡托投影坐标
        private void button4_Click_1(object sender, EventArgs e)
        {
            richTextBox1.Refresh();
            button3_Click_1(sender, e);
            getQ(); 
            route();
            float tempx = 0; float tempy = 0;
            
            using (Graphics g = this.richTextBox1.CreateGraphics())
            {
                for (int i = 0; i < routes.Count; i++)
                {
                    Copy((PointF[])routes[i], out points);
                    if (points != null)
                    {
                        for (int j = 0; j < points.Length; j++)
                        {
                            x = points[j].X;
                            points[j].X = (float)(((Math.Pow(a1, 2) / b1) / Math.Sqrt(1 + e11)) * Math.Log((Math.Tan(Math.PI / 4 + (points[j].Y * Math.PI / 180) / 2)) * Math.Pow(((1 - Math.Sqrt(e01) * Math.Sin(points[j].Y * Math.PI / 180)) / (1 + Math.Sqrt(e01) * Math.Sin(points[j].Y * Math.PI / 180))), (Math.Sqrt(e01) / 2)), Math.E));
                            points[j].Y = (float)(((Math.Pow(a1, 2) / b1) / Math.Sqrt(1 + e11)) * ((x * Math.PI / 180) - 0));

                            tempx = points[j].X;
                            tempy = points[j].Y;
                            points[j].X = 0.00002f * tempy + 580;
                            points[j].Y = 330 - 0.00002f * tempx;
                            npoints = points;
                        }
                        g.DrawLines(np, npoints);
                    }
                    points = null;
                }
            }
            npoints = null;
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }

       


    }
}

