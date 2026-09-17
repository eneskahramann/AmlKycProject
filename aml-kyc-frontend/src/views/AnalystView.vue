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
            <td class ="transaction-detail">
              <div v-if = "alert.transfer">
                <span class="account-badge">Hesap: {{ alert.transfer.senderAccountId }}</span>
                <span class="arrow"> ➡️ </span> 
                <span class="account-badge">Hesap: {{ alert.transfer.receiverAccountId }}</span>
                <span class="amount"> Tutar: {{ alert.transfer.amount?.toLocaleString('tr-TR') }} TL</span>
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
.dashboard-container { max-width: 1000px; margin: 40px auto; padding: 20px; font-family: Arial, sans-serif; }
h2 { color: #d9534f; margin-bottom: 5px; }
.subtitle { color: #6c757d; margin-bottom: 30px; }
.table-responsive { overflow-x: auto; background: white; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
.alert-table { width: 100%; border-collapse: collapse; text-align: left; }
.alert-table th { background-color: #343a40; color: white; padding: 15px; }
.alert-table td { padding: 15px; border-bottom: 1px solid #dee2e6; vertical-align: middle; }
.high-risk-row { background-color: #fff5f5; }
.score { font-weight: bold; font-size: 1.1em; color: #dc3545; }
.badge { padding: 6px 12px; border-radius: 20px; font-size: 0.85em; font-weight: bold; }
.badge.açık, .badge.open { background-color: #ffc107; color: #000; }
.badge.approved { background-color: #28a745; color: white; }
.badge.suspicious { background-color: #dc3545; color: white; }
.rules ul { margin: 0; padding-left: 20px; color: #495057; font-size: 0.9em; }
.action-buttons { display: flex; gap: 8px; }
.btn-approve { padding: 8px 12px; background-color: #28a745; color: white; border: none; border-radius: 4px; cursor: pointer; font-weight: bold; transition: 0.2s;}
.btn-approve:hover { background-color: #218838; }
.btn-reject { padding: 8px 12px; background-color: #dc3545; color: white; border: none; border-radius: 4px; cursor: pointer; font-weight: bold; transition: 0.2s;}
.btn-reject:hover { background-color: #c82333; }
.text-muted { color: #6c757d; font-style: italic; font-weight: bold; }
.loading, .no-data { text-align: center; padding: 30px; font-size: 1.2em; color: #6c757d; }
.error-msg { background-color: #f8d7da; color: #721c24; padding: 15px; border-radius: 5px; }
</style>