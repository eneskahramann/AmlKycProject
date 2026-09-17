// src/services/api.js
import axios from 'axios'

// Tüm istekler için temel ayarları (Base URL) bir kere tanımlıyoruz.
const apiClient = axios.create({
  baseURL: 'http://localhost:5045/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

export default apiClient