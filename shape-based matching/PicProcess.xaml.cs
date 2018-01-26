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
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

namespace shape_based_matching
{
    /// <summary>
    /// Interaction logic for PicProcess.xaml
    /// </summary>
    public partial class PicProcess : Window
    {
        XmlSerializer xs;
        HObject Global_ProcessingPic = new HObject();
        HObject EmphasizePicture = new HObject();
        HTuple hv_Width, hv_Height;

        HTuple hv_ModelId;
        HObject HalconPic;
        CallBack cb;

        List<HTuple> drawing_objects;
        HDevelopExport HD = new HDevelopExport();
        object image_lock = new object();
        bool MoudlePicProcessingFlag = false;
        HObject Filter_ProcessingPic = new HObject();
        string CurrentStepGroupBox = "";
        FilterfunctionState Filterfc = new FilterfunctionState
        {
            picturetype = "gray",
            meanImageState = false,
            meanImagemaskValue = 5,

            medianImageState = false,
            medianImagemasktype = "circle",
            medianImageradiusvalue = 1,

            smoothImageState = false,
            smoothImagefiltertype = "derich1",
            smoothImagealpha = 0.5,

            binomialfilterState = false,
            binomialfiltermaskvalue = 1,

            sigmaImageState = false,
            sigmaImagemaskValue = 5,
            sigmaImagesigmaValue =3,

            gaussFilterState = false,
            gaussFiltersize = 5
        };


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           

            HOperatorSet.GetImageSize(HalconPic, out hv_Width, out hv_Height);

            HOperatorSet.SetPart(OriginPic.HalconWindow, 0, 0, hv_Height - 1, hv_Width - 1);
            HOperatorSet.SetPart(ProcessingPic.HalconWindow, 0, 0, hv_Height - 1, hv_Width - 1);
            HOperatorSet.DispObj(HalconPic, OriginPic.HalconWindow);
            HOperatorSet.DispObj(HalconPic, ProcessingPic.HalconWindow);

            
            Global_ProcessingPic = HalconPic;
            ProcessingPic.HalconWindow.AttachBackgroundToWindow(new HImage(HalconPic));

            //firstMeanImageMaskValue.SelectionChanged += MeanImageMaskValue_SelectionChanged;
            //firstmedianImageMaskType.SelectionChanged += medianImageMaskType_SelectionChanged;
            //firstmedianImageRadius.SelectionChanged += medianImageRadius_SelectionChanged;
            //firstsmoothImageFilter.SelectionChanged += smoothImageFilter_SelectionChanged;
            //firstsmoothImageAlpha.SelectionChanged += smoothImageAlpha_SelectionChanged;
            //firstbinomialfilterMaskValue.SelectionChanged += binomialfilterMaskValue_SelectionChanged;
            //firstsigmaImagesigmaMaskValue.SelectionChanged += sigmaImagesigmaMaskValue_SelectionChanged;
            //firstsigmaImageSigmaValue.SelectionChanged += sigmaImageSigmaValue_SelectionChanged;
            //firstgaussfiltersize.SelectionChanged += gaussfiltersize_SelectionChanged;

            xs = new XmlSerializer(typeof(FilterfunctionState));
            FileStream fs = new FileStream("Config.xml", FileMode.Create, FileAccess.Write);
   
            xs.Serialize(fs, Filterfc);
        
            fs.Close();

            FirstStep.SelectionChanged += PicProcessingComboBox_SelectionChanged;
            SecondStep.SelectionChanged += PicProcessingComboBox_SelectionChanged;
            ThirdStep.SelectionChanged += PicProcessingComboBox_SelectionChanged;
            FourthStep.SelectionChanged += PicProcessingComboBox_SelectionChanged;
            FivethStep.SelectionChanged += PicProcessingComboBox_SelectionChanged;
            SixthStep.SelectionChanged += PicProcessingComboBox_SelectionChanged;

            Thread MoudleProcessing = new Thread(MoudleMaking);
            MoudleProcessing.Start();
            MoudlePicProcessingFlag = true;
        }

        public void MoudleMaking()
        {
            int FirstStepComboxIndex = 0, SecondStepComboxIndex = 0, ThirdStepComboxIndex = 0, FourthStepComboxIndex = 0, FivethStepComboxIndex = 0, SixthStepComboxIndex = 0;
            string FirstStepComboxValue = null, SecondStepComboxValue = null, ThirdStepComboxValue = null, FourthStepComboxValue = null, FivethStepComboxValue = null, SixthStepComboxValue = null;
            HObject firststeppic = HalconPic, secondsteppic = firststeppic, thirdsteppic = secondsteppic, fourthsteppic = thirdsteppic, fivethsteppic = fourthsteppic, sixthsteppic = fivethsteppic;
            HObject laststeppic = sixthsteppic;
            while (MoudlePicProcessingFlag)
            {

                Thread.Sleep(100);
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    FirstStepComboxIndex = FirstStep.SelectedIndex;
                    SecondStepComboxIndex = SecondStep.SelectedIndex;
                    ThirdStepComboxIndex = ThirdStep.SelectedIndex;
                    FourthStepComboxIndex = FourthStep.SelectedIndex;
                    FivethStepComboxIndex = FivethStep.SelectedIndex;
                    SixthStepComboxIndex = SixthStep.SelectedIndex;

                    ComboBoxItem selectedItem = (ComboBoxItem)(FirstStep.SelectedValue);
                    FirstStepComboxValue = selectedItem.Content.ToString();

                    ComboBoxItem selectedItem2 = (ComboBoxItem)(SecondStep.SelectedValue);
                    SecondStepComboxValue = selectedItem2.Content.ToString();

                    ComboBoxItem selectedItem3 = (ComboBoxItem)(ThirdStep.SelectedValue);
                    ThirdStepComboxValue = selectedItem3.Content.ToString();

                    ComboBoxItem selectedItem4 = (ComboBoxItem)(FourthStep.SelectedValue);
                    FourthStepComboxValue = selectedItem4.Content.ToString();

                    ComboBoxItem selectedItem5 = (ComboBoxItem)(FivethStep.SelectedValue);
                    FivethStepComboxValue = selectedItem5.Content.ToString();

                    ComboBoxItem selectedItem6 = (ComboBoxItem)(SixthStep.SelectedValue);
                    SixthStepComboxValue = selectedItem6.Content.ToString();
                }));
                if (FirstStepComboxIndex != 0)
                {

                    Type thisType = this.GetType();
                    MethodInfo theMethod = thisType.GetMethod(FirstStepComboxValue);
                    secondsteppic = (HObject)theMethod.Invoke(this, new[] { firststeppic });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        secondsteppic = firststeppic;
                        HOperatorSet.DispObj(firststeppic, ProcessingPic.HalconWindow);
                    }));
                }
                if (SecondStepComboxIndex != 0)
                {
                    Type thisType = this.GetType();
                    MethodInfo theMethod = thisType.GetMethod(SecondStepComboxValue);
                    thirdsteppic = (HObject)theMethod.Invoke(this, new[] { secondsteppic });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        thirdsteppic = secondsteppic;
                        HOperatorSet.DispObj(secondsteppic, ProcessingPic.HalconWindow);
                    }));
                }

                if (ThirdStepComboxIndex != 0)
                {
                    Type thisType = this.GetType();
                    MethodInfo theMethod = thisType.GetMethod(ThirdStepComboxValue);
                    fourthsteppic = (HObject)theMethod.Invoke(this, new[] { thirdsteppic });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        fourthsteppic = thirdsteppic;
                        HOperatorSet.DispObj(thirdsteppic, ProcessingPic.HalconWindow);
                    }));
                }
                if (FourthStepComboxIndex != 0)
                {
                    Type thisType = this.GetType();
                    MethodInfo theMethod = thisType.GetMethod(FourthStepComboxValue);
                    fivethsteppic = (HObject)theMethod.Invoke(this, new[] { fourthsteppic });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        fivethsteppic = fourthsteppic;
                        HOperatorSet.DispObj(fourthsteppic, ProcessingPic.HalconWindow);
                    }));
                }
                if (FivethStepComboxIndex != 0)
                {
                    Type thisType = this.GetType();
                    MethodInfo theMethod = thisType.GetMethod(FivethStepComboxValue);
                    sixthsteppic = (HObject)theMethod.Invoke(this, new[] { fivethsteppic });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        sixthsteppic = fivethsteppic;
                        HOperatorSet.DispObj(fivethsteppic, ProcessingPic.HalconWindow);
                    }));
                }
                if (SixthStepComboxIndex != 0)
                {
                    Type thisType = this.GetType();
                    MethodInfo theMethod = thisType.GetMethod(FivethStepComboxValue);
                    laststeppic = (HObject)theMethod.Invoke(this, new[] { sixthsteppic });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        laststeppic = sixthsteppic;
                        HOperatorSet.DispObj(sixthsteppic, ProcessingPic.HalconWindow);
                    }));
                }


            }
        }

        public HObject meanImage(HObject pic)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ComboBox MeanImageMaskValue = (ComboBox)FindName(CurrentStepGroupBox + "MeanImageMaskValue");
                if (MeanImageMaskValue == null)
                    return;
               
                ComboBoxItem selectedItem = (ComboBoxItem)(MeanImageMaskValue.SelectedValue);
                int MaskWidth = Convert.ToInt32(selectedItem.Content);
                int MaskHeight = Convert.ToInt32(selectedItem.Content);
                Global_ProcessingPic = HD.MeanImage(pic, null, MaskWidth, MaskHeight);
                HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
               
            }));
            return Global_ProcessingPic;
        }

        public HObject medianImage(HObject pic)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ComboBox medianImageMaskType = (ComboBox)FindName(CurrentStepGroupBox + "medianImageMaskType");
                if (medianImageMaskType == null)
                    return;
                ComboBox medianImageRadius = (ComboBox)FindName(CurrentStepGroupBox + "medianImageRadius");
                if (medianImageRadius == null)
                    return;
                ComboBoxItem MaskType_selectedItem = (ComboBoxItem)(medianImageMaskType.SelectedValue);
            ComboBoxItem Radius_selectedItem = (ComboBoxItem)(medianImageRadius.SelectedValue);

            string MaskType = (MaskType_selectedItem.Content).ToString();
            int Radius = Convert.ToInt32(Radius_selectedItem.Content);

            Global_ProcessingPic = HD.MedianImage(pic, ProcessingPic.HalconWindow, MaskType, Radius, "mirrored");
            HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            }));
            return Global_ProcessingPic;
        }

        public HObject smoothImage(HObject pic)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ComboBox smoothImageFilter = (ComboBox)FindName(CurrentStepGroupBox + "smoothImageFilter");
                if (smoothImageFilter == null)
                    return;
                ComboBox smoothImageAlpha = (ComboBox)FindName(CurrentStepGroupBox + "smoothImageAlpha");
                if (smoothImageAlpha == null)
                    return;

                ComboBoxItem smoothImageFilter_selectedItem = (ComboBoxItem)(smoothImageFilter.SelectedValue);
                ComboBoxItem smoothImageAlpha_selectedItem = (ComboBoxItem)(smoothImageAlpha.SelectedValue);

                string filtertype = smoothImageFilter_selectedItem.Content.ToString();
                double Alpha = Convert.ToDouble(smoothImageAlpha_selectedItem.Content);

                Global_ProcessingPic = HD.SmoothImage(HalconPic, ProcessingPic.HalconWindow, filtertype, Alpha);

                HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            }));
            return Global_ProcessingPic;
        }

        public HObject binomialfilter(HObject pic)
        {


            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ComboBox binomialfilterMaskValue = (ComboBox)FindName(CurrentStepGroupBox + "binomialfilterMaskValue");
                if (binomialfilterMaskValue == null)
                    return;

                ComboBoxItem binomialfilterMaskValue_selectedItem = (ComboBoxItem)(binomialfilterMaskValue.SelectedValue);

                int MaskValue = Convert.ToInt32(binomialfilterMaskValue_selectedItem.Content);

                Global_ProcessingPic = HD.BinomialFilter(HalconPic, ProcessingPic.HalconWindow, MaskValue, MaskValue);
                HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            }));
            return Global_ProcessingPic;
        }
        public HObject sigmaImage(HObject pic)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ComboBox sigmaImagesigmaMaskValue = (ComboBox)FindName(CurrentStepGroupBox + "sigmaImagesigmaMaskValue");
                if (sigmaImagesigmaMaskValue == null)
                    return;
                ComboBox sigmaImageSigmaValue = (ComboBox)FindName(CurrentStepGroupBox + "sigmaImageSigmaValue");
                if (sigmaImageSigmaValue == null)
                    return;

                ComboBoxItem sigmaImagesigmaMaskValue_selectedItem = (ComboBoxItem)(sigmaImagesigmaMaskValue.SelectedValue);
                ComboBoxItem sigmaImageSigmaValue_selectedItem = (ComboBoxItem)(sigmaImageSigmaValue.SelectedValue);

                int sigmaMaskValue = Convert.ToInt32(sigmaImagesigmaMaskValue_selectedItem.Content);
                int sigmaValue = Convert.ToInt32(sigmaImageSigmaValue_selectedItem.Content);

                Global_ProcessingPic = HD.SigmaImage(HalconPic, ProcessingPic.HalconWindow, sigmaMaskValue, sigmaMaskValue, sigmaValue);
                HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
                
            }));
            return Global_ProcessingPic;
        }

        public HObject gaussfilter(HObject pic)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ComboBox gaussfiltersize = (ComboBox)FindName(CurrentStepGroupBox + "gaussfiltersize");
                if (gaussfiltersize == null)
                    return;
                ComboBoxItem gaussfiltersize_selectedItem = (ComboBoxItem)(gaussfiltersize.SelectedValue);
                int size = Convert.ToInt32(gaussfiltersize_selectedItem.Content);

                Global_ProcessingPic = HD.GaussFilter(HalconPic, ProcessingPic.HalconWindow, size);
                HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            }));
            return Global_ProcessingPic;
        }

    private void gaussfiltersize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gaussfilter_Checked(null, null);
        }

        private void sigmaImageSigmaValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sigmaImage_Checked(null,null);
        }

        private void sigmaImagesigmaMaskValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sigmaImage_Checked(null, null);
        }

        private void binomialfilterMaskValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            binomialfilter_Checked(null,null);
        }

        private void smoothImageAlpha_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            smoothImage_Checked(null,null);
        }

        private void smoothImageFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            smoothImage_Checked(null, null);
        }

        private void medianImageRadius_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            medianImage_Checked(null,null);
        }

        private void medianImageMaskType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            medianImage_Checked(null,null);
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
            //if (Emphasize.IsChecked == true)
            //{
            //    EmphasizePicture = Global_ProcessingPic;
            //    Global_ProcessingPic = HD.Emphsize(EmphasizePicture, ProcessingPic.HalconWindow);
            //}
            //else
            //{
            //    Global_ProcessingPic = EmphasizePicture;
            //    HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            //}
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

        public PicProcess(HObject halconpic,string picturetype)
        {
            InitializeComponent();
            drawing_objects = new List<HTuple>();
            HalconPic = halconpic;
            cb = new CallBack(HDrawingObjectCallback);
            Filterfc.picturetype = picturetype;

           

           
        }
        public PicProcess()
        {
    
            
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


       //     Filterfc.binomialfilterState = binomialfilter.IsChecked ?? false;
       //     ComboBoxItem binomialfilterMaskValue_selectedItem = (ComboBoxItem)(binomialfilterMaskValue.SelectedValue);
       //     Filterfc.binomialfiltermaskvalue = Convert.ToInt32(binomialfilterMaskValue_selectedItem.Content);
             
            
            
       //     ComboBoxItem gaussfiltersize_selectedItem = (ComboBoxItem)(gaussfiltersize.SelectedValue);
       //     Filterfc.gaussFiltersize = Convert.ToInt32(gaussfiltersize_selectedItem.Content);
       //     Filterfc.gaussFilterState = gaussfilter.IsChecked ?? false;

       //     ComboBoxItem selectedItem = (ComboBoxItem)(MeanImageMaskValue.SelectedValue);
       //     Filterfc.meanImagemaskValue = Convert.ToInt32(selectedItem.Content);
       ////     Filterfc.meanImageState = meanImage.IsChecked ?? false;

       //     ComboBoxItem MaskType_selectedItem = (ComboBoxItem)(medianImageMaskType.SelectedValue);
       //     ComboBoxItem Radius_selectedItem = (ComboBoxItem)(medianImageRadius.SelectedValue);
       //     Filterfc.medianImagemasktype = (MaskType_selectedItem.Content).ToString();
       //     Filterfc.medianImageradiusvalue = Convert.ToInt32(Radius_selectedItem.Content);           
       //     //Filterfc.medianImageState = medianImage.IsChecked ?? false;

       //     ComboBoxItem sigmaImagesigmaMaskValue_selectedItem = (ComboBoxItem)(sigmaImagesigmaMaskValue.SelectedValue);
       //     ComboBoxItem sigmaImageSigmaValue_selectedItem = (ComboBoxItem)(sigmaImageSigmaValue.SelectedValue);
       //     Filterfc.sigmaImagemaskValue = Convert.ToInt32(sigmaImagesigmaMaskValue_selectedItem.Content);
       //     Filterfc.sigmaImagesigmaValue = Convert.ToInt32(sigmaImageSigmaValue_selectedItem.Content);                    
       //     Filterfc.sigmaImageState = sigmaImage.IsChecked ?? false;


       //     ComboBoxItem smoothImageFilter_selectedItem = (ComboBoxItem)(smoothImageFilter.SelectedValue);
       //     ComboBoxItem smoothImageAlpha_selectedItem = (ComboBoxItem)(smoothImageAlpha.SelectedValue);
       //     Filterfc.smoothImagefiltertype = smoothImageFilter_selectedItem.Content.ToString();
       //     Filterfc.smoothImagealpha = Convert.ToDouble(smoothImageAlpha_selectedItem.Content);
       //     Filterfc.smoothImageState = smoothImage.IsChecked ?? false;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "(*.xml) | *.xml"; // or just "txt files (*.txt)|*.txt" if you only want to save text files
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == true)
            {
                string filename = saveFileDialog1.FileName;
                using (FileStream fc = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    xs = new XmlSerializer(typeof(FilterfunctionState));
                    xs.Serialize(fc, Filterfc);
                    fc.Close();
                    MessageBox.Show("配置文件保存成功！");
                }
            }

           
            saveFileDialog1.Filter = "(*.shm) | *.shm"; // or just "txt files (*.txt)|*.txt" if you only want to save text files
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == true)
            {   
                 string filename = saveFileDialog1.FileName;
                 HOperatorSet.WriteShapeModel(hv_ModelId, filename);
                MessageBox.Show("模板保存成功！");
            }

            
            

        }

        private void RecoverToDefaultPic_Checked(object sender, RoutedEventArgs e)
        {
            
      //      Global_ProcessingPic = HalconPic;
      //      HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
        }

        private void meanImage_Checked(object sender, RoutedEventArgs e)
        {
            //if(meanImage.IsChecked == true)
            //{
               
            //    ComboBoxItem selectedItem = (ComboBoxItem)(MeanImageMaskValue.SelectedValue);
            //    int MaskWidth = Convert.ToInt32(selectedItem.Content);
            //    int MaskHeight = Convert.ToInt32(selectedItem.Content);
            //    Global_ProcessingPic =   HD.MeanImage(HalconPic, ProcessingPic.HalconWindow, MaskWidth, MaskHeight);
            //    HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            //}
        }

        private void medianImage_Checked(object sender, RoutedEventArgs e)
        {
            //if(medianImage.IsChecked == true)
            //{
            //    ComboBoxItem MaskType_selectedItem = (ComboBoxItem)(medianImageMaskType.SelectedValue);
            //    ComboBoxItem Radius_selectedItem = (ComboBoxItem)(medianImageRadius.SelectedValue);

            //    string MaskType = (MaskType_selectedItem.Content).ToString();
            //    int Radius = Convert.ToInt32(Radius_selectedItem.Content);

            //    Global_ProcessingPic = HD.MedianImage(HalconPic, ProcessingPic.HalconWindow, MaskType, Radius, "mirrored");
            //    HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            //}
        }

        private void smoothImage_Checked(object sender, RoutedEventArgs e)
        {
         //   if(smoothImage.IsChecked == true)
            {
                //ComboBoxItem smoothImageFilter_selectedItem = (ComboBoxItem)(smoothImageFilter.SelectedValue);
                //ComboBoxItem smoothImageAlpha_selectedItem = (ComboBoxItem)(smoothImageAlpha.SelectedValue);

                //string filtertype = smoothImageFilter_selectedItem.Content.ToString();
                //double Alpha = Convert.ToDouble(smoothImageAlpha_selectedItem.Content);

                //Global_ProcessingPic = HD.SmoothImage(HalconPic, ProcessingPic.HalconWindow, filtertype, Alpha);

                //HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            }
        }

        private void binomialfilter_Checked(object sender, RoutedEventArgs e)
        {
            //if(binomialfilter.IsChecked == true)
            //{
            //    ComboBoxItem binomialfilterMaskValue_selectedItem = (ComboBoxItem)(binomialfilterMaskValue.SelectedValue);

            //    int MaskValue = Convert.ToInt32(binomialfilterMaskValue_selectedItem.Content);

            //    Global_ProcessingPic = HD.BinomialFilter(HalconPic, ProcessingPic.HalconWindow, MaskValue, MaskValue);
            //    HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            //}
        }

        private void sigmaImage_Checked(object sender, RoutedEventArgs e)
        {
            //if(sigmaImage.IsChecked == true)
            //{
                 
            //    ComboBoxItem sigmaImagesigmaMaskValue_selectedItem = (ComboBoxItem)(sigmaImagesigmaMaskValue.SelectedValue);
            //    ComboBoxItem sigmaImageSigmaValue_selectedItem = (ComboBoxItem)(sigmaImageSigmaValue.SelectedValue);

            //    int sigmaMaskValue = Convert.ToInt32(sigmaImagesigmaMaskValue_selectedItem.Content);
            //    int sigmaValue = Convert.ToInt32(sigmaImageSigmaValue_selectedItem.Content);

            //    Global_ProcessingPic = HD.SigmaImage(HalconPic, ProcessingPic.HalconWindow, sigmaMaskValue, sigmaMaskValue, sigmaValue);
            //    HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            //}
        }

        private void gaussfilter_Checked(object sender, RoutedEventArgs e)
        {
            //if(gaussfilter.IsChecked  == true)
            //{        
            //    ComboBoxItem gaussfiltersize_selectedItem = (ComboBoxItem)(gaussfiltersize.SelectedValue);
            //    int size = Convert.ToInt32(gaussfiltersize_selectedItem.Content);

            //    Global_ProcessingPic = HD.GaussFilter(HalconPic, ProcessingPic.HalconWindow, size);
            //    HOperatorSet.DispObj(Global_ProcessingPic, ProcessingPic.HalconWindow);
            //}



        }

 

        private void MeanImageMaskValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            meanImage_Checked(null, null);
        }
        //private Control FindControlByName(string name)
        //{
        //    foreach (Control c in this.Controls)
        //    {
        //        if (c.Name == name)
        //            return c; //found
        //    }
        //    return null; //not found
        //}
        private void PicProcessingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            //     ComboBoxItem selectItem = (ComboBoxItem)((ComboBox)sender.SelectedValue);
            //ComboBoxItem selectitem = (ComboBoxItem)(binomialfilterMaskValue.SelectedValue);
            //Filterfc.binomialfiltermaskvalue = Convert.ToInt32(binomialfilterMaskValue_selectedItem.Content);
            string comboxName = ((ComboBox)(sender)).Name;
   
            GroupBox ControlmeanImage = (GroupBox)FindName(comboxName + "meanImage");
            ControlmeanImage.Visibility = System.Windows.Visibility.Hidden;
            GroupBox ControlmedianImage = (GroupBox)FindName(comboxName + "medianImage");
            ControlmedianImage.Visibility = System.Windows.Visibility.Hidden;
            GroupBox ControlsmoothImage = (GroupBox)FindName(comboxName + "smoothImage");
            ControlsmoothImage.Visibility = System.Windows.Visibility.Hidden;
            GroupBox Controlbinomialfilter = (GroupBox)FindName(comboxName + "binomialfilter");
            Controlbinomialfilter.Visibility = System.Windows.Visibility.Hidden;
            GroupBox ControlsigmalImage = (GroupBox)FindName(comboxName + "sigmaImage");
            ControlsigmalImage.Visibility = System.Windows.Visibility.Hidden;
            GroupBox Controlgaussfilter = (GroupBox)FindName(comboxName + "gaussfilter");
            Controlgaussfilter.Visibility = System.Windows.Visibility.Hidden;

            ComboBoxItem selectItem = (ComboBoxItem)(((ComboBox)sender).SelectedValue);
            string FilterName = selectItem.Content.ToString();
            if ( FilterName == "")
                return;


            GroupBox control = (GroupBox)FindName(comboxName+FilterName);
            string judgeGroupbox = comboxName.Substring(0, 3);
            string currentStep = "";
            if (judgeGroupbox == "Fir")
            {
                currentStep = "first";
            }
            else if  (judgeGroupbox == "Sec")
            {
                currentStep = "second";
            }
            else if (judgeGroupbox == "Thi")
            {
                currentStep = "third";
            }
            else if (judgeGroupbox == "Fou")
            {
                currentStep = "fourth";
            }
            else if (judgeGroupbox == "Fiv")
            {
                currentStep = "fiveth";
            }
            else if (judgeGroupbox == "Six")
            {
                currentStep = "sixth";
            }
            if (control == null)
                return;
            
             control.Visibility = Visibility;

            //if (MoudlePicProcessingFlag == true)
            //    MoudlePicProcessingFlag = false;
            CurrentStepGroupBox = currentStep;
            // Thread PicProcessingChange = new Thread(() => MoudleMaking(currentStep));
 
            //PicProcessingChange.Start();
            //MoudlePicProcessingFlag = true;


        }

        protected int HDrawingObjectCallback(long draw_id, long window_handle, IntPtr type)
        {
            // On callback, process and display image
            HObject ho_EdgeAmplitude;
            lock (image_lock)
            {
                HOperatorSet.ClearWindow(ProcessingPic.HalconWindow);
                ProcessingPic.HalconWindow.AttachBackgroundToWindow(new HImage(Global_ProcessingPic));
                hv_ModelId = HD.process_image(Global_ProcessingPic, out ho_EdgeAmplitude, ProcessingPic.HalconID, draw_id);
                MoudlePicProcessingFlag = false;
            }
            // You need to switch to the UI thread to display the results
            //    Dispatcher.BeginInvoke(display_results_delegate);
            return 0;
        }


    }

    public class FilterfunctionState
    {
        public string picturetype { set; get; }
        public bool meanImageState { set; get; }
        public int meanImagemaskValue { set; get; }
        public bool medianImageState { set; get; }
        public string medianImagemasktype { set; get; }
        public int medianImageradiusvalue { set; get; }

        public bool smoothImageState { set; get; }
        public string smoothImagefiltertype { set; get; }
        public double smoothImagealpha { set; get; }

        public bool binomialfilterState { set; get; }
        public int binomialfiltermaskvalue { set; get; }

        public bool sigmaImageState { set; get; }
        public int sigmaImagemaskValue { set; get; }
        public int sigmaImagesigmaValue { set; get; }

        public bool gaussFilterState { set; get; }
        public int gaussFiltersize { set; get; }

    }


    public class FilterInforNeeded
    {
               
    }
 
}
