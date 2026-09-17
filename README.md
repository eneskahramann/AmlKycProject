# 🛡️ SentinelAML - Anti-Money Laundering & Risk Engine

Şüpheli finansal işlemleri tespit etmek, engellemek ve raporlamak amacıyla geliştirilmiş **.NET Core** ve **Vue.js** tabanlı bir Kara Para Aklama Önleme (AML - Anti-Money Laundering) ve İşlem İzleme (Transaction Monitoring) sistemidir.

Proje, kurumsal bankacılık standartlarında geliştirilmiş olup, ACID prensiplerine uygun para transferi yönetimi ve kural tabanlı dinamik bir risk motoru içermektedir.

## 🚀 Kullanılan Teknolojiler

*   **Backend:** .NET Core API, C#
*   **Frontend:** Vue.js
*   **Veritabanı:** PostgreSQL
*   **ORM:** Entity Framework Core
*   **Mimari:** N-Tier Architecture (Controller > Service > Data/Entity), Dependency Injection

## 💡 Temel Özellikler

*   **Güvenlik Duvarı (Hard Block):** Yaptırım (Sanction) listesindeki müşterilerin işlemleri, bakiye kontrolüne dahi girmeden veritabanı seviyesinde "Başarısız" (Failed) olarak işaretlenir ve anında bloke edilir.
*   **Dinamik Risk Motoru (`RiskService`):** Transferler, çeşitli finansal suç senaryolarına karşı analiz edilir ve 0-100 arası bir risk skoru üretilir.
*   **ACID Transaction Yönetimi (`TransferService`):** İşlem sırasında oluşabilecek herhangi bir hataya karşı veritabanı bütünlüğünü korumak için `BeginTransactionAsync` ve `Rollback` mekanizmaları kullanılır.
*   **Analist Dashboard'u:** Risk skoru belirli bir eşiği geçen (veya bloke edilen) işlemler, JSON formatında tetiklenen kural detaylarıyla birlikte `Alert` (Alarm) olarak analist ekranına düşer.

## 🔍 Kural Motoru (Risk Engine) Senaryoları

Sistem şu anda aşağıdaki şüpheli işlem örüntülerini (pattern) tespit edebilmektedir:
1.  **Sanction Match (Yaptırım Eşleşmesi):** Anında bloke ve Kırmızı Alarm.
2.  **Yüksek Tutar:** Yapılan işlemin hacminin çok yüksek olması.
3.  **Sınır Altı İşlem:** Yasal bildirim sınırının hemen altındaki (Örn: 95.000 - 99.999 TL) şüpheli tutarlar.
4.  **Gece İşlemi(22:00 - 06:00):** Gece belli saatlerde yapılan işlemler.
5.  **Dormant Account (Uyuyan Hesap):** Uzun süre inaktif olan hesaplardan aniden çıkan yüklü miktarlar.
6.  **New-to-New (Çifte Taze Hesap):** Yeni açılmış hesaplar arasındaki yüksek hacimli trafik.
7.  **Doğal Olmayan Küsuratsız İşlem:** Yasadışı bahis veya haraç ödemeleri çoğunlukla pürüzsüz, tam binlik katlar şeklinde yapılır.

## ⚙️ Kurulum ve Çalıştırma

### 1. Veritabanı (PostgreSQL) Ayarları
`appsettings.json` dosyasındaki `ConnectionStrings` alanını kendi PostgreSQL bilgilerinize göre güncelleyin.
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=AmlKycDb;Username=postgres;Password=sifreniz"
}

## 2. Backend'i Ayağa Kaldırma
Bash
dotnet ef database update
dotnet run

## 3. Frontend'i Ayağa Kaldırma
Bash
npm install
npm run dev
