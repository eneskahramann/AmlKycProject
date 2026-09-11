//HTML Arayüzü için Vue.js kullanarak bir para transferi formu oluşturuldu. Kullanıcıdan gönderici ve alıcı hesap ID'leri ile transfer edilecek tutar alınmakta. Form gönderildiğinde `makeTransfer` fonksiyonu çalıştırılıyor ve bu fonksiyon backend API'sine bir POST isteği gönderiyor. Başarılı veya başarısız işlemler için kullanıcıya mesaj gösteriliyor.
<template>
  <div class="transfer-container">
    <h2>Para Transferi</h2>
    <form @submit.prevent="makeTransfer">
      <div class="form-group">
        <label>Gönderici Hesap ID:</label>
        <input type="number" v-model="senderId" required placeholder="Örn: 1" />
      </div>
      
      <div class="form-group">
        <label>Alıcı Hesap ID:</label>
        <input type="number" v-model="receiverId" required placeholder="Örn: 2" />
      </div>
      
      <div class="form-group">
        <label>Tutar (TL):</label>
        <input type="number" v-model="amount" required placeholder="Örn: 150000" />
      </div>
      
      <button type="submit">Transferi Gerçekleştir</button>
    </form>

    <div v-if="message" :class="{'error-msg': isError, 'success-msg': !isError}">
      {{ message }}
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'

// Form verilerini tutacağımız reaktif değişkenler
const senderId = ref('')
const receiverId = ref('')
const amount = ref('')
const message = ref('')
const isError = ref(false)

// Transfer butonuna basıldığında çalışacak fonksiyon
const makeTransfer = async () => {
  try {
    message.value = 'İşlem yapılıyor...'
    isError.value = false

    // Backend API'mize POST isteği atıyoruz
    const response = await axios.post('http://localhost:5045/api/Transfer', {
      senderAccountId: parseInt(senderId.value),
      receiverAccountId: parseInt(receiverId.value),
      amount: parseFloat(amount.value)
    })

    message.value = "İşlem Başarılı! Arka planda risk motoru çalıştı."
    
    // İşlem başarılıysa formu temizle
    senderId.value = ''
    receiverId.value = ''
    amount.value = ''

  } 
  // Eğer backend 400 Bad Request (örn: Yetersiz bakiye) dönerse buraya düşeriz
  catch (error) {
  
    isError.value = true
    // Backend'den gelen veri
    const errData = error.response?.data
    // Backend'den gelen yetersiz bakiye gibi mesajları ekrana basıyoruz
    message.value = errData?.message || errData || "Bir hata oluştu."
  }
}
</script>

<style scoped>
.transfer-container {
  max-width: 450px;
  margin: 40px auto;
  padding: 30px;
  background-color: #f8f9fa;
  border-radius: 10px;
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
  font-family: Arial, sans-serif;
}
h2 { text-align: center; color: #333; margin-bottom: 20px; }
.form-group { margin-bottom: 15px; }
label { display: block; font-weight: bold; margin-bottom: 5px; color: #555; }
input { width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 5px; box-sizing: border-box; }
button { width: 100%; padding: 12px; background-color: #007bff; color: white; font-weight: bold; border: none; border-radius: 5px; cursor: pointer; transition: 0.3s; }
button:hover { background-color: #0056b3; }
.success-msg { margin-top: 15px; padding: 10px; background-color: #d4edda; color: #155724; border-radius: 5px; text-align: center; }
.error-msg { margin-top: 15px; padding: 10px; background-color: #f8d7da; color: #721c24; border-radius: 5px; text-align: center; }
</style>