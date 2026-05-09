<template>
  <div class="login-container">
    <el-card class="login-card">
      <h2>🏢 HRM System</h2>
      <el-form :model="form" @submit.prevent="handleLogin">
        <el-form-item label="Tên đăng nhập">
          <el-input v-model="form.username" placeholder="Nhập tên đăng nhập" />
        </el-form-item>
        <el-form-item label="Mật khẩu">
          <el-input v-model="form.password" type="password" placeholder="Nhập mật khẩu" />
        </el-form-item>
        <el-button type="primary" native-type="submit" :loading="loading" style="width:100%">
          Đăng nhập
        </el-button>
        <p v-if="error" style="color:red; margin-top:10px">{{ error }}</p>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'

const store = useStore()
const router = useRouter()

const form = ref({ username: '', password: '' })
const loading = ref(false)
const error = ref('')

async function handleLogin() {
  loading.value = true
  error.value = ''
  try {
    await store.dispatch('login', form.value)
    router.push('/')
  } catch {
    error.value = 'Tên đăng nhập hoặc mật khẩu không đúng'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-container {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f0f2f5;
}
.login-card {
  width: 400px;
  padding: 20px;
}
h2 {
  text-align: center;
  margin-bottom: 24px;
}
</style>
