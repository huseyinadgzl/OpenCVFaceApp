using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.IO;

namespace OpenCVFaceApp
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Mat _frame;
        private Timer _timer;
        private CascadeClassifier _faceCascade;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string cascadePath = "haarcascade_frontalface_default.xml";
            _faceCascade = new CascadeClassifier(cascadePath);

            if (_faceCascade.Empty())
            {
               // lblProcess.Text = $"HATA: Yüz tanıma dosyası ({cascadePath}) bulunamadı! Lütfen XML dosyasını 'bin\Debug' klasörüne kopyalayın.";
                // Dosya yoksa kamerayı başlatmaya gerek yok.
                return;
            }
            else
            {
                lblProcess.Text = "Model yüklendi. Kamerayı başlatılıyor...";
            }


            // 2. KAMERAYI BAŞLAT
            _capture = new VideoCapture(0);

            if (!_capture.IsOpened())
            {
                lblProcess.Text = "HATA: Kamera bulunamadı veya açılamadı.";
                return;
            }

            // 3. Görüntüleme için Timer'ı ayarla
            _timer = new Timer();
            _timer.Interval = 30; // Yaklaşık 33 FPS
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_capture == null || !_capture.IsOpened()) return;

            if (_frame != null)
            {
                _frame.Dispose(); // Önceki Mat nesnesini temizle
            }

            _frame = new Mat();
            if (_capture.Read(_frame))
            {
                // 1. Önceki resmi temizle (Bellek sızıntısını önlemek için)
                if (pictureBoxCamera.Image != null)
                {
                    pictureBoxCamera.Image.Dispose();
                    pictureBoxCamera.Image = null;
                }

                // 2. Görüntüyü gri tonlamaya çevir (Yüz algılama için)
                using (Mat grayFrame = new Mat())
                {
                    Cv2.CvtColor(_frame, grayFrame, ColorConversionCodes.BGR2GRAY);

                    // 3. Kontrastı ayarla
                    Cv2.EqualizeHist(grayFrame, grayFrame);

                    // 4. Yüzleri Algıla (CS0104 hatasını düzelttik!)
                    Rect[] faces = _faceCascade.DetectMultiScale(
                        grayFrame,
                        1.1,
                        4,
                        0,
                        new OpenCvSharp.Size(30, 30)); // <-- OpenCvSharp.Size kullandık!

                    // 5. Tespit edilen yüzlerin etrafına Kırmızı dikdörtgen çiz
                    foreach (var face in faces)
                    {
                        Cv2.Rectangle(_frame, face, new Scalar(0, 0, 255), 2);
                    }

                    // 6. Sonucu PictureBox'a Göster (Güvenli Yöntem: Klonlama)
                    using (Bitmap tempBmp = _frame.ToBitmap())
                    {
                        Bitmap finalBmp = (Bitmap)tempBmp.Clone();
                        pictureBoxCamera.Image = finalBmp;
                    }
                }

                // 7. Okunan Mat nesnesini serbest bırak
                _frame.Dispose();
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Kaynakları temizle
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

    // 2. Yüz algılamanın çalıştığını kontrol et
    if (_faceCascade == null || _faceCascade.Empty())
    {
        lblProcess.Text = "HATA: Yüz tanıma modeli yüklenmemiş.";
        return;
    }

    // 3. Kameradan son kareyi yakala (Tanıtım için sadece tek bir kare yeterli)
    if (_capture == null || !_capture.IsOpened())
    {
        lblProcess.Text = "HATA: Kamera açık değil.";
        return;
    }

    // Kameradan mevcut kareyi yakala
    using (Mat frameToSave = new Mat())
    {
                if (!_capture.Read(frameToSave))
                {
            lblProcess.Text = "Kameradan kare alınamadı.";
            return;
        }

        // 4. Yüz Tespiti (Kaydedeceğimiz yüzü bulmak için)
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
            
            // Sadece ilk bulunan yüzü kaydedeceğiz (birden fazla yüz varsa)
            Rect faceRect = faces[0]; 

            // 5. Yüz Bölgesini Kes (Region of Interest - ROI)
            // OpenCV'de Mat kesmek basit bir işlemle yapılır:
            using (Mat faceImg = new Mat(frameToSave, faceRect))
            {
                // 6. Kayıt Klasörünü Oluştur ve Dosyayı Kaydet
                
                // Ana klasör yolu (Projenin çalıştırıldığı yer)
                string baseDir = AppDomain.CurrentDomain.BaseDirectory; 
                string personFolder = Path.Combine(baseDir, "Faces", personName);

                // Eğer kişiye ait klasör yoksa oluştur
                if (!Directory.Exists(personFolder))
                {
                    Directory.CreateDirectory(personFolder);
                }

                // Dosya adı oluştur (Örn: John_1.jpg)
                int count = Directory.GetFiles(personFolder, "*.jpg").Length + 1;
                string filePath = Path.Combine(personFolder, $"{personName}_{count}.jpg");

                // Resmi kaydet (Ölçeklendirme önerilir, şimdilik orijinal boyutta kaydediyoruz)
                Cv2.ImWrite(filePath, faceImg); 

                lblProcess.Text = $"{personName} için {count}. yüz kaydedildi!";
            }
        } // grayFrame Dispose olur
    } // frameToSave Dispose olur
        }
    }
}
