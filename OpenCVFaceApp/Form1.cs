using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Face;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenCVFaceApp
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Mat _frame;
        private Timer _timer;
        private CascadeClassifier _faceCascade;
        private LBPHFaceRecognizer _recognizer; 
        private List<int> _labels = new List<int>(); 
        private int _nextLabel = 0; 
        public Form1()
        {
            InitializeComponent();
        }
        private void TrainRecognizer()
        {
            List<Mat> imagesToTrain = new List<Mat>();
            List<int> labelsToTrain = new List<int>();
            _nextLabel = 0; 

            string facesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Faces");
            if (!Directory.Exists(facesDir))
            {
                lblProcess.Text = "Eğitim için 'Faces' klasöründe veri bulunamadı.";
                return;
            }

           
            _recognizer = LBPHFaceRecognizer.Create();

            var subDirs = Directory.GetDirectories(facesDir);

            foreach (var dir in subDirs)
            {
                string personName = new DirectoryInfo(dir).Name;

                
                int label = _nextLabel++;

                string[] images = Directory.GetFiles(dir, "*.jpg");

                foreach (string imagePath in images)
                {
                    using (Mat img = Cv2.ImRead(imagePath, ImreadModes.Grayscale))
                    {
                        if (img.Empty()) continue;

                       
                        Rect[] faces = _faceCascade.DetectMultiScale(img, 1.1, 4, 0, new OpenCvSharp.Size(30, 30));

                        if (faces.Length > 0)
                        {
                            
                            using (Mat faceROI = new Mat(img, faces[0]))
                            {
                                
                                Mat clonedFace = faceROI.Clone();

                                imagesToTrain.Add(clonedFace); 
                                labelsToTrain.Add(label);

                               
                            }
                        }
                    }
                }
                lblProcess.Text = $"{personName} etiketi ({label}) için yüzler yüklendi.";
            }

            if (imagesToTrain.Count > 0)
            {
              
                _recognizer.Train(imagesToTrain.ToArray(), labelsToTrain.ToArray());

                lblProcess.Text = $"EĞİTİM BAŞARILI! Toplam {imagesToTrain.Count} yüz örneği ile eğitildi.";
            }
            else
            {
                lblProcess.Text = "HATA: Hiçbir eğitilebilir yüz örneği bulunamadı.";
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string cascadePath = "haarcascade_frontalface_default.xml";
            _faceCascade = new CascadeClassifier(cascadePath);

            if (_faceCascade.Empty())
            {
              
                return;
            }
            else
            {
                lblProcess.Text = "Model yüklendi. Kamerayı başlatılıyor...";
            }


           
            _capture = new VideoCapture(0);

            if (!_capture.IsOpened())
            {
                lblProcess.Text = "HATA: Kamera bulunamadı veya açılamadı.";
                return;
            }

          
            _timer = new Timer();
            _timer.Interval = 30; 
            _timer.Tick += Timer_Tick;
            _timer.Start();
            if (_faceCascade != null && !_faceCascade.Empty())
            {
                TrainRecognizer();
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_capture == null || !_capture.IsOpened()) return;

            if (_frame != null)
            {
                _frame.Dispose();
            }

            _frame = new Mat();
            if (_capture.Read(_frame))
            {
               
                if (pictureBoxCamera.Image != null)
                {
                    pictureBoxCamera.Image.Dispose();
                    pictureBoxCamera.Image = null;
                }

              
                using (Mat grayFrame = new Mat())
                {
                    Cv2.CvtColor(_frame, grayFrame, ColorConversionCodes.BGR2GRAY);

                    Cv2.EqualizeHist(grayFrame, grayFrame);

                    Rect[] faces = _faceCascade.DetectMultiScale(
    grayFrame,
    1.1,
    4,
    0,
    new OpenCvSharp.Size(30, 30));

                    
                    foreach (var face in faces) 
                    {
                        
                        string personName = "Bilinmiyor";
                        double confidence = 100.0; 
                        int predictedLabel = -1;

                       
                        if (_recognizer != null && !_recognizer.Empty)
                        {
                            
                            using (Mat faceROI = new Mat(grayFrame, face))
                            {
                                
                                _recognizer.Predict(faceROI, out predictedLabel, out confidence);

                                double threshold = 80.0; 

                                if (confidence < threshold)
                                {
                                   
                                    if (predictedLabel == 0) personName = "Huseyin (ID:0)";
                                    else if (predictedLabel == 1) personName = "unknown (ID:1)";
                                    
                                }
                            } 
                        }

                        
                        Cv2.PutText(_frame, $"{personName} ({confidence:0.0})",
                                    new OpenCvSharp.Point(face.X, face.Y - 10), 
                                    HersheyFonts.HersheySimplex, 0.7, new Scalar(0, 255, 0), 2);

                        
                        Scalar color = (confidence < 80.0) ? new Scalar(0, 255, 0) : new Scalar(0, 0, 255);
                        Cv2.Rectangle(_frame, face, color, 2);
                    }

                   
                    using (Bitmap tempBmp = _frame.ToBitmap())
                    {
                        Bitmap finalBmp = (Bitmap)tempBmp.Clone();
                        pictureBoxCamera.Image = finalBmp;
                    }
                }

                
                _frame.Dispose();
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
           
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            if (_capture != null && _capture.IsOpened())
            {
                _capture.Release();
                _capture.Dispose();
            }
            if (_faceCascade != null)
            {
                _faceCascade.Dispose();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string personName = txtName.Text.Trim();
    if (string.IsNullOrEmpty(personName))
    {
        lblProcess.Text = "HATA: Lütfen bir isim girin.";
        return;
    }

    
    if (_faceCascade == null || _faceCascade.Empty())
    {
        lblProcess.Text = "HATA: Yüz tanıma modeli yüklenmemiş.";
        return;
    }

    
    if (_capture == null || !_capture.IsOpened())
    {
        lblProcess.Text = "HATA: Kamera açık değil.";
        return;
    }

    
    using (Mat frameToSave = new Mat())
    {
                if (!_capture.Read(frameToSave))
                {
            lblProcess.Text = "Kameradan kare alınamadı.";
            return;
        }

       
        using (Mat grayFrame = new Mat())
        {
            Cv2.CvtColor(frameToSave, grayFrame, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayFrame, grayFrame);

            Rect[] faces = _faceCascade.DetectMultiScale(
                grayFrame, 
                1.1, 4, 0, new OpenCvSharp.Size(30, 30));

            if (faces.Length == 0)
            {
                lblProcess.Text = "Yüz algılanmadı. Lütfen kameraya bakın ve tekrar deneyin.";
                return;
            }
            
            
            Rect faceRect = faces[0]; 

            
            
            using (Mat faceImg = new Mat(frameToSave, faceRect))
            {
              
                string baseDir = AppDomain.CurrentDomain.BaseDirectory; 
                string personFolder = Path.Combine(baseDir, "Faces", personName);

                
                if (!Directory.Exists(personFolder))
                {
                    Directory.CreateDirectory(personFolder);
                }

                int count = Directory.GetFiles(personFolder, "*.jpg").Length + 1;
                string filePath = Path.Combine(personFolder, $"{personName}_{count}.jpg");

                Cv2.ImWrite(filePath, faceImg); 

                lblProcess.Text = $"{personName} için {count}. yüz kaydedildi!";
            }
        } 
    } 
        }
    }
}
