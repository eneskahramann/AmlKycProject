// src/services/AlertService.js
import api from './api'

export default {
  // Alarmları getiren metot
  getAlerts() {
    return api.get('/Transfer/alerts')
  },
  
  // Alarm durumunu güncelleyen metot
  updateStatus(id, newStatus) {
    return api.put(`/Transfer/alerts/${id}/status`, { status: newStatus })
  }
}