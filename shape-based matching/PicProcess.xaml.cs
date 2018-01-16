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
using System.Windows.Shapes;
using HalconDotNet;
using Microsoft.Win32;
using System.Threading;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls;
namespace shape_based_matching
{
    /// <summary>
    /// Interaction logic for PicProcess.xaml
    /// </summary>
    public partial class PicProcess : Window
    {
        HTuple hv_Width, hv_Height;

        HTuple hv_ModelId;
        HObject HalconPic;
        CallBack cb;

        List<HTuple> drawing_objects;
        HDevelopExport HD = new HDevelopExport();
        object image_lock = new object();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           

            HOperatorSet.GetImageSize(HalconPic, out hv_Width, out hv_Height);

            HOperatorSet.SetPart(OriginPic.HalconWindow, 0, 0, hv_Height - 1, hv_Width - 1);
            HOperatorSet.SetPart(ProcessingPic.HalconWindow, 0, 0, hv_Height - 1, hv_Width - 1);
            HOperatorSet.DispObj(HalconPic, OriginPic.HalconWindow);
            HOperatorSet.DispObj(HalconPic, ProcessingPic.HalconWindow);
            ProcessingPic.HalconWindow.AttachBackgroundToWindow(new HImage(HalconPic));
        }

        private void RangeSlider_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            try
            {
                GrayMaxValue.Text = RangeSli.UpperValue.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            try
            {
                HD.AutoModerateGray(HalconPic, ProcessingPic.HalconWindow, (uint)RangeSli.LowerValue, (uint)RangeSli.UpperValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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
                HOperatorSet.AttachDrawingObjectToWindow(ProcessingPic.HalconID, draw_id);
            }
        }
        private void OnRectangle1_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessingPic.HalconWindow == null)
            {
                MessageBox.Show("未有任何图片加载", "warning");
                return;
            }
            HTuple draw_id;
            HD.add_new_drawing_object("rectangle1", ProcessingPic.HalconID, out draw_id);
            SetCallbacks(draw_id);
        }

        private void OnRectangle2_Click(object sender, RoutedEventArgs e)
        {
            // Execute context menu command:
            // Add new rectangle2 drawing object
            HTuple draw_id;
            HD.add_new_drawing_object("rectangle2", ProcessingPic.HalconID, out draw_id);
            SetCallbacks(draw_id);
        }

        private void OnCircle_Click(object sender, RoutedEventArgs e)
        {
            // Execute context menu command:
            // Add new circle drawing object
            HTuple draw_id;
            HD.add_new_drawing_object("circle", ProcessingPic.HalconID, out draw_id);
            SetCallbacks(draw_id);
        }

        private void OnEllipse_Click(object sender, RoutedEventArgs e)
        {
            // Execute context menu command:
            // Add new ellipse drawing object
            HTuple draw_id;
            HD.add_new_drawing_object("ellipse", ProcessingPic.HalconID, out draw_id);
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
            ProcessingPic.HalconWindow.ClearWindow();
        }

        private void RangeSlider_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            try
            {
                GrayMinValue.Text = RangeSli.LowerValue.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            try
            {
                HD.AutoModerateGray(HalconPic, ProcessingPic.HalconWindow, (uint)RangeSli.LowerValue, (uint)RangeSli.UpperValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Emphasize_Checked(object sender, RoutedEventArgs e)
        {
            HD.Emphsize();
        }

        private void ProcessingPic_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (e.Button == MouseButton.Right)
            {
                ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
                cm.PlacementTarget = sender as Button;
                cm.IsOpen = true;
            }
        }

        public PicProcess(HObject halconpic)
        {
            InitializeComponent();
            drawing_objects = new List<HTuple>();
            HalconPic = halconpic;
            cb = new CallBack(HDrawingObjectCallback);

            
        }

        private void EnablePicProcess_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void SaveMoudle_Click(object sender, RoutedEventArgs e)
        {
            /// write_shape_model
            /// 
            if(hv_ModelId == null)
            {
                MessageBox.Show("请先制作模板");
                return;
            }
            HOperatorSet.WriteShapeModel(hv_ModelId, "MoudleSaved.shm");
            MessageBox.Show("保存成功！");

        }

        private void RecoverToDefaultPic_Checked(object sender, RoutedEventArgs e)
        {

        }

        protected int HDrawingObjectCallback(long draw_id, long window_handle, IntPtr type)
        {
            // On callback, process and display image
            HObject ho_EdgeAmplitude;
            lock (image_lock)
            {
                HOperatorSet.ClearWindow(ProcessingPic.HalconWindow);
                hv_ModelId = HD.process_image(HalconPic, out ho_EdgeAmplitude, ProcessingPic.HalconID, draw_id);
            }
            // You need to switch to the UI thread to display the results
            //    Dispatcher.BeginInvoke(display_results_delegate);
            return 0;
        }
    }
}
