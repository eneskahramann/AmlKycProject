<template>
  <header class="navbar">
    <h1>AML KYC Project</h1>
    <nav>
      <RouterLink to="/">Transfer Yap</RouterLink>
      <RouterLink to="/analyst">Analist Paneli</RouterLink>
    </nav>
  </header>

  <!-- Router'da tanımladığımız sayfalar burada gösterilecek -->
  <RouterView />
</template>

<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<style>

body 
{ 
  margin: 0; 
  padding: 0; 
  background-color: #d9dee2; 
}
.navbar 
{ 
  background-color: #343a40; 
  padding: 15px 30px; 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  color: white; 
  font-family: Arial, sans-serif; 
}
.navbar h1 
{ 
  margin: 0; 
  font-size: 24px; 
  color: #ff6600; 
}
.navbar nav a 
{ 
  color: white; 
  text-decoration: none;
  margin-left: 20px; 
  font-weight: bold; 
}
.navbar nav a:hover 
{ 
  color: #ff7300; 
}
</style>