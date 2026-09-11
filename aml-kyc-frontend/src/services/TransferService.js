import api from './api'

export default {
  // Transfer işlemini backend'e ileten metot
  makeTransfer(transferData) {
    // api.js içindeki baseURL (http://localhost:5045/api) otomatik olarak başa eklenir
    return api.post('/Transfer', transferData)
  }
}