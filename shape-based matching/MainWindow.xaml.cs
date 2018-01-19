using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HalconDotNet;
using Microsoft.Win32;
using System.Threading;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls;
using System.Xml.Serialization;
using System.IO;

namespace shape_based_matching
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public delegate int CallBack(long draw_id, long window_handle, IntPtr type);
    public delegate void DisplayResultsDelegate();
    
    public partial class MainWindow : Window
    {
        HTuple MoudleID;
        HObject ModelContours  ;

        HObject RedPic, GreenPic, BluePic;
        HObject HuePic, SaturationPic, ValuePic;
        public static List<coordinate> cd;
        HDevelopExport HD = new HDevelopExport();
        string ImagePath;
        object image_lock = new object();
        DisplayResultsDelegate display_results_delegate;

        HObject ho_EdgeAmplitude;
        HObject background_image = null;

        CallBack cb;

        List<HTuple> drawing_objects;
        XmlSerializer xs;
        FilterfunctionState ReadFilterfc = new FilterfunctionState();
        public MainWindow()
        {
            InitializeComponent();
            drawing_objects = new List<HTuple>();
            cd = new List<coordinate>();

            xs = new XmlSerializer(typeof(FilterfunctionState));
            FileStream fs = new FileStream("Config.xml", FileMode.Open, FileAccess.Read);
            ReadFilterfc = (FilterfunctionState)xs.Deserialize(fs);
            fs.Close();

 
        }

       


         
        private void ReadPic_Click(object sender, RoutedEventArgs e)
        {

            //for (int i = 0; i < 60; i++)
            //{
            //    HOperatorSet.DispCircle(HwindowShow.HalconWindow, i * 10, i * 10, 5);

            //    HOperatorSet.DispCircle(HwindowShow.HalconWindow, i * 10, i * 10, 5);

            //}
             
            HD.closecamera();
            background_image = null;

            HD.clearAllDrawingObject();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "All files(*.*) | *.*| JPEG文件 |*.jpg*|BMP文件|*.bmp*|PNG文件|*.png*";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.FilterIndex = 1; //设置对话框属性
            if (openFileDialog1.ShowDialog() == true)
            {
                ImagePath = openFileDialog1.FileName;
                
                background_image = HD.readImage(HwindowShow.HalconWindow, ImagePath);

                HObject bg_Red, bg_Green, bg_Blue;
                HObject bg_H, bg_S, bg_V;
                HOperatorSet.Decompose3(background_image, out bg_Red, out bg_Green, out bg_Blue);
                HOperatorSet.TransFromRgb(bg_Red, bg_Green, bg_Blue, out bg_H, out bg_S, out bg_V, "hsv");
                
             //   HOperatorSet.Rgb1ToGray(background_image, out background_image);
                if (background_image == null)
                    return;

                if (ReadFilterfc.picturetype == "gray")
                {
                    HOperatorSet.Rgb1ToGray(background_image, out background_image);
                }
                else if (ReadFilterfc.picturetype == "R")
                {
                    background_image = bg_Red;
                }
                else if (ReadFilterfc.picturetype == "G")
                {
                    background_image = bg_Green;
                }
                else if (ReadFilterfc.picturetype == "B")
                {
                    background_image = bg_Blue;
                }
                else if (ReadFilterfc.picturetype == "H")
                {
                    background_image = bg_H;
                }
                else if (ReadFilterfc.picturetype == "S")
                {
                    background_image = bg_S;
                }
                else if (ReadFilterfc.picturetype == "V")
                {
                    background_image = bg_V;
                }
                else
                {
                      HOperatorSet.Rgb1ToGray(background_image, out background_image);
                }

                if (ReadFilterfc.binomialfilterState == true)
                {
                    HD.BinomialFilter(background_image, HwindowShow.HalconWindow,  ReadFilterfc.binomialfiltermaskvalue, ReadFilterfc.binomialfiltermaskvalue);
                }
                else if (ReadFilterfc.gaussFilterState == true)
                {
                    HD.GaussFilter(background_image, HwindowShow.HalconWindow, ReadFilterfc.gaussFiltersize);
                }
                else if (ReadFilterfc.meanImageState == true)
                {
                    HD.MeanImage(background_image, HwindowShow.HalconWindow, ReadFilterfc.meanImagemaskValue, ReadFilterfc.meanImagemaskValue);
                }
                else if (ReadFilterfc.sigmaImageState == true)
                {
                    HD.SigmaImage(background_image, HwindowShow.HalconWindow, ReadFilterfc.sigmaImagemaskValue, ReadFilterfc.sigmaImagemaskValue, ReadFilterfc.sigmaImagesigmaValue);
                }
                else if (ReadFilterfc.smoothImageState == true)
                {
                    HD.SmoothImage(background_image, HwindowShow.HalconWindow, ReadFilterfc.smoothImagefiltertype, ReadFilterfc.smoothImagealpha);
                }
                HOperatorSet.DispObj(background_image, HwindowShow.HalconWindow);
            }
            else
            {
                return;
            }
            
            HwindowShow.HalconWindow.AttachBackgroundToWindow(new HImage(background_image));

            display_results_delegate = new DisplayResultsDelegate(() => {
                lock (image_lock)
                {
                    if (ho_EdgeAmplitude != null)
                        HD.display_results(ho_EdgeAmplitude);
                    HwindowShow.HalconWindow.DispCross(-12.0, -12.0, 3.0, 0);  // 在 坐标（-12.0 ,-12.0） 画线宽为3 角度为0 的交叉线


                }
            });
            cb = new CallBack(HDrawingObjectCallback);
            HwindowShow.Focus();
        }

        private void PicGray_Click(object sender, RoutedEventArgs e)
        {
            HD.processImage();
        }

        private void ReadPic_Copy_Click(object sender, RoutedEventArgs e)
        {
            HwindowShow.Background = null;

            HOperatorSet.DetachBackgroundFromWindow(HwindowShow.HalconWindow);

            
            HOperatorSet.ClearWindow(HwindowShow.HalconWindow);
            HD.CameraDisp_Threadflag = true;
            Thread thread = new Thread(() => HD.dispCamera(HwindowShow.HalconWindow));
            thread.Start();
            
        }

        private void PicClose_Copy_Click(object sender, RoutedEventArgs e)
        {
            HD.closecamera();
        }

        protected int HDrawingObjectCallback(long draw_id, long window_handle, IntPtr type)
        {
            // On callback, process and display image
            lock (image_lock)
            {
                HOperatorSet.ClearWindow(HwindowShow.HalconWindow);
                HD.process_image(background_image, out ho_EdgeAmplitude, HwindowShow.HalconID, draw_id);
            }
            // You need to switch to the UI thread to display the results
        //    Dispatcher.BeginInvoke(display_results_delegate);
            return 0;
        }


        private void SetCallbacks(HTuple draw_id)
        {
            // Set callbacks for all relevant interactions
            drawing_objects.Add(draw_id);

            IntPtr ptr = Marshal.GetFunctionPointerForDelegate(cb);
            HOperatorSet.SetDrawingObjectCallback(draw_id, "on_resize", ptr);
            HOperatorSet.SetDrawingObjectCallback(draw_id, "on_drag", ptr);
            HOperatorSet.SetDrawingObjectCallback(draw_id, "on_attach", ptr);
            HOperatorSet.SetDrawingObjectCallback(draw_id, "on_select", ptr);
            lock (image_lock)
            {
                HOperatorSet.AttachDrawingObjectToWindow(HwindowShow.HalconID, draw_id);
            }
        }

        private void OnRectangle1_Click(object sender, RoutedEventArgs e)
        {
            // Execute context menu command:
            // Add new rectangle1 drawing object
            
            if(background_image == null)
            {
                MessageBox.Show("未有任何图片加载","warning");
                return;
            }
            HTuple draw_id;
            HD.add_new_drawing_object("rectangle1", HwindowShow.HalconID, out draw_id);

            
            SetCallbacks(draw_id);
        }

        private void OnRectangle2_Click(object sender, RoutedEventArgs e)
        {
            // Execute context menu command:
            // Add new rectangle2 drawing object
            HTuple draw_id;
            HD.add_new_drawing_object("rectangle2", HwindowShow.HalconID, out draw_id);
            SetCallbacks(draw_id);
        }

        private void OnCircle_Click(object sender, RoutedEventArgs e)
        {
            // Execute context menu command:
            // Add new circle drawing object
            HTuple draw_id;
            HD.add_new_drawing_object("circle", HwindowShow.HalconID, out draw_id);
            SetCallbacks(draw_id);
        }

        private void OnEllipse_Click(object sender, RoutedEventArgs e)
        {
            // Execute context menu command:
            // Add new ellipse drawing object
            HTuple draw_id;
            HD.add_new_drawing_object("ellipse", HwindowShow.HalconID, out draw_id);
            SetCallbacks(draw_id);
        }

        private void OnClearAllObjects_Click(object sender, RoutedEventArgs e)
        {
            if (drawing_objects == null)
                return;
            lock (image_lock)
            {
                foreach (HTuple dobj in drawing_objects)
                {
                    HOperatorSet.ClearDrawingObject(dobj);
                }
                drawing_objects.Clear();
            }
            HwindowShow.HalconWindow.ClearWindow();
        }
     
        private void Run_matching_Click(object sender, RoutedEventArgs e)
        {
            OnClearAllObjects_Click(null,null);
            cd.Clear();
            //      coordinateGrid.Items.Clear();
            coordinateGrid.ItemsSource = null;
            coordinateGrid.ItemsSource = cd;
            if (background_image == null)
            {
                MessageBox.Show("没有加载任何图片！");
                return;
            }
            if(ho_EdgeAmplitude == null)
            {
                MessageBox.Show("没有模板！");
                return;
            }
            HD.Matching(ho_EdgeAmplitude);

            coordinateGrid.ItemsSource = null;
            coordinateGrid.ItemsSource = cd;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HD.closecamera();
            Environment.Exit(1);
        }

 
 

        private void RangeSlider_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            // RangeSli = new RangeSlider();


            try
            {
                GrayMinValue.Text = RangeSli.LowerValue.ToString();
            }
            catch(Exception ex)
            {
                 MessageBox.Show(ex.ToString());
            }
            try
            {
                HD.AutoModerateGray((uint)RangeSli.LowerValue, (uint)RangeSli.UpperValue);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RangeSlider_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            try
            {
                GrayMaxValue.Text = RangeSli.UpperValue.ToString();
            }
            catch(Exception ex)
            {
                 MessageBox.Show(ex.ToString());
            }
            try
            {
                HD.AutoModerateGray((uint)RangeSli.LowerValue, (uint)RangeSli.UpperValue);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(AutoThreshold.IsChecked == true)
            {
                Thread BinaryThread = new Thread(HD.AutoThreshold);
                BinaryThread.Start();
                 
            }
        }

        private void HwindowShow_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (e.Button == MouseButton.Right)
            {
                ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
                cm.PlacementTarget = sender as Button;
                cm.IsOpen = true;
            }
            if (e.Button == MouseButton.Middle)
            {
                HTuple X = new HTuple();
                HTuple Y = new HTuple();
                HObject image;
                HTuple row = new HTuple();
                HTuple col = new HTuple();
                HTuple grayval = new HTuple();

                row = (int)e.X;
                col = (int)e.Y;
                try
                {
                    HOperatorSet.GetGrayval(background_image, col, row, out grayval);
                }
                catch (Exception ex)
                {
                    ;
                }
                MessageBox.Show("当前坐标：  X：" + row.ToString() + "  Y: " + col.ToString() + "  灰度值  " + grayval.ToString());
            }
        }

        private void Camera_screenshot_Click(object sender, RoutedEventArgs e)
        {
              
            background_image =  HD.closecamera();
            
            if (background_image == null)
            {
                MessageBox.Show("先打开摄像头再截图！！ ");
                return;
            }
            HOperatorSet.WriteImage(background_image, "jpg", 0,  ".\\1.jpg");

            
            display_results_delegate = new DisplayResultsDelegate(() => {
                lock (image_lock)
                {
                    if (ho_EdgeAmplitude != null)
                        HD.display_results(ho_EdgeAmplitude);
                    HwindowShow.HalconWindow.DispCross(-12.0, -12.0, 3.0, 0);  // 在 坐标（-12.0 ,-12.0） 画线宽为3 角度为0 的交叉线
                }
            });
            cb = new CallBack(HDrawingObjectCallback);
            HwindowShow.Focus();
        }

        private void HwindowShow_HMouseDown(object sender, HMouseEventArgsWPF e)
        {
            if (e.Button == MouseButton.Right)
            {
                ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
                cm.PlacementTarget = sender as Button;
                cm.IsOpen = true;
            }
            if (e.Button == MouseButton.Middle)
            {
                HTuple X = new HTuple();
                HTuple Y = new HTuple();
                HObject image;
                HTuple row = new HTuple();
                HTuple col = new HTuple();
                HTuple grayval = new HTuple();

                row = (int)e.X;
                col = (int)e.Y;
                try
                {
                    HOperatorSet.GetGrayval(background_image, col, row, out grayval);
                }
                catch (Exception ex)
                {
                    ;
                }
                MessageBox.Show("当前坐标：  X：" + row.ToString() + "  Y: " + col.ToString() + "  灰度值  " + grayval.ToString());
            }
        }

 

        private void MatchScoreModerate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
          //
            MatchScoreTxt.Text = MatchScoreModerate.Value.ToString();
            MatchingParameters.Score = MatchScoreModerate.Value;
            OnClearAllObjects_Click(null, null);
        }

        private void Inverse_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("inverse");
        }

        private void sqr_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("sqr");
        }

        private void inv_sqr_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("inv_sqr");
        }

        private void cube_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("cube");
        }

        private void inv_cube_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("inv_cube");
        }

        private void sqrt_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("sqrt");
        }

        private void inv_sqrt_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("inv_sqrt");
        }

        private void cubic_root_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("cubic_root");
        }

        private void inv_cubic_root_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("inv_cubic_root");
        }

        private void default_Checked(object sender, RoutedEventArgs e)
        {
            HD.PicLookUpTable("default");
        }

        private void Emphasize_Checked(object sender, RoutedEventArgs e)
        {
            HD.Emphsize();
        }

        private void EnablePicProcess_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ConvertPic1_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (e.Button == MouseButton.Right)
            {
                ContextMenu cm = this.FindResource("ConvertPic1") as ContextMenu;
                cm.PlacementTarget = sender as Button;
                cm.IsOpen = true;
            }
        }

        private void ConvertPic2_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {

        }

        private void ConvertPic3_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Pic1Gray_Click(object sender, RoutedEventArgs e)
        {
            //PicProcess PicPWindow = new PicProcess();
            //PicPWindow.ShowDialog();
        }

        private void RedPicture_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            //if (e.Button == MouseButton.Right)
            //{
            //    ContextMenu cm = this.FindResource("PicProcessing") as ContextMenu;
            //    cm.PlacementTarget = sender as Button;
            //    cm.IsOpen = true;
            //}
        }

        private void GreenPicture_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            //if (e.Button == MouseButton.Right)
            //{
            //    ContextMenu cm = this.FindResource("PicProcessing") as ContextMenu;
            //    cm.PlacementTarget = sender as Button;
            //    cm.IsOpen = true;
            //}
        }

        private void BluePicture_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {

        }

        private void HuePicture_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {

        }

        private void SaturationPicture_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {

        }

        private void ValuePicture_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {

        }

 
 

        private void PicAutoAdjustWindow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PicProcessing_Click(object sender, RoutedEventArgs e)
        {
     
             
            //{
            //    PicProcess PictureProcess = new PicProcess(SubWindowHalconID.RedPic);
            //    PictureProcess.ShowDialog();
            //}
            
        }

        private void RedPicture_HMouseDoubleClick(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if(SubWindowHalconID.RedPic == null)
            {
                MessageBox.Show("请先进行图片转换，在执行此操作！");
                return;
            }
            PicProcess PictureProcess = new PicProcess(SubWindowHalconID.RedPic,"R");
            PictureProcess.ShowDialog();
        }

        private void GreenPicture_HMouseDoubleClick(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (SubWindowHalconID.GreenPic == null)
            {
                MessageBox.Show("请先进行图片转换，在执行此操作！");
                return;
            }
            PicProcess PictureProcess = new PicProcess(SubWindowHalconID.GreenPic,"G");
            PictureProcess.ShowDialog();
        }

        private void BluePicture_HMouseDoubleClick(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (SubWindowHalconID.BluePic == null)
            {
                MessageBox.Show("请先进行图片转换，在执行此操作！");
                return;
            }
            PicProcess PictureProcess = new PicProcess(SubWindowHalconID.BluePic,"B");
            PictureProcess.ShowDialog();

        }

        private void ValuePicture_HMouseDoubleClick(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (SubWindowHalconID.ValuePic == null)
            {
                MessageBox.Show("请先进行图片转换，在执行此操作！");
                return;
            }
            PicProcess PictureProcess = new PicProcess(SubWindowHalconID.ValuePic,"V");
            PictureProcess.ShowDialog();
        }

        private void SaturationPicture_HMouseDoubleClick(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (SubWindowHalconID.SaturationPic == null)
            {
                MessageBox.Show("请先进行图片转换，在执行此操作！");
                return;
            }
            PicProcess PictureProcess = new PicProcess(SubWindowHalconID.SaturationPic,"S");
            PictureProcess.ShowDialog();
        }

        private void HuePicture_HMouseDoubleClick(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (SubWindowHalconID.HuePic == null)
            {
                MessageBox.Show("请先进行图片转换，在执行此操作！");
                return;
            }
            PicProcess PictureProcess = new PicProcess(SubWindowHalconID.HuePic,"H");
            PictureProcess.ShowDialog();
        }

        private void ReadMoudle_Click(object sender, RoutedEventArgs e)
        {

            HTuple hv_HomMat,hv_HomMat2D;
           
           
            double MoudleWidth, MoudleHeight;

            MoudleHeight = HwindowMoudle.Height;
            MoudleWidth = HwindowMoudle.Width;
            
            HOperatorSet.ClearWindow(HwindowMoudle.HalconID);

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "All files(*.*) | *.*| shm文件 |*.shm* ";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.FilterIndex = 1; //设置对话框属性
            if (openFileDialog1.ShowDialog() == true)
            {
                ImagePath = openFileDialog1.FileName;

                HOperatorSet.ReadShapeModel(ImagePath, out MoudleID);

                HOperatorSet.GetShapeModelContours(out ModelContours, MoudleID, 1);

                HOperatorSet.HomMat2dIdentity(out hv_HomMat);

                HOperatorSet.HomMat2dIdentity(out hv_HomMat2D);

                HOperatorSet.HomMat2dRotate(hv_HomMat, 0, 0, 0, out hv_HomMat);

                HOperatorSet.HomMat2dTranslate(hv_HomMat, 0, 0, out hv_HomMat);

                HOperatorSet.VectorAngleToRigid(0,0,0,100,100,0, out hv_HomMat2D);

                // HOperatorSet.AffineTransContourXld(ModelContours, out ModelContours, hv_HomMat);
                HOperatorSet.AffineTransContourXld(ModelContours, out ModelContours, hv_HomMat2D);

                HOperatorSet.SetWindowParam(HwindowMoudle.HalconID, "flush", "false");
                HOperatorSet.ClearWindow(HwindowMoudle.HalconID);
                HOperatorSet.DispObj(ModelContours, HwindowMoudle.HalconID);
                HOperatorSet.SetWindowParam(HwindowMoudle.HalconID, "flush", "true");
                HOperatorSet.FlushBuffer(HwindowMoudle.HalconID);

            }
            else
            {
                return;
            }
        }

        private void LoadMoudleMatch_Click(object sender, RoutedEventArgs e)
        {
            HTuple hv_HomMat2D;
            HTuple hv_ModelRow, hv_ModelColumn, hv_ModelAngle, hv_ModelScore;
            HTuple hv_Deg, hv_MatchingObjIdx;
            HTuple hv_HomMat;
            HObject ho_TransContours = null;
            HOperatorSet.GenEmptyObj(out ho_TransContours);
            List<coordinate> Lctest = new List<coordinate>();
 

 
            HOperatorSet.FindShapeModel(background_image, MoudleID, (new HTuple(0)).TupleRad(),
            (new HTuple(360)).TupleRad(), MatchingParameters.Score, 0, 0.5, "least_squares", (new HTuple(2)).TupleConcat(
            1), 0.9, out hv_ModelRow, out hv_ModelColumn, out hv_ModelAngle, out hv_ModelScore);

            hv_Deg = hv_ModelAngle.TupleDeg();

            HOperatorSet.SetColor(HwindowShow.HalconWindow, "red");
            HOperatorSet.SetLineWidth(HwindowShow.HalconWindow, 3);

            int totalcount = hv_ModelScore.TupleLength();


            HOperatorSet.SetColor(HwindowShow.HalconWindow, "green");

            HOperatorSet.SetLineWidth(HwindowShow.HalconWindow, 1);

            for (hv_MatchingObjIdx = 0; (int)hv_MatchingObjIdx <= totalcount - 1; hv_MatchingObjIdx = (int)hv_MatchingObjIdx + 1)
            {
                MainWindow.cd.Add(new coordinate
                {
                    Number = hv_MatchingObjIdx + 1,
                    X = hv_ModelRow.TupleSelect(hv_MatchingObjIdx),
                    Y = hv_ModelColumn.TupleSelect(hv_MatchingObjIdx),
                    Angle = hv_Deg.TupleSelect(hv_MatchingObjIdx),

                });

                Lctest.Add(new coordinate
                {
                    Number = hv_MatchingObjIdx + 1,
                    X = hv_ModelRow.TupleSelect(hv_MatchingObjIdx),
                    Y = hv_ModelColumn.TupleSelect(hv_MatchingObjIdx),
                    Angle = hv_Deg.TupleSelect(hv_MatchingObjIdx),
                });
            }

            foreach (coordinate hs in Lctest)
            {

            }
            for (int i = 0; i < totalcount; i++)
            {
                // HOperatorSet.DispCircle(hv_ExpDefaultWinHandle, hv_ModelRow.TupleSelect(i), hv_ModelColumn.TupleSelect(i), 10);
                //  HOperatorSet.GenCircle(hv_ExpDefaultWinHandle, hv_ModelRow.TupleSelect(i), hv_ModelColumn.TupleSelect(i), 10);
            }

            for (hv_MatchingObjIdx = 0; (int)hv_MatchingObjIdx <= totalcount - 1; hv_MatchingObjIdx = (int)hv_MatchingObjIdx + 1)
            {

                HOperatorSet.DispCross(HwindowShow.HalconWindow, hv_ModelRow.TupleSelect(hv_MatchingObjIdx),
                    hv_ModelColumn.TupleSelect(hv_MatchingObjIdx), 10, hv_ModelAngle.TupleSelect(hv_MatchingObjIdx));

                HOperatorSet.HomMat2dIdentity(out hv_HomMat);
                HOperatorSet.HomMat2dIdentity(out hv_HomMat2D);
                HOperatorSet.HomMat2dRotate(hv_HomMat, hv_ModelAngle.TupleSelect(hv_MatchingObjIdx),
                    0, 0, out hv_HomMat);
                HOperatorSet.HomMat2dTranslate(hv_HomMat, hv_ModelRow.TupleSelect(hv_MatchingObjIdx),
                    hv_ModelColumn.TupleSelect(hv_MatchingObjIdx), out hv_HomMat);

                HOperatorSet.VectorAngleToRigid(100, 100, 0, hv_ModelRow.TupleSelect(hv_MatchingObjIdx), hv_ModelColumn.TupleSelect(hv_MatchingObjIdx), hv_ModelAngle.TupleSelect(hv_MatchingObjIdx), out hv_HomMat2D);

                ho_TransContours.Dispose();
                //    HOperatorSet.AffineTransContourXld(ModelContours, out ho_TransContours, hv_HomMat);

                HOperatorSet.AffineTransContourXld(ModelContours, out ho_TransContours, hv_HomMat2D);

                //HOperatorSet.ClearWindow(hv_ExpDefaultWinHandle);
                //HOperatorSet.DispObj(ho_Dog, hv_ExpDefaultWinHandle);

                HOperatorSet.SetDraw(HwindowShow.HalconWindow, "fill");


                HOperatorSet.DispObj(ho_TransContours, HwindowShow.HalconWindow);


                HOperatorSet.SetWindowParam(HwindowShow.HalconWindow, "flush", "true");

                HOperatorSet.FlushBuffer(HwindowShow.HalconWindow);
            }



 
      //      HOperatorSet.ClearShapeModel(hv_ModelId);
             
        }

        private void ConvertToRGB_Click(object sender, RoutedEventArgs e)
        {
            if (background_image == null)
            {
                MessageBox.Show("请先加载图片！");
                return;
            }
            HOperatorSet.Decompose3(background_image,out RedPic,out GreenPic,out BluePic);

            HD.ShowSubPic(RedPicture.HalconWindow, RedPic);
            HD.ShowSubPic(GreenPicture.HalconWindow, GreenPic);
            HD.ShowSubPic(BluePicture.HalconWindow, BluePic);

            SubWindowHalconID.RedPictureHalconID = RedPicture.HalconID;

            SubWindowHalconID.RedPicrureHalconWindow = RedPicture.HalconWindow;

            SubWindowHalconID.RedPic = RedPic;
            SubWindowHalconID.GreenPic = GreenPic;
            SubWindowHalconID.BluePic = BluePic;
   
        }



        private void LoadOriginPic_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "All files(*.*) | *.*| JPEG文件 |*.jpg*|BMP文件|*.bmp*|PNG文件|*.png*";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.FilterIndex = 1; //设置对话框属性
            if (openFileDialog1.ShowDialog() == true)
            {
                ImagePath = openFileDialog1.FileName;

                background_image = HD.readImage(OriginalPicture.HalconWindow, ImagePath);
                if (background_image == null)
                    return;
                HOperatorSet.DispObj(background_image, OriginalPicture.HalconWindow);
            }
            else
            {
                return;
            }
        }

        private void ConvertToHSV_Click(object sender, RoutedEventArgs e)
        {
            if (background_image == null)
            {
                MessageBox.Show("请先加载图片！");
                return;
            }
            HOperatorSet.Decompose3(background_image, out RedPic, out GreenPic, out BluePic);
            HOperatorSet.TransFromRgb(RedPic, GreenPic, BluePic,out HuePic,out SaturationPic,out ValuePic, "hsv");

            HD.ShowSubPic(HuePicture.HalconWindow, HuePic);
            HD.ShowSubPic(SaturationPicture.HalconWindow, SaturationPic);
            HD.ShowSubPic(ValuePicture.HalconWindow, ValuePic);

            SubWindowHalconID.HuePic = HuePic;
            SubWindowHalconID.SaturationPic = SaturationPic;
            SubWindowHalconID.ValuePic = ValuePic;
        }

        private void OriginalPicture_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (e.Button == MouseButton.Right)
            {
                ContextMenu cm = this.FindResource("PicProcess") as ContextMenu;
                cm.PlacementTarget = sender as Button;
                cm.IsOpen = true;
            }
        }
    }

    //public class HwindowSize
    //{
    //    public double H { get; set; } = new MainWindow().HwindowShow.Height;
    //}

}
