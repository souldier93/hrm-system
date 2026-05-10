<template>
  <el-container style="height: 100vh">
    <!-- Sidebar -->
    <el-aside width="220px" style="background:#001529">
      <div style="color:white; text-align:center; padding:20px; font-size:18px; font-weight:bold">
        🏢 HRM System
      </div>
      <el-menu
        :default-active="$route.path"
        router
        background-color="#001529"
        text-color="#ccc"
        active-text-color="#fff"
      >
        <el-menu-item index="/">
          <el-icon><HomeFilled /></el-icon>
          <span>Trang chủ</span>
        </el-menu-item>
        <el-menu-item index="/employees">
          <el-icon><User /></el-icon>
          <span>Nhân viên</span>
        </el-menu-item>
        <el-menu-item index="/attendance">
          <el-icon><Calendar /></el-icon>
          <span>Chấm công</span>
        </el-menu-item>
        <el-menu-item index="/salary">
          <el-icon><Money /></el-icon>
          <span>Lương</span>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <el-container>
      <!-- Header -->
      <el-header style="background:white; display:flex; align-items:center; justify-content:space-between; box-shadow:0 1px 4px rgba(0,0,0,0.1)">
        <NotificationBell />
        <span style="font-size:16px">{{ $route.name }}</span>
        
        <el-dropdown @command="handleCommand">
          <span style="cursor:pointer">
            👤 {{ user?.fullName }} <el-icon><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="logout">Đăng xuất</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </el-header>

      <!-- Nội dung trang -->
      <el-main style="background:#f0f2f5">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { computed } from 'vue'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import NotificationBell from './NotificationBell.vue'
const store = useStore()
const router = useRouter()
const user = computed(() => store.getters.currentUser)

function handleCommand(command) {
  if (command === 'logout') {
    store.dispatch('logout')
    router.push('/login')
  }
}
</script>