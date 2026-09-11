import { createRouter, createWebHistory } from 'vue-router'
import TransferView from '../views/TransferView.vue'
import AnalystView from '../views/AnalystView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'transfer',
      component: TransferView
    },
    
    {
      path: '/analyst', 
      name: 'analyst',
      component: AnalystView
    }
  ]
})

export default router