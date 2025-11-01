C# WinForms ve OpenCvSharp ile Yüz Tanıma Projesi
Bu proje, C# WinForms arayüzü kullanılarak geliştirilmiş, OpenCvSharp kütüphanesi aracılığıyla web kamerasından canlı görüntü yakalayan ve LBPH (Local Binary Patterns Histograms) algoritması ile yüzleri kaydedip tanıyan temel bir yüz tanıma sistemidir.

* Özellikler
Canlı Görüntüleme: Web kamerasından gerçek zamanlı video akışı.

Yüz Algılama: OpenCV'nin Haar Cascade sınıflandırıcısı ile anlık yüz tespiti.

Yüz Kaydetme: Algılanan yüzün belirli bir isme göre (ROI olarak) kaydedilmesi.

Yüz Tanıma: LBPH algoritması kullanılarak kaydedilen yüzlerin canlı akıştaki yüzlerle eşleştirilmesi.

Basit Etiketleme: Kaydedilen her kişi için basit bir ID ataması (0, 1, 2...).

* Teknolojiler
Programlama Dili: C#

Arayüz: Windows Forms (.NET Framework 4.8)

Kütüphane: OpenCvSharp4 (OpenCV'nin C# bağlayıcısı)

Tanıma Algoritması: LBPH Face Recognizer
