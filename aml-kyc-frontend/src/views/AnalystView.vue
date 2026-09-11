<template>
  <div class="dashboard-container">
    <h2>🚨 Analist Kontrol Paneli</h2>
    <p class="subtitle">Sistem tarafından tespit edilen şüpheli işlemler aşağıda listelenmektedir.</p>
    
    <div v-if="loading" class="loading">Alarmlar yükleniyor...</div>
    <div v-else-if="error" class="error-msg">{{ error }}</div>
    
    <div v-else class="table-responsive">
      <table class="alert-table">
        <thead>
          <tr>
            <th>Alarm ID</th>
            <th>Tarih</th>
            <th>Risk Skoru</th>
            <th>Durum</th>
            <th>Tetiklenen Kurallar</th>
            <th>Aksiyon</th>
          </tr>
        </thead>
        <tbody>
          <!-- Hata Koruması: alert.riskScore kontrolü -->
          <tr v-for="alert in alerts" :key="alert.id" :class="{'high-risk-row': alert.riskScore >= 70}">
            <td>#{{ alert.id }}</td>
            <td>{{ new Date(alert.createdAt).toLocaleString('tr-TR') }}</td>
            <td class="score">{{ alert.riskScore }} / 100</td>
            <td>
              <!-- Hata Koruması: status null gelirse çökme, 'Bilinmiyor' yaz -->
              <span class="badge" :class="alert.status ? alert.status.toLowerCase() : ''">
                {{ alert.status || 'Bilinmiyor' }}
              </span>
            </td>
            <td class="rules">
              <ul>
                <!-- Hata Koruması: riskLog null ise sistemi çökertmek yerine güvenli metot kullan (?. operatörü) -->
                <li v-for="(rule, index) in parseRules(alert.riskLog?.triggeredRules)" :key="index">
                  {{ rule }}
                </li>
              </ul>
            </td>
            <td>
              <button class="btn-review" disabled>İncele</button>
            </td>
          </tr>
        </tbody>
      </table>
      
      <div v-if="alerts.length === 0" class="no-data">
        Şu an bekleyen hiçbir şüpheli işlem yok.
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const alerts = ref([])
const loading = ref(true)
const error = ref('')

const fetchAlerts = async () => {
  try {
    const response = await axios.get('http://localhost:5045/api/Transfer/alerts')
    alerts.value = response.data
  } catch (err) {
    error.value = "Alarmlar çekilirken API'ye ulaşılamadı."
  } finally {
    loading.value = false
  }
}

// Güçlendirilmiş kural parçalayıcı: Eğer kural yoksa veya null ise sistemi çökertmez
const parseRules = (rulesString) => {
  if (!rulesString) return ['Kural detayı backend\'den gelmedi (Include eksik)']
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
/* CSS kısımları aynı kalıyor */
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
.rules ul { margin: 0; padding-left: 20px; color: #495057; font-size: 0.9em; }
.btn-review { padding: 8px 15px; background-color: #17a2b8; color: white; border: none; border-radius: 4px; cursor: not-allowed; opacity: 0.7;}
.loading, .no-data { text-align: center; padding: 30px; font-size: 1.2em; color: #6c757d; }
.error-msg { background-color: #f8d7da; color: #721c24; padding: 15px; border-radius: 5px; }
</style>