<template>
  <div class="dashboard-container">
    <h2>🚨 Analist Kontrol Paneli </h2>
    <p class="subtitle">Sistem tarafından tespit edilen şüpheli işlemler aşağıda listelenmektedir.</p>
    
    <div v-if="loading" class="loading">Alarmlar yükleniyor...</div>
    <div v-else-if="error" class="error-msg">{{ error }}</div>
    
    <div v-else class="table-responsive">
      <table class="alert-table">
        <thead>
          <tr>
            <th>Alarm ID</th>
            <th>Tarih</th>
            <th>İşlem Detayları</th>
            <th>Risk Skoru</th>
            <th>Durum</th>
            <th>Tetiklenen Kurallar</th>
            <th>Aksiyon</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="alert in alerts" :key="alert.id" :class="{'high-risk-row': alert.riskScore >= 70}">
            <td>#{{ alert.id }}</td>
            <td>{{ new Date(alert.createdAt).toLocaleString('tr-TR') }}</td>


            <!--UI - Gönderen - Alıcı - Hesap No kısmı-->
<td class="transaction-detail">
  <div v-if="alert.transfer" class="vertical-transaction">
    
    <!-- Üst Kısım: Gönderen -->
    <div class="account-box">
      <span class="person-name">{{ alert.transfer.senderName?.trim() || 'İsimsiz Müşteri' }}</span>
      <span class="acc-id"> (Hesap: {{ alert.transfer.senderAccountId }})</span>
    </div>
    
    <!-- Orta Kısım: Aşağı Ok ve Mavi Tutar -->
    <div class="middle-section">
      <div class="arrow-down">⬇️</div>
      <div class="amount-text">Tutar: {{ alert.transfer.amount?.toLocaleString('tr-TR') }} TL</div>
    </div>
    
    <!-- Alt Kısım: Alıcı -->
    <div class="account-box">
      <span class="person-name">{{ alert.transfer.receiverName?.trim() || 'İsimsiz Müşteri' }}</span>
      <span class="acc-id"> (Hesap: {{ alert.transfer.receiverAccountId }})</span>
    </div>

  </div>
  <div v-else class="text-muted">Transfer verisi bulunamadı.</div>               
</td>
          
            <td class="score">{{ alert.riskLog?.riskScore || alert.riskLog?.score }} / 100</td>
            <td>
              <span class="badge" :class="alert.status ? alert.status.toLowerCase() : ''">
                {{ alert.status || 'Bilinmiyor' }}
              </span>
            </td>
            <td class="rules">
              <ul>
                <li v-for="(rule, index) in parseRules(alert.riskLog?.triggeredRules)" :key="index">
                  {{ rule }}
                </li>
              </ul>
            </td>
            <td>
              <div v-if="alert.status === 'Open' || alert.status === 'Açık'" class="action-buttons">
                <button class="btn-approve" @click="updateStatus(alert.id, 'Approved')">✅ Temiz</button>
                <button class="btn-reject" @click="updateStatus(alert.id, 'Suspicious')">🚨 Şüpheli</button>
              </div>
              <div v-else class="text-muted">
                İncelendi ({{ alert.status }})
              </div>
            </td>
          </tr>
        </tbody>
      </table>
      
      <div v-if="alerts.length === 0" class="no-data">
        Harika! Şu an bekleyen hiçbir şüpheli işlem yok.
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import AlertService from '../services/AlertService'

const alerts = ref([])
const loading = ref(true)
const error = ref('')
// API'den alarmları çekmek için kullanılan fonksiyon
const fetchAlerts = async () => {
  try {
    
    const response = await AlertService.getAlerts()
    alerts.value = response.data
  } catch (err) {
    error.value = "Alarmlar çekilirken API'ye ulaşılamadı."
  } finally {
    loading.value = false
  }
}
// Alarm durumunu güncellemek için kullanılan fonksiyon
const updateStatus = async (id, newStatus) => {
  try {
    // Güncelleme isteği de AlertService üzerinden yapılıyor.
    await AlertService.updateStatus(id, newStatus)
    
    const alertIndex = alerts.value.findIndex(a => a.id === id)
    if (alertIndex !== -1) {
      alerts.value[alertIndex].status = newStatus
    }
  } catch (err) {
    alert("Durum güncellenirken bir hata oluştu. Backend açık mı?")
  }
}
// riskLog içindeki triggeredRules alanı JSON string olarak geliyor. Bunu parse edip listeye çevirmek için kullanılan fonksiyon
const parseRules = (rulesString) => {
  if (!rulesString) return ['Kural detayı bulunamadı']
  try {
    return JSON.parse(rulesString)
  } catch (e) {
    return [rulesString]
  }
}

onMounted(() => {
  fetchAlerts()
})
</script>

<style scoped>
/* GENEL KONTEYNER VE YAZI TİPLERİ */
.dashboard-container { 
  max-width: 1200px; 
  margin: 40px auto; 
  padding: 30px; 
  font-family: 'Segoe UI', system-ui, -apple-system, sans-serif; 
}
h2 { 
  color: #1e293b; 
  margin-bottom: 8px; 
  font-size: 1.8em;
  display: flex;
  align-items: center;
  gap: 10px;
}
.subtitle { 
  color: #64748b; 
  margin-bottom: 30px; 
  font-size: 1.05em; 
}

/* TABLO KAPSAYICISI */
.table-responsive { 
  overflow-x: auto; /* Ekran daraldığında sağa sola kaydırma barı çıkartır */
  background: white; 
  border-radius: 12px; 
  box-shadow: 0 10px 15px -3px rgba(0,0,0,0.05), 0 4px 6px -2px rgba(0,0,0,0.025); 
}

/* TABLO VE SATIR TASARIMI */
.alert-table { 
  width: 100%; 
  border-collapse: collapse; 
  text-align: left; 
  min-width: 800px; 
}
.alert-table th { 
  background-color: #f8fafc; 
  color: #475569; 
  padding: 16px; 
  font-weight: 600;
  font-size: 0.9em;
  text-transform: uppercase; 
  letter-spacing: 0.5px;
  border-bottom: 2px solid #cbd5e1; /* Başlık alt çizgisini koyulaştırdık */
}
.alert-table td { 
  padding: 20px 16px; 
  border-bottom: 1px solid #e2e8f0; /* Satır alt çizgilerini daha belirgin yaptık */
  vertical-align: middle; 
  color: #334155;
}
.alert-table tbody tr {
  transition: all 0.2s ease;
}

/* ZEBRA DESENİ: Çift numaralı satırlar çok açık mavi/gri olur */
.alert-table tbody tr:nth-child(even) {
  background-color: #eef2f5;
}

/* ZEBRA DESENİ: Tek numaralı satırlar beyaz kalır */
.alert-table tbody tr:nth-child(odd) {
  background-color: #e7e1e1;
}

/* HOVER EFEKTİ: Üzerine gelinen satır hafif koyulaşır */
.alert-table tbody tr:hover {
  background-color: #f1f5f9 !important; 
}

.high-risk-row { 
  background-color: #fef2f2 !important; 
  border-left: 4px solid #ef4444; 
}

/* YAZI VE BADGE (ETİKET) DETAYLARI */
.score { 
  font-weight: 800; 
  font-size: 1.1em; 
  color: #e41313 !important; 
}
.badge { 
  padding: 6px 12px; 
  border-radius: 20px; 
  font-size: 0.85em; 
  font-weight: bold; 
  text-transform: capitalize;
}
.badge.açık, .badge.open { 
  background-color: #fef08a;
   color: #854d0e; 
}
.badge.approved { 
background-color: #dcfce7; 
color: #166534; 
}
.badge.suspicious { 
  background-color: #fee2e2; 
  color: #991b1b; 
}

.rules ul { 
  margin: 0; 
  padding-left: 20px; 
  color: #4b668b; 
  font-size: 0.9em; 
  line-height: 1.5; 
}

/* BUTONLAR */
.action-buttons { 
  display: flex; 
  gap: 10px; 
  flex-wrap: wrap; 
}
.btn-approve, .btn-reject {
  padding: 10px 16px; 
  color: white; 
  border: none; 
  border-radius: 8px; 
  cursor: pointer; 
  font-weight: 600; 
  font-size: 0.9em;
  transition: all 0.2s ease;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  flex: 1; /* Butonların eşit genişlikte olmasını sağlar */
  text-align: center;
}

.btn-approve { 
  background-color: #10b981; 
}
.btn-approve:hover { 
  background-color: #059669; 
  transform: translateY(-1px); 
  box-shadow: 0 4px 6px rgba(0,0,0,0.15); 
}

.btn-reject { 
  background-color: #ef4444; 
}
.btn-reject:hover { 
  background-color: #dc2626; 
  transform: translateY(-1px); 
  box-shadow: 0 4px 6px rgba(0,0,0,0.15); 
}

/* YARDIMCI SINIFLAR */
.text-muted { 
  color: #94a3b8; 
  font-style: italic; 
  font-size: 0.9em; 
}
.loading, .no-data { 
  text-align: center; 
  padding: 40px; 
  font-size: 1.1em; 
  color: #64748b; 
}
.error-msg { 
  background-color: #fee2e2; 
  color: #991b1b; 
  padding: 15px; 
  border-radius: 8px; 
  font-weight: 500; 
  border-left: 4px solid #ef4444; 
}

/* === İŞLEM DETAYLARI (DİKEY TASARIM) === */
.vertical-transaction {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}
.account-box {
  background-color: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 8px 16px;
  text-align: center;
  width: 100%;
  max-width: 240px;
  box-sizing: border-box;
  box-shadow: 0 1px 2px rgba(0,0,0,0.02);
}
.person-name {
  display: block;
  font-weight: 700;
  color: #1e293b;
  font-size: 1em;
}
.acc-id {
  display: block;
  font-size: 0.85em;
  color: #64748b;
  margin-top: 2px;
}
.middle-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}
.arrow-down {
  font-size: 1.2em;
  color: #94a3b8;
}
.amount-text {
  color: #0ea5e9; 
  font-weight: 700;
  font-size: 1.05em;
  background-color: #f0f9ff;
  padding: 6px 16px;
  border-radius: 20px;
  border: 1px solid #bae6fd;
}

/* === RESPONSİVE (MOBİL UYUMLULUK) AYARLARI === */
@media (max-width: 768px) {
  .dashboard-container {
    margin: 15px auto;
    padding: 15px;
  }
  
  h2 {
    font-size: 1.4em;
  }
  
  .alert-table th, .alert-table td {
    padding: 12px 10px; /* Dar ekranlarda tablo içindeki boşlukları küçültüyoruz */
  }

  .action-buttons {
    flex-direction: column; /* Dar ekranda butonları yan yana sıkıştırmak yerine alt alta dizer */
  }
}
</style>