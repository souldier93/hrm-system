<template>
  <div>
    <!-- Chuông thông báo trên header -->
    <el-badge :value="unreadCount || ''" :hidden="!unreadCount">
      <el-button circle @click="drawerVisible = true">
        <el-icon><Bell /></el-icon>
      </el-button>
    </el-badge>

    <!-- Drawer danh sách thông báo -->
    <el-drawer
      v-model="drawerVisible"
      title="🔔 Thông báo"
      direction="rtl"
      size="380px"
    >
      <div style="padding:0 16px">
        <!-- Nút đánh dấu tất cả đã đọc -->
        <div style="text-align:right; margin-bottom:12px">
          <el-button
            size="small"
            text
            @click="handleMarkAllRead"
            :disabled="!unreadCount"
          >
            Đánh dấu tất cả đã đọc
          </el-button>
        </div>

        <!-- Danh sách thông báo -->
        <div v-if="notifications.length === 0"
          style="text-align:center; color:#999; padding:40px 0">
          Không có thông báo nào
        </div>

        <div
          v-for="n in notifications"
          :key="n.id"
          class="notification-item"
          :class="{ unread: !n.isRead }"
          @click="handleRead(n)"
        >
          <el-icon class="notif-icon" :style="{ color: typeColor(n.type) }">
            <component :is="typeIcon(n.type)" />
          </el-icon>
          <div class="notif-content">
            <div class="notif-title">{{ n.title }}</div>
            <div class="notif-message">{{ n.message }}</div>
            <div class="notif-time">
              {{ formatTime(n.createdAt) }}
            </div>
          </div>
        </div>

        <!-- Load more -->
        <div style="text-align:center; margin-top:16px"
          v-if="hasMore">
          <el-button text @click="loadMore">Xem thêm</el-button>
        </div>
      </div>
    </el-drawer>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useStore } from 'vuex'
import * as signalR from '@microsoft/signalr'
import notificationApi from '../api/notification'
import { ElMessage } from 'element-plus'

const store = useStore()
const drawerVisible = ref(false)
const notifications = ref([])
const unreadCount = ref(0)
const page = ref(1)
const hasMore = ref(false)
let connection = null

function typeColor(type) {
  return { Success: '#67C23A', Warning: '#E6A23C', Info: '#409EFF' }[type] || '#409EFF'
}

function typeIcon(type) {
  return { Success: 'CircleCheck', Warning: 'Warning', Info: 'InfoFilled' }[type] || 'InfoFilled'
}

function formatTime(dateStr) {
  const date = new Date(dateStr)
  return date.toLocaleString('vi-VN')
}

async function loadNotifications(reset = false) {
  if (reset) page.value = 1
  const res = await notificationApi.getAll(page.value)
  const data = res.data

  if (reset) {
    notifications.value = data.items
  } else {
    notifications.value.push(...data.items)
  }

  unreadCount.value = data.unreadCount
  hasMore.value = notifications.value.length < data.total
}

async function loadMore() {
  page.value++
  await loadNotifications()
}

async function handleRead(notification) {
  if (!notification.isRead) {
    await notificationApi.markAsRead(notification.id)
    notification.isRead = true
    unreadCount.value = Math.max(0, unreadCount.value - 1)
  }
}

async function handleMarkAllRead() {
  await notificationApi.markAllAsRead()
  notifications.value.forEach(n => n.isRead = true)
  unreadCount.value = 0
  ElMessage.success('Đã đánh dấu tất cả đã đọc!')
}

function connectSignalR() {
  const token = store.state.token
  connection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5000/hubs/notification', {
      accessTokenFactory: () => token
    })
    .withAutomaticReconnect()
    .build()

  connection.on('NewNotification', (notification) => {
    notifications.value.unshift(notification)
    unreadCount.value++
    ElMessage({
      message: `🔔 ${notification.title}: ${notification.message}`,
      type: notification.type.toLowerCase(),
      duration: 4000
    })
  })

  connection.start().catch(console.error)
}

onMounted(async () => {
  await loadNotifications(true)
  connectSignalR()
})

onUnmounted(() => connection?.stop())
</script>

<style scoped>
.notification-item {
  display: flex;
  gap: 12px;
  padding: 12px;
  border-radius: 8px;
  cursor: pointer;
  margin-bottom: 8px;
  border: 1px solid #f0f0f0;
  transition: background 0.2s;
}
.notification-item:hover { background: #f5f7fa; }
.notification-item.unread { background: #ecf5ff; border-color: #b3d8ff; }
.notif-icon { margin-top: 2px; font-size: 20px; flex-shrink: 0; }
.notif-title { font-weight: bold; font-size: 14px; margin-bottom: 4px; }
.notif-message { font-size: 13px; color: #666; margin-bottom: 4px; }
.notif-time { font-size: 12px; color: #999; }
</style>