using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using ImageProcessing;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.ShowDialog(); //OFD.Filename
            String filename = OFD.FileName;

            //String filename = "C:\\Users\\ASUS\\Videos\\Captures\\83039715_188795668839191_8407660474925056000_n.jpg";
            Origin.Source = new BitmapImage(new Uri(filename));
            //BitmapImage image0 = new BitmapImage(new Uri(filename));
            LOCImage image0 = new LOCImage(filename, Int32Rect.Empty);

            int W = image0.Width;
            int H = image0.Height;

            Double a0, a1, a2, b0, b1, b2, t3, t4, t5, t6;
            a0 = double.Parse(TB1.Text); //X平移
            b0 = double.Parse(TB2.Text); //Y平移
            t3 = double.Parse(TB3.Text); //X尺度
            t4 = double.Parse(TB4.Text); //Y尺度
            t5 = double.Parse(TB5.Text); //旋轉角
            t6 = double.Parse(TB6.Text); //兩軸角

            a1 = t3 * Math.Cos((t6 - t5) * (Math.PI / 180)) / Math.Cos(t6 * (Math.PI / 180));
            b1 = -t3 * Math.Sin((t6 - t5) * (Math.PI / 180)) / Math.Cos(t6 * (Math.PI / 180));
            a2 = -t4 * Math.Sin(t5 * (Math.PI / 180)) / Math.Cos(t6 * (Math.PI / 180));
            b2 = t4 * Math.Cos(t5 * (Math.PI / 180)) / Math.Cos(t6 * (Math.PI / 180));
            
            int[] coorX = new int[4], coorY = new int[4];
            coorX[0] = (int)(Math.Floor(a0 + a1 * 0 + a2 * 0));
            coorY[0] = (int)(Math.Floor(b0 + b1 * 0 + b2 * 0));
            coorX[1] = (int)(Math.Floor(a0 + a1 * (W - 1) + a2 * 0));
            coorY[1] = (int)(Math.Floor(b0 + b1 * (W - 1) + b2 * 0));
            coorX[2] = (int)(Math.Floor(a0 + a1 * 0 + a2 * (H - 1)));
            coorY[2] = (int)(Math.Floor(b0 + b1 * 0 + b2 * (H - 1)));
            coorX[3] = (int)(Math.Floor(a0 + a1 * (W - 1) + a2 * (H - 1)));
            coorY[3] = (int)(Math.Floor(b0 + b1 * (W - 1) + b2 * (H - 1)));

            //for(int i = 0; i < 4; i++)
            //{
            //    if(coorX[i] < 0 && coorY[i] < 0)
            //    {
            //        coorX[i] -= 1;
            //        coorY[i] -= 1;
            //    }
            //    else if (coorX[i] < 0 && coorY[i] > 0)
            //    {
            //        coorX[i] -= 1;
            //    }
            //    else if (coorX[i] > 0 && coorY[i] < 0)
            //    {
            //        coorY[i] += 1;
            //    }
            //}

            int tranH, tranW, tmp;

            for(int i = 1; i < 4; i++)
            {
                for(int j = 0; j < 4 - i; j++)
                {
                    if(coorX[j] > coorX[j+1])
                    {
                        tmp = coorX[j];
                        coorX[j] = coorX[j + 1];
                        coorX[j+1] = tmp;
                    }
                }
            }

            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < 4 - i; j++)
                {
                    if (coorY[j] > coorY[j + 1])
                    {
                        tmp = coorY[j];
                        coorY[j] = coorY[j + 1];
                        coorY[j + 1] = tmp;
                    }
                }
            }

            tranW = coorX[3] - coorX[0];
            tranH = coorY[3] - coorY[0];
                        
            LOCImage trans = new LOCImage(tranW, tranH, image0.DpiX, image0.DpiY, PixelFormats.Bgr24, null);
            

            //最鄰近
            if(Radiobutton1.IsChecked == true)
            {
                for(int i = coorX[0]; i < coorX[3]; i++)
                {
                    int index, oriX, oriY;
                    for(int j = coorY[0]; j < coorY[3]; j++)
                    {
                        index = ((j - coorY[0]) * tranW + i - coorX[0]) * 3;
                        oriX = (int)(Math.Round((b2 * (i - a0) - a2 * (j - b0)) / (a1 * b2 - a2 * b1), 0, MidpointRounding.AwayFromZero));
                        oriY = (int)(Math.Round((-b1 * (i - a0) + a1 * (j - b0)) / (a1 * b2 - a2 * b1), 0, MidpointRounding.AwayFromZero));

                        if (oriX == W && oriY < H - 1)
                        {
                            oriX -= 1;
                        }
                        else if (oriX < W - 1 && oriY == H)
                        {
                            oriY -= 1;
                        }                            
                        else if(oriX == W && oriY == H)
                        {
                            oriX -= 1;
                            oriY -= 1;
                        }

                        if (oriX < 0 || oriX >= W || oriY < 0 || oriY >= H)
                        { }                    
                        else
                        {
                            for(int k = 0; k < 3; k++)
                            {
                                trans.ByteData[index + k] = (Byte)(image0.ByteData[(oriX + oriY * W) * 3 + k]);
                            }                        
                        }                    
                    }
                }
            
                FileInfo fi = new FileInfo(filename);
                String direc = fi.DirectoryName, file = fi.Name, transfile = direc + "\\new_" + file;
                trans.Save(transfile, ImageFormat.Tiff);
            
                using (var stream = new FileStream(transfile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    target.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }

                LOCImage backimage = new LOCImage(W, H, image0.DpiX, image0.DpiY, PixelFormats.Bgr24, null);
                int backX, backY;

                for (int i = 0; i < W; i++)
                {
                    int index2;
                    for (int j = 0; j < H; j++)
                    {
                        index2 = (j * W + i) * 3; //error
                        backX = (int)(Math.Round((a0 + a1 * i + a2 * j), 0, MidpointRounding.AwayFromZero));
                        backY = (int)(Math.Round((b0 + b1 * i + b2 * j), 0, MidpointRounding.AwayFromZero));

                        for (int k = 0; k < 3; k++)
                        {
                            if(backX < coorX[0] || backX >= coorX[3] || backY < coorY[0] || backY >= coorY[3])
                            { }
                            else
                            {
                                backimage.ByteData[index2 + k] = (Byte)(trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]);
                            }
                        }
                    }
                }

                //FileInfo fi = new FileInfo(filename);
                String backfile = direc + "\\new2_" + file; //direc = fi.DirectoryName, file = fi.Name,
                backimage.Save(backfile, ImageFormat.Tiff);

                using (var stream = new FileStream(backfile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    back.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }

                LOCImage diffima = new LOCImage(W, H, image0.DpiX, image0.DpiY, PixelFormats.Bgr24, null);
                for (int i = 0; i < W; i++)
                {
                    int index3;
                    for (int j = 0; j < H; j++)
                    {
                        index3 = (j * W + i) * 3;                     
                        for (int k = 0; k < 3; k++)
                        {
                           diffima.ByteData[index3 + k] = (Byte)(backimage.ByteData[index3 + k] - image0.ByteData[index3 + k] + 128);
                        }
                    }
                }

                String difffile = direc + "\\new3_" + file; //direc = fi.DirectoryName, file = fi.Name,
                diffima.Save(difffile, ImageFormat.Tiff);

                using (var stream = new FileStream(difffile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    back.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }

            
            //雙線性
            if (Radiobutton2.IsChecked == true)
            {
                for (int i = coorX[0]; i < coorX[3]; i++)
                {
                    int index;
                    double oriX, oriY;
                    for (int j = coorY[0]; j < coorY[3]; j++)
                    {
                        index = ((j - coorY[0]) * tranW + i - coorX[0]) * 3;
                        oriX = ((b2 * (i - a0) - a2 * (j - b0)) / (a1 * b2 - a2 * b1));
                        oriY = ((-b1 * (i - a0) + a1 * (j - b0)) / (a1 * b2 - a2 * b1));

                        if (oriX < 0 || oriX >= W || oriY < 0 || oriY >= H)
                        { }
                        else
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                if((int)oriX != W - 1 && (int)oriY != H - 1)
                                {
                                    trans.ByteData[index + k] = (Byte)(image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k]
                                        + ((image0.ByteData[((int)oriX + 1 + (int)oriY * W) * 3 + k]) - (image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k])) * (oriX - (int)oriX)
                                        + ((image0.ByteData[((int)oriX + ((int)oriY + 1) * W) * 3 + k]) - (image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k])) * (oriY - (int)oriY)
                                        + ((image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k]) - (image0.ByteData[((int)oriX + 1 + (int)oriY * W) * 3 + k])
                                            - (image0.ByteData[((int)oriX + ((int)oriY + 1) * W) * 3 + k]) + (image0.ByteData[((int)oriX + 1 + ((int)oriY + 1) * W) * 3 + k])) * (oriX - (int)oriX) * (oriY - (int)oriY));
                                }
                                else if((int)oriX == W - 1 && (int)oriY != H - 1)
                                {
                                    trans.ByteData[index + k] = (Byte)(image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k]
                                        + ((image0.ByteData[((int)oriX - 1 + (int)oriY * W) * 3 + k]) - (image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k])) * (oriX - (int)oriX)
                                        + ((image0.ByteData[((int)oriX + ((int)oriY + 1) * W) * 3 + k]) - (image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k])) * (oriY - (int)oriY)
                                        + ((image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k]) - (image0.ByteData[((int)oriX - 1 + (int)oriY * W) * 3 + k])
                                            - (image0.ByteData[((int)oriX + ((int)oriY + 1) * W) * 3 + k]) + (image0.ByteData[((int)oriX - 1 + ((int)oriY + 1) * W) * 3 + k])) * (oriX - (int)oriX) * (oriY - (int)oriY));
                                }
                                else if ((int)oriX != W - 1 && (int)oriY == H - 1)
                                {
                                    trans.ByteData[index + k] = (Byte)(image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k]
                                        + ((image0.ByteData[((int)oriX + 1 + (int)oriY * W) * 3 + k]) - (image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k])) * (oriX - (int)oriX)
                                        + ((image0.ByteData[((int)oriX + ((int)oriY - 1) * W) * 3 + k]) - (image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k])) * (oriY - (int)oriY)
                                        + ((image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k]) - (image0.ByteData[((int)oriX + 1 + (int)oriY * W) * 3 + k])
                                            - (image0.ByteData[((int)oriX + ((int)oriY - 1) * W) * 3 + k]) + (image0.ByteData[((int)oriX + 1 + ((int)oriY - 1) * W) * 3 + k])) * (oriX - (int)oriX) * (oriY - (int)oriY));
                                }
                                else if ((int)oriX == W - 1 && (int)oriY == H - 1)
                                {
                                    trans.ByteData[index + k] = (Byte)(image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k]
                                        + ((image0.ByteData[((int)oriX - 1 + (int)oriY * W) * 3 + k]) - (image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k])) * (oriX - (int)oriX)
                                        + ((image0.ByteData[((int)oriX + ((int)oriY - 1) * W) * 3 + k]) - (image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k])) * (oriY - (int)oriY)
                                        + ((image0.ByteData[((int)oriX + (int)oriY * W) * 3 + k]) - (image0.ByteData[((int)oriX - 1 + (int)oriY * W) * 3 + k])
                                            - (image0.ByteData[((int)oriX + ((int)oriY - 1) * W) * 3 + k]) + (image0.ByteData[((int)oriX - 1 + ((int)oriY - 1) * W) * 3 + k])) * (oriX - (int)oriX) * (oriY - (int)oriY));
                                }
                            }
                        }
                    }
                }

                FileInfo fi = new FileInfo(filename);
                String direc = fi.DirectoryName, file = fi.Name, transfile = direc + "\\new_" + file;
                trans.Save(transfile, ImageFormat.Tiff);

                using (var stream = new FileStream(transfile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    target.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }

                LOCImage backimage = new LOCImage(W, H, image0.DpiX, image0.DpiY, PixelFormats.Bgr24, null);
                double backX, backY;

                for (int i = 0; i < W; i++)
                {
                    int index2;
                    for (int j = 0; j < H; j++)
                    {
                        index2 = (j * W + i) * 3; //error
                        backX = (a0 + a1 * i + a2 * j);
                        backY = (b0 + b1 * i + b2 * j);

                        if (backX < 0 && backY < 0)
                        {
                                backX -= 1;
                                backY -= 1;
                        }
                        if (backX < 0 && backY > 0)
                        {
                            backX -= 1;
                        }
                        if (backX > 0 && backY < 0)
                        {
                            backY -= 1;
                        }     

                        if(backX < coorX[0] || backX >= coorX[3] || backY < coorY[0] || backY >= coorY[3])
                        { }
                        else
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                if((int)backX != tranW + coorX[0] - 1 && (int)backY != tranH + coorY[0] - 1)
                                {
                                    backimage.ByteData[index2 + k] = (Byte)(trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]
                                     + (trans.ByteData[((int)backX - coorX[0] + 1 + ((int)backY - coorY[0]) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]) * Math.Abs(backX - (int)backX)
                                     + (trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0] + 1) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]) * Math.Abs(backY - (int)backY)
                                     + (trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + 1 + ((int)backY - coorY[0]) * tranW) * 3 + k]
                                        - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0] + 1) * tranW) * 3 + k] + trans.ByteData[((int)backX - coorX[0] + 1 + ((int)backY - coorY[0] + 1) * tranW) * 3 + k]) * Math.Abs(backX - (int)backX) * Math.Abs(backY - (int)backY));
                                }
                                else if ((int)backX == tranW + coorX[0] - 1 && (int)backY != tranH + coorY[0] - 1)
                                {
                                    backimage.ByteData[index2 + k] = (Byte)(trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]
                                     + (trans.ByteData[((int)backX - coorX[0] - 1 + ((int)backY - coorY[0]) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]) * Math.Abs(backX - (int)backX)
                                     + (trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0] + 1) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]) * Math.Abs(backY - (int)backY)
                                     + (trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] - 1 + ((int)backY - coorY[0]) * tranW) * 3 + k]
                                        - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0] + 1) * tranW) * 3 + k] + trans.ByteData[((int)backX - coorX[0] - 1 + ((int)backY - coorY[0] + 1) * tranW) * 3 + k]) * Math.Abs(backX - (int)backX) * Math.Abs(backY - (int)backY));
                                }
                                else if ((int)backX != tranW + coorX[0] - 1 && (int)backY == tranH + coorY[0] - 1)
                                {
                                    backimage.ByteData[index2 + k] = (Byte)(trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]
                                     + (trans.ByteData[((int)backX - coorX[0] + 1 + ((int)backY - coorY[0]) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]) * Math.Abs(backX - (int)backX)
                                     + (trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0] - 1) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]) * Math.Abs(backY - (int)backY)
                                     + (trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + 1 + ((int)backY - coorY[0]) * tranW) * 3 + k]
                                        - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0] - 1) * tranW) * 3 + k] + trans.ByteData[((int)backX - coorX[0] + 1 + ((int)backY - coorY[0] - 1) * tranW) * 3 + k]) * Math.Abs(backX - (int)backX) * Math.Abs(backY - (int)backY));
                                }
                                else if ((int)backX == tranW + coorX[0] - 1 && (int)backY == tranH + coorY[0] - 1)
                                {
                                    backimage.ByteData[index2 + k] = (Byte)(trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]
                                     + (trans.ByteData[((int)backX - coorX[0] - 1 + ((int)backY - coorY[0]) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]) * Math.Abs(backX - (int)backX)
                                     + (trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0] - 1) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k]) * Math.Abs(backY - (int)backY)
                                     + (trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0]) * tranW) * 3 + k] - trans.ByteData[((int)backX - coorX[0] - 1 + ((int)backY - coorY[0]) * tranW) * 3 + k]
                                        - trans.ByteData[((int)backX - coorX[0] + ((int)backY - coorY[0] - 1) * tranW) * 3 + k] + trans.ByteData[((int)backX - coorX[0] - 1 + ((int)backY - coorY[0] - 1) * tranW) * 3 + k]) * Math.Abs(backX - (int)backX) * Math.Abs(backY - (int)backY));
                                }
                            }
                        }
                    }
                }

                //FileInfo fi = new FileInfo(filename);
                String backfile = direc + "\\new2_" + file; //direc = fi.DirectoryName, file = fi.Name,
                backimage.Save(backfile, ImageFormat.Tiff);

                using (var stream = new FileStream(backfile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    back.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }

                LOCImage diffima = new LOCImage(W, H, image0.DpiX, image0.DpiY, PixelFormats.Bgr24, null);
                for (int i = 0; i < W; i++)
                {
                    int index3;
                    for (int j = 0; j < H; j++)
                    {
                        index3 = (j * W + i) * 3;
                        for (int k = 0; k < 3; k++)
                        {
                            diffima.ByteData[index3 + k] = (Byte)(backimage.ByteData[index3 + k] - image0.ByteData[index3 + k] +128);// 
                        }
                    }
                }

                String difffile = direc + "\\new3_" + file; //direc = fi.DirectoryName, file = fi.Name,
                diffima.Save(difffile, ImageFormat.Tiff);

                using (var stream = new FileStream(difffile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    back.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }
        }
    }
}
