
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;



namespace WindowsFormsApplication1
{

    public partial class MainWin : Form
    {
        public const int samples = 10;       // количество опорных точек
        public int order = 3;               // порядок многочлена
        public const int datapoints = 1000; // количество точек данных для интерполяции

        public TextBox[,] tbm;
        public TextBox[] tbs;
        public TextBox[] tbx;

        public class TMatrix
        {
            public double[,] c;
            public double[] x;
            public double[] d;

            public TMatrix(int size)
            {
                c = new double[size, size];
                x = new double[size];
                d = new double[size];
            }
        }

        TMatrix m;
        double[] yp = new double[datapoints];
        double[] xp = new double[datapoints];
        double[] y_k = new double[samples];
        double[] x_k = new double[samples];

        /*Алгоритм Гаусса для решения матричного уравнения*/
        public class TGauss
        {
            public bool calcError;
            public int maxOrder = samples;
            TMatrix m;

            void SwitchRows(int n)
            {
                double tempD;
                int i, j;
                for (i = n; i <= maxOrder - 2; i++)
                {
                    for (j = 0; j <= maxOrder - 1; j++)
                    {
                        tempD = m.c[i, j];
                        m.c[i, j] = m.c[i + 1, j];
                        m.c[i + 1, j] = tempD;
                    }
                    tempD = m.d[i];
                    m.d[i] = m.d[i + 1];
                    m.d[i + 1] = tempD; 
                }
            }

            public TGauss(int size, TMatrix mi)
            {
                maxOrder = size;
                m = mi;
            }

            /* строим R-диагональную матрицу*/
            public bool Eliminate()
            {
                int i, k, l;
                if (Math.Abs(m.c[0, 0]) < 1e-8)
                    SwitchRows(0);
                calcError = false;
                for (k = 0; k <= maxOrder - 2; k++)
                {
                    for (i = k; i <= maxOrder - 2; i++)
                    {
                        if (Math.Abs(m.c[i+1, i]) < 1e-8)
                        {
                            SwitchRows(i+1);
                        }
                        if (m.c[i + 1, k] != 0.0)
                        {
                            for (l = k + 1; l <= maxOrder - 1; l++)
                            {
                                if (!calcError)
                                {
                                    m.c[i + 1, l] = m.c[i + 1, l] * m.c[k, k] - m.c[k, l] * m.c[i + 1, k];
                                    if (m.c[i + 1, l] > 10E260)
                                    {
                                        m.c[i + 1, k] = 0;  // range overflow 
                                        calcError = true;
                                    }
                                }
                            }
                            m.d[i + 1] = m.d[i + 1] * m.c[k, k] - m.d[k] * m.c[i + 1, k];
                            m.c[i + 1, k] = 0;
                        }
                    }
                }
                return !calcError;
            }

            /*решаем уравнение с R-диагональной матрицей*/
            public void Solve()
            {
                int k, l;
                for (k = maxOrder - 1; k >= 0; k--)
                {
                    for (l = maxOrder - 1; l >= k; l--)
                    {
                        m.d[k] = m.d[k] - m.x[l] * m.c[k, l];
                    }
                    if (m.c[k, k] != 0)
                        m.x[k] = m.d[k] / m.c[k, k];
                    else
                        m.x[k] = 0;
                }
            }
        }

        
        public MainWin()
        {
            int i;
            m = new TMatrix(samples);
            tbs = new TextBox[10];
            tbx = new TextBox[10];

            InitializeComponent();

            tbs[0] = textBoxY1; tbs[1] = textBoxY2; tbs[2] = textBoxY3; tbs[3] = textBoxY4; tbs[4] = textBoxY5;
            tbs[5] = textBoxY6; tbs[6] = textBoxY7; tbs[7] = textBoxY8; tbs[8] = textBoxY9; tbs[9] = textBoxY10;

            tbx[0] = textBoxX1; tbx[1] = textBoxX2; tbx[2] = textBoxX3; tbx[3] = textBoxX4; tbx[4] = textBoxX5;
            tbx[5] = textBoxX6; tbx[6] = textBoxX7; tbx[7] = textBoxX8; tbx[8] = textBoxX9; tbx[9] = textBoxX10;

            // некоторые примеры значений для y
             y_k[0] = 9.7666;
             y_k[1] = 11.2476;
             y_k[2] = 11.832;
             y_k[3] = 11.1044;
             y_k[4] = 12.8112;
             y_k[5] = 13.728;
             y_k[6] = 13.5994;
             y_k[7] = 14.454;
             y_k[8] = 16.3936;
             y_k[9] = 16.275;

             // некоторые примеры значение для х
             x_k[0] = 0.5;
             x_k[1] = 0.55;
             x_k[2] = 0.61;
             x_k[3] = 0.65;
             x_k[4] = 0.69;
             x_k[5] = 0.74;
             x_k[6] = 0.8;
             x_k[7] = 0.84;
             x_k[8] = 0.87;
             x_k[9] = 0.9;

             if (samples < 10)
            {
                for (i = 0; i < samples; i++)
                {
                    tbs[i].Text = Convert.ToString(y_k[i]);
                    tbx[i].Text = Convert.ToString(x_k[i]);

                }
                for (i = samples; i < 10; i++)
                {
                    tbs[i].Visible = false;
                    tbx[i].Visible = false;
                }
            }
            tBOder.Text = Convert.ToString(order);
        }

        public void Interpolate(TMatrix a, int maxPoints, double maxTime)
        {
            int i, k;
            for (i = 0; i < maxPoints; i++)
            {
                yp[i] = 0;
                xp[i] = (double)(i) * maxTime / (double)maxPoints;
                for (k = 0; k < order; k++)
                {   
                    yp[i] = yp[i] + a.x[k] * Math.Pow( xp[i], (double)(k));
                }
            }
        }

        public void DrawGraph(int maxPoints, double minX, double maxX, bool doClear)
        {
            Point p1, p2;
            int j;
            double maxTime = -1.0;
            double maxVal = -1.0;
            double minVal = 1.0;
            double scalefactor;
            bool drawCeroline;
            for (j = 0; j < maxPoints - 1; j++)
            {
                if ((xp[j] >= minX) && (xp[j] <= maxX))
                {
                    if (maxTime < xp[j])
                        maxTime = xp[j];
                    if (maxVal < yp[j])
                        maxVal = yp[j];
                    if (minVal > yp[j])
                        minVal = yp[j];
                }
            }
            maxTime = maxTime * 1.05;
            maxTime = maxTime * 1.05;
            if (minVal < 0)
            {
                if (maxVal > 0)
                {
                    if (maxVal > Math.Abs(minVal))
                        scalefactor = pGraph.Height / maxVal / 2.2;
                    else
                        scalefactor = pGraph.Height / Math.Abs(minVal) / 2.2;
                }
                else
                    scalefactor = 5.0;
                drawCeroline = true;
            }
            else
            {
                scalefactor = pGraph.Height / (maxVal) / 1.1;
                drawCeroline = false;
            }
            p1 = new Point();
            p2 = new Point();
            Graphics g = pGraph.CreateGraphics();
            if (doClear)
                g.Clear(Color.White);
            Pen bluePen = new Pen(Color.Blue, 2);
            Pen redPen = new Pen(Color.Red, 2);
            Pen blackPen = new Pen(Color.Black, 2);
            if (drawCeroline)
            {
                p1.X = 0;
                p1.Y = (pGraph.Height / 2);
                p2.X = pGraph.Width;
                p2.Y = (pGraph.Height / 2);
                g.DrawLine(blackPen, p1, p2);
            }
            bluePen.Width = 1;
            if (maxTime > 0) // рисуем интр граф
            {
                for (j = 0; j < maxPoints - 1; j++)
                {
                    if ((xp[j] >= minX) && (xp[j] <= maxX))
                    {
                        p1.X = Convert.ToInt32(xp[j] * pGraph.Width / (double)maxTime);
                        p2.X = Convert.ToInt32(xp[j + 1] * pGraph.Width / (double)maxTime);
                        if (drawCeroline)
                        {
                            p1.Y = (pGraph.Height / 2) - Convert.ToInt32(Math.Round(yp[j] * scalefactor));
                            p2.Y = (pGraph.Height / 2) - Convert.ToInt32(Math.Round(yp[j + 1] * scalefactor));
                        }
                        else
                        {
                            p1.Y = (pGraph.Height) - Convert.ToInt32(Math.Round(yp[j] * scalefactor));
                            p2.Y = (pGraph.Height) - Convert.ToInt32(Math.Round(yp[j + 1] * scalefactor));
                        }
                        if (p2.X > p1.X)
                            g.DrawLine(bluePen, p1, p2);
                    }
                }
                for (j = 0; j < samples; j++) // отметить контрольные точки
                {
                    p1.X = Convert.ToInt32(x_k[j] * pGraph.Width / (double)maxTime);
                    if (drawCeroline)
                        p1.Y = (pGraph.Height / 2) - Convert.ToInt32(Math.Round(y_k[j] * scalefactor));
                    else
                        p1.Y = (pGraph.Height) - Convert.ToInt32(Math.Round(y_k[j] * scalefactor));
                    g.DrawEllipse(redPen, (int)(p1.X - 1.0), (int)(p1.Y - 1.0), 2, 2);
                }
            }
            g.Dispose();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            int i, j, k;
            order = Convert.ToInt32(tBOder.Text);
            if (order > samples)
                order = samples;
            if (order <= 1)
                order = 1;
            tBOder.Text = Convert.ToString(order);
            TMatrix a = new TMatrix(order);
            TGauss gauss = new TGauss(order, a);
            if (samples < 10)
            {
                for (i = 0; i < samples; i++)
                {
                    if (tbs[i].Text != "")
                    {
                        y_k[i] = Convert.ToDouble(tbs[i].Text);
                        x_k[i] = Convert.ToDouble(tbx[i].Text);
                    }
                    else
                        y_k[i] = 0.0;
                    m.x[i] = 0.0;
                }
            }

            // заполняем матрицу
            for (i = 0; i < samples; i++)
            {
                m.d[i] = y_k[i];
                for (k = 0; k < order; k++)
                {
                    m.c[i, k] = Math.Pow(x_k[i], k);
                }   
            }

            // a * транспонировано (a) и y * транспонировано (a)
            for (i = 0; i < order; i++)
            {
                a.d[i] = 0.0;
                for (k = 0; k < samples; k++)
                    a.d[i] = a.d[i] + m.c[k, i] * m.d[k];

                for (j = 0; j < order; j++)
                {
                    a.c[j,i] = 0.0;
                    for (k = 0; k < samples; k++)
                    {
                        a.c[j, i] = a.c[j, i] + m.c[k,j] * m.c[k, i];
                    }
                }
            }

            /*решаем матричное уравнение по Гауссу*/
            if (gauss.Eliminate())
            {
                gauss.Solve();
                Interpolate(a, datapoints, x_k[samples - 1]);
                DrawGraph(datapoints, x_k[0], x_k[samples - 1], true);
            }
            else
                MessageBox.Show("Matrix calculattion", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void pGraph_Paint(object sender, PaintEventArgs e)
        {
            DrawGraph(datapoints, x_k[0], x_k[samples - 1], false);
        }  
    }
}
